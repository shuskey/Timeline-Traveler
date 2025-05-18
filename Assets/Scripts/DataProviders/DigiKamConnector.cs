using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using System.Xml;
using Assets.Scripts.Utilities;

namespace Assets.Scripts.DataProviders
{
    public class DigiKamConnector : DataProviderBase
    {

        public List<DigiKamFaceTag> faceTagList;
        private Dictionary<int, int> _ownerIdToTagIdMap;
        private string _rootsMagicDataBaseFileNameWithFullPath;  // usually *.rmtree, *.rmgc, or *.sqlite
        private string _digiKamDataBaseFileNameWithFullPath;     // usually digikam4.db
        private string _digiKamThumbnailsDataBaseFileNameWithFullPath;  // usually thumbnails-digikam.db
        static string DigiKam_Thumbnails_DataBaseFileNameOnly = "thumbnails-digikam.db";

        public DigiKamConnector(string RootMagicDataBaseFileName, string DigiKamDataBaseFileName)           
        {
            _rootsMagicDataBaseFileNameWithFullPath = RootMagicDataBaseFileName;
            _digiKamDataBaseFileNameWithFullPath = DigiKamDataBaseFileName;
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
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
                Debug.LogError($"RootsMagic database file not found. {_rootsMagicDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!System.IO.File.Exists(_digiKamDataBaseFileNameWithFullPath)) {
                Debug.LogError($"Base DigiKam database file not found. {_digiKamDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!DoesThisDBFileExistInDigiKamFolder(DigiKam_Thumbnails_DataBaseFileNameOnly)) {
                Debug.LogError($"DigiKam Thumbnail database file not found. {DigiKam_Thumbnails_DataBaseFileNameOnly}");
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

        //This is Deprecated, use GetPhotoInfoForPrimaryThumbnailForPersonFromDataBase instead
        public byte[] Deprecated_GetPrimaryThumbnailForPersonFromDataBase(int ownerId)
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
            // A note about AlbumRoots:
            // My usage of DigiKam I have seen the label be "Photos" as well as "Pictures"
            // So, I will just use the id of 1 for the AlbumRoot
            string QUERYTHUMBNAILS = $@"
                SELECT 
                    tags.id as tagId,
                    tags.name,
                    paths.thumbId,
                    images.id as ""imageId"",
                    tnails.type,
                    tnails.modificationDate,
                    tnails.orientationHint as ""orientation"",
                    'C:' || 
                    (SELECT specificPath FROM AlbumRoots WHERE AlbumRoots.id = 1) ||
                        CASE
                            WHEN albums.relativePath = '/' THEN '/'
                            ELSE albums.relativePath || '/'
                        END || 
                    TRIM(images.name, '/') as ""fullPathToFileName"",
                    region.value as ""region"",
                    tnails.data as 'PGFImageData'
                FROM Tags tags
                LEFT JOIN Images images 
                    ON tags.icon = images.id
                LEFT JOIN Albums albums 
                    ON images.album = albums.id
                LEFT JOIN ImageTagProperties region 
                    ON tags.icon = region.imageid 
                    AND tags.id = region.tagid
                INNER JOIN [thumbnails-digikam].FilePaths paths 
                    ON fullPathToFileName = paths.path
                INNER JOIN [thumbnails-digikam].Thumbnails tnails 
                    ON paths.thumbId = tnails.id
                WHERE tags.id = {tagId} 
                    AND images.album IS NOT NULL;";

            string sqlQuery = QUERYTHUMBNAILS;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                // now read in the string value for region
                string region = (string)reader["region"];
                var orient64 = reader["orientation"] as Int64?;
                // orientation is an INT64 in the DB
                int orientation = (int)orient64;
                // Parse XML string
                Rect faceRegion = ImageUtils.ParseRegionXml(region, 0, 0);

                imageToReturn = LoadAndProcessImage(pathToFullResolutionImage, faceRegion, orientation);
 
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
        public PhotoInfo GetPhotoInfoForPrimaryThumbnailForPersonFromDataBase(int ownerId, bool verbose = false)
        {
            // if verbose is true, we will log the SQL query and all results
            if (verbose)    {
                Debug.Log($"Gettering ready to get Thumbnail for Person {ownerId} with an SQL Query");
            }
            PhotoInfo photoInfo = null;
            // Get the tag ID for this owner ID
            int tagId = GetTagIdForOwnerId(ownerId);
            if (verbose)    {
                Debug.Log($"Tag ID for owner ID {ownerId} is {tagId}");
            }
            if (tagId == -1)
            {
                Debug.LogWarning($"No tag found for owner ID {ownerId}");
                return photoInfo;
            }

            int limitListSizeTo = 1;
            string conn = "URI=file:" + _digiKamDataBaseFileNameWithFullPath;
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();

            AttachThumbnailsDatabase(dbcmd);
            // A note about AlbumRoots:
            // My usage of DigiKam I have seen the label be "Photos" as well as "Pictures"
            // So, I will just use the id of 1 for the AlbumRoot
            string QUERYTHUMBNAILS = $@"
                SELECT 
                    tags.id as tagId,
                    tags.name,
                    paths.thumbId,
                    images.id as ""imageId"",
                    tnails.type,
                    tnails.modificationDate,
                    tnails.orientationHint as orientation,
                    'C:' || 
                    (SELECT specificPath FROM AlbumRoots WHERE AlbumRoots.id = 1) ||
                        CASE
                            WHEN albums.relativePath = '/' THEN '/'
                            ELSE albums.relativePath || '/'
                        END || 
                    TRIM(images.name, '/') as ""fullPathToFileName"",
                    info.width as imageWidth,
                    info.height as imageHeight,
                    region.value as ""region"",
                    tnails.data as 'PGFImageData'
                FROM Tags tags
                LEFT JOIN Images images ON tags.icon = images.id
                LEFT JOIN ImageInformation info ON images.id = info.imageid
                LEFT JOIN Albums albums ON images.album = albums.id
                LEFT JOIN ImageTagProperties region ON tags.icon = region.imageid AND tags.id = region.tagid
                INNER JOIN [thumbnails-digikam].FilePaths paths ON fullPathToFileName = paths.path
                INNER JOIN [thumbnails-digikam].Thumbnails tnails ON paths.thumbId = tnails.id
                WHERE tags.id = {tagId} 
                    AND images.album IS NOT NULL;";

            string sqlQuery = QUERYTHUMBNAILS;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo) { 
                string fullPathToFileName = reader["fullPathToFileName"] as string;
                string region = reader["region"] as string;
                if (verbose)    {
                    Debug.Log($"Current Array Index: {currentArrayIndex}");
                    Debug.Log($"Full Path to File Name: {fullPathToFileName}");
                    Debug.Log($"Region string: {region}");
                }
                var imageWidth = (float)((reader["imageWidth"] as Int64?) ?? 0);
                var imageHeight = (float)((reader["imageHeight"] as Int64?) ?? 0);
                if (verbose)    {
                    Debug.Log($"Image Width: {imageWidth}, Image Height: {imageHeight}");
                }
                // Parse XML string
                Rect faceRegion = ImageUtils.ParseRegionXml(region, imageWidth, imageHeight, verbose);
                // orientation is an INT64 in the DB
                var orient64 = reader["orientation"] as Int64?;
                int orientation = (int)orient64;
                if (verbose)    {
                    Debug.Log($"Int orientation: {orientation}");
                }
                // I want to bound the orientation to a valid ExifOrientation enum value
                orientation = (int)Mathf.Clamp(orientation, 1, 8);  
                var exitOrientation = (ExifOrientation)orientation;
                if (verbose)    {
                    Debug.Log($"ExifOrientation: {exitOrientation}");
                }
                if (!string.IsNullOrEmpty(fullPathToFileName))
                {
                    photoInfo = new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation);
                }
                else
                {
                    Debug.LogWarning($"Primary Thumbnail: No full path to file name found for owner ID {ownerId}");
                }
                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            return photoInfo;
        }

        /// <summary>
        /// Loads and processes an image file, applying face region cropping and orientation.
        /// </summary>
        /// <param name="pathToFullResolutionImage">Full path to the image file</param>
        /// <param name="faceRegion">The region of the face to crop</param>
        /// <param name="orientation">The EXIF orientation value</param>
        /// <returns>Processed image as byte array, or null if processing fails</returns>
        private byte[] LoadAndProcessImage(string pathToFullResolutionImage, Rect faceRegion, int orientation)
        {
            if (!System.IO.File.Exists(pathToFullResolutionImage))
            {
                Debug.LogWarning($"Image file not found: {pathToFullResolutionImage}");
                return null;
            }

            try
            {
                // Load the full image into a byte array
                byte[] fullImageBytes = System.IO.File.ReadAllBytes(pathToFullResolutionImage);
                return ProcessAndCropImage(fullImageBytes, faceRegion, orientation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading image file {pathToFullResolutionImage}: {ex.Message}");
                return null;
            }
        }
        private byte[] ProcessAndCropImage(byte[] fullImageBytes, Rect faceRegion, int orientation)
        {
            // Create a texture from the bytes
            Texture2D fullTexture = new Texture2D(2, 2);
            fullTexture.LoadImage(fullImageBytes);
            
            // Get a square bounded region for the face
            RectInt squareRegion = ImageUtils.GetSquareBoundedRegion(fullTexture, faceRegion);

            //The image need EXIF orientation applied to it
         //   fullTexture = ImageUtils.ApplyExifOrientation(fullTexture, orientation);
            
            // Create a new texture for the square cropped region
            Texture2D croppedTexture = new Texture2D(squareRegion.width, squareRegion.height);

            //We need to flip the image vertically because the coo
            int flippedY = fullTexture.height - (squareRegion.y + squareRegion.height);
            
            // Copy the pixels from the square region
            Color[] pixels = fullTexture.GetPixels(squareRegion.x, flippedY, 
                                                 squareRegion.width, squareRegion.height);
            croppedTexture.SetPixels(0, 0, squareRegion.width, squareRegion.height, pixels);
            croppedTexture.Apply();
            
            // Convert back to bytes
            byte[] processedImage = croppedTexture.EncodeToPNG();
            
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

            return processedImage;
        }

        public List<PhotoInfo> GetPhotoInfoListForPersonFromDataBase(int ownerId)
        {
            List<PhotoInfo> photoList = new List<PhotoInfo>();

            // Get the tag ID for this owner ID
            int tagId = GetTagIdForOwnerId(ownerId);
            if (tagId == -1)
            {
                Debug.LogWarning($"No tag found for owner ID {ownerId}");
                return photoList;
            }

            string conn = "URI=file:" + _digiKamDataBaseFileNameWithFullPath;
            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    string sqlQuery = $@"
                        SELECT 
                            ""C:"" || (
                            SELECT specificPath 
                            FROM AlbumRoots 
                            WHERE id = 1)
                              || 
                              CASE
                                  WHEN albums.relativePath = '/' THEN '/'
                                  ELSE albums.relativePath || '/'
                              END
                              || images.name 
                            as ""fullPathToFileName"",
                            region.value as ""region"",
                            info.width as imageWidth,
                            info.height as imageHeight,
                            info.orientation as orientation
                      FROM ImageTags imagetags
                      LEFT JOIN Images images ON imagetags.imageid = images.id
                      LEFT JOIN ImageInformation info ON images.id = info.imageid 
                      LEFT JOIN Albums albums ON images.album = albums.id
                      LEFT JOIN ImageTagProperties region ON imagetags.imageid = region.imageid AND imagetags.tagid = region.tagid 
                      WHERE imagetags.tagid={tagId}";

                    dbcmd.CommandText = sqlQuery;
                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fullPathToFileName = reader["fullPathToFileName"] as string;
                            string region = reader["region"] as string;
                            var imageWidth = (float)((reader["imageWidth"] as Int64?) ?? 0);
                            var imageHeight = (float)((reader["imageHeight"] as Int64?) ?? 0);
                            
                            Rect faceRegion = ImageUtils.ParseRegionXml(region, imageWidth, imageHeight);
                            var orient64 = reader["orientation"] as Int64?;
                            // orientation is an INT64 in the DB
                            int orientation = (int)orient64;
                            // I want to bound the orientation to a valid ExifOrientation enum value
                            orientation = (int)Mathf.Clamp(orientation, 1, 8);  
                            var exitOrientation = (ExifOrientation)orientation;
                           
                            if (!string.IsNullOrEmpty(fullPathToFileName))
                            {
                                photoList.Add(new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation));
                            }
                        }
                    }
                }
            }

            return photoList;
        }
    }
}
