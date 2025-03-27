using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using System.Xml;

namespace Assets.Scripts.DataProviders
{
    public class DigiKamConnector : DataProviderBase
    {
        public List<DigiKamFaceTag> faceTagList;
        private Dictionary<int, int> _ownerIdToTagIdMap;
        private string _rootsMagicDataBaseFileNameWithFullPath;  // usually *.rmtree, *.rmgc, or *.sqlite
        private string _digiKamDataBaseFileNameWithFullPath;     // usually digikam4.db
        private string _rootsMagicToDigiKamDataBaseFileNameWithFullPath;  // usually rootsmagic-digikam.db
        private string _digiKamThumbnailsDataBaseFileNameWithFullPath;  // usually thumbnails-digikam.db

        static string RootsMagic_DigiKam_DataBaseFileNameOnly = "rootsmagic-digikam.db";
        static string DigiKam_Thumbnails_DataBaseFileNameOnly = "thumbnails-digikam.db";

        public DigiKamConnector(string RootMagicDataBaseFileName, string DigiKamDataBaseFileName)           
        {
            _rootsMagicDataBaseFileNameWithFullPath = RootMagicDataBaseFileName;
            _digiKamDataBaseFileNameWithFullPath = DigiKamDataBaseFileName;
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            _rootsMagicToDigiKamDataBaseFileNameWithFullPath = justThePath + "\\" + RootsMagic_DigiKam_DataBaseFileNameOnly;
            _digiKamThumbnailsDataBaseFileNameWithFullPath = justThePath + "\\" + DigiKam_Thumbnails_DataBaseFileNameOnly;
            faceTagList = new List<DigiKamFaceTag>();
            _ownerIdToTagIdMap = new Dictionary<int, int>();
            BuildOwnerIdToTagIdMap();
        }

        private void BuildOwnerIdToTagIdMap()
        {
            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT t.id as tagId, 
                               CAST(p.value as INTEGER) as ownerId
                        FROM Tags t
                        JOIN TagProperties p ON t.id = p.tagid
                        WHERE p.property = 'rootsmagic_owner_id'";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int tagId = reader.GetInt32(0);
                            int ownerId = reader.GetInt32(1);
                            _ownerIdToTagIdMap[ownerId] = tagId;
                        }
                    }
                }
            }
        }

        public int GetTagIdForOwnerId(int ownerId)
        {
            return _ownerIdToTagIdMap.TryGetValue(ownerId, out int tagId) ? tagId : -1;
        }

        public bool AreAllDatabaseFilesPresent()
        {
            if (!System.IO.File.Exists(_rootsMagicDataBaseFileNameWithFullPath)) {
                Debug.Log($"RootsMagic database file not found. {_rootsMagicDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!System.IO.File.Exists(_digiKamDataBaseFileNameWithFullPath)) {
                Debug.Log($"Base DigiKam database file not found. {_digiKamDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!DoesThisDBFileExistInDigiKamFolder(DigiKam_Thumbnails_DataBaseFileNameOnly)) {
                Debug.Log($"DigiKam Thumbnail database file not found. {DigiKam_Thumbnails_DataBaseFileNameOnly}");
                return false;
            }
            return true;    
        }

        private bool DoesThisDBFileExistInDigiKamFolder(string filename)
        {
            var folderOnly = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            var filenameToCheck = folderOnly + "\\" + filename;
            return System.IO.File.Exists(filenameToCheck);
        }

        private void AttachThumbnailsDatabase(IDbCommand dbcmd)
        {
            // First check if already attached
            dbcmd.CommandText = "SELECT name FROM pragma_database_list WHERE name = 'thumbnails-digikam';";
            bool isAlreadyAttached = false;
            using (var reader = dbcmd.ExecuteReader())
            {
                isAlreadyAttached = reader.Read();
            }

            // Attach the thumbnails database if not already attached
            if (!isAlreadyAttached)
            {
                dbcmd.CommandText = $"ATTACH DATABASE '{_digiKamThumbnailsDataBaseFileNameWithFullPath}' as 'thumbnails-digikam';";
                dbcmd.ExecuteNonQuery();
            }
        }

        public byte[] GetPrimaryThumbnailForPersonFromDataBase(int ownerId)
        {
            byte[] imageToReturn = null;

            // Get the tag ID for this owner ID
            int tagId = GetTagIdForOwnerId(ownerId);
            if (tagId == -1)
            {
                Debug.LogWarning($"No tag found for owner ID {ownerId}");
                return null;
            }

            int limitListSizeTo = 1;
            string conn = "URI=file:" + _digiKamDataBaseFileNameWithFullPath;
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();

            AttachThumbnailsDatabase(dbcmd);

            string QUERYTHUMBNAILS =
                "SELECT tags.id as tagId, tags.name, paths.thumbId, images.id as \"imageId\",\n" +
                "tnails.type, tnails.modificationDate, tnails.orientationHint,\n" +
                "\"C:\" || (SELECT specificPath FROM AlbumRoots WHERE AlbumRoots.label = \"Photos\") ||\n" +
                "albums.relativePath || images.name as \"fullPathToFileName\",\n" +
                "region.value as \"region\", tnails.data as 'PGFImageData'\n" +
                "FROM Tags tags\n" +
                "LEFT JOIN Images images ON tags.icon = images.id\n" +
                "LEFT JOIN Albums albums ON images.album = albums.id\n" +
                "LEFT JOIN ImageTagProperties region ON tags.icon = region.imageid AND tags.id = region.tagid\n" +
                "INNER JOIN [thumbnails-digikam].FilePaths paths ON fullPathToFileName = paths.path\n" +
                "INNER JOIN [thumbnails-digikam].Thumbnails tnails ON paths.thumbId = tnails.id\n" +
                $"WHERE tags.id = {tagId} AND images.album IS NOT NULL;";

            string sqlQuery = QUERYTHUMBNAILS;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                // now read in the string value for region
                string region = (string)reader["region"];
                Debug.Log($"Region: {region}");
                // Parse XML string
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(region);
                var rectElement = doc.DocumentElement;

                // Create Rect from XML attributes
                Rect faceRegion = new Rect(
                    float.Parse(rectElement.GetAttribute("x")),
                    float.Parse(rectElement.GetAttribute("y")),
                    float.Parse(rectElement.GetAttribute("width")),
                    float.Parse(rectElement.GetAttribute("height"))
                );
                
                if (System.IO.File.Exists(pathToFullResolutionImage)) {
                    try {
                        // Load the full image into a byte array
                        byte[] fullImageBytes = System.IO.File.ReadAllBytes(pathToFullResolutionImage);
                        
                        // Create a texture from the bytes
                        Texture2D fullTexture = new Texture2D(2, 2);
                        fullTexture.LoadImage(fullImageBytes);
                        
                        // convert float actual values to integer
                        int x = Mathf.RoundToInt(faceRegion.x);
                        int y = Mathf.RoundToInt(faceRegion.y);
                        int width = Mathf.RoundToInt(faceRegion.width);
                        int height = Mathf.RoundToInt(faceRegion.height);
                        
                        // Create a new texture for the cropped region
                        Texture2D croppedTexture = new Texture2D(width, height);
                        
                        // Copy the pixels from the region we want
                        Color[] pixels = fullTexture.GetPixels(x, y, width, height);
                        croppedTexture.SetPixels(0, 0, width, height, pixels);
                        croppedTexture.Apply();
                        
                        // Convert back to bytes
                        imageToReturn = croppedTexture.EncodeToPNG();
                        
                        // Clean up
                        if (Application.isPlaying)
                        {
                            UnityEngine.Object.Destroy(fullTexture);
                            UnityEngine.Object.Destroy(croppedTexture);
                        }
                        else
                        {
                            UnityEngine.Object.DestroyImmediate(fullTexture);
                            UnityEngine.Object.DestroyImmediate(croppedTexture);
                        }
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Error reading image file {pathToFullResolutionImage}: {ex.Message}");
                        imageToReturn = null;
                    }
                }
                else {
                    Debug.LogWarning($"Image file not found: {pathToFullResolutionImage}");
                    imageToReturn = null;
                }

                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            return imageToReturn;
        }
    }
}
