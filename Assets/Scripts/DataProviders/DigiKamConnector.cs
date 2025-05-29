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
    /// <summary>
    /// Connector class for interacting with DigiKam photo database and RootsMagic database.
    /// Handles retrieval of photo information and face tags for persons in the family tree.
    /// </summary>
    public class DigiKamConnector : DataProviderBase
    {
        public List<DigiKamFaceTag> faceTagList;
        private Dictionary<int, int> _ownerIdToTagIdMap;
        private string _rootsMagicDataBaseFileNameWithFullPath;  // usually *.rmtree, *.rmgc, or *.sqlite
        private string _digiKamDataBaseFileNameWithFullPath;     // usually digikam4.db
        private string _digiKamThumbnailsDataBaseFileNameWithFullPath;  // usually thumbnails-digikam.db
        static string DigiKam_Thumbnails_DataBaseFileNameOnly = "thumbnails-digikam.db";

        /// <summary>
        /// Initializes a new instance of the DigiKamConnector with paths to the RootsMagic and DigiKam databases.
        /// </summary>
        /// <param name="RootMagicDataBaseFileName">Full path to the RootsMagic database file</param>
        /// <param name="DigiKamDataBaseFileName">Full path to the DigiKam database file</param>
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

        /// <summary>
        /// Builds a mapping between RootsMagic owner IDs and DigiKam tag IDs.
        /// </summary>
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

        /// <summary>
        /// Gets the DigiKam tag ID associated with a RootsMagic owner ID.
        /// </summary>
        /// <param name="ownerId">The RootsMagic owner ID to look up</param>
        /// <returns>The corresponding DigiKam tag ID, or -1 if not found</returns>
        public int GetTagIdForOwnerId(int ownerId)
        {
            return _ownerIdToTagIdMap.TryGetValue(ownerId, out int tagId) ? tagId : -1;
        }

        /// <summary>
        /// Checks if all required database files are present in the expected locations.
        /// </summary>
        /// <returns>True if all required database files exist, false otherwise</returns>
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

        /// <summary>
        /// Checks if a specific database file exists in the DigiKam folder.
        /// </summary>
        /// <param name="filename">The name of the database file to check</param>
        /// <returns>True if the file exists, false otherwise</returns>
        private bool DoesThisDBFileExistInDigiKamFolder(string filename)
        {
            var folderOnly = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            var filenameToCheck = folderOnly + "\\" + filename;
            return System.IO.File.Exists(filenameToCheck);
        }

        /// <summary>
        /// Attaches the DigiKam thumbnails database to the current database connection if not already attached.
        /// </summary>
        /// <param name="dbcmd">The database command object to use for the attachment</param>
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

        /// <summary>
        /// Retrieves the primary thumbnail photo information for a person from the database.
        /// </summary>
        /// <param name="ownerId">The RootsMagic owner ID of the person</param>
        /// <returns>A PhotoInfo object containing the photo details, or null if no photo is found</returns>
        public PhotoInfo GetPhotoInfoForPrimaryThumbnailForPersonFromDataBase(int ownerId)
        {
            PhotoInfo photoInfo = null;
            // Get the tag ID for this owner ID
            int tagId = GetTagIdForOwnerId(ownerId);
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
                var imageWidth = (float)((reader["imageWidth"] as Int64?) ?? 0);
                var imageHeight = (float)((reader["imageHeight"] as Int64?) ?? 0);
                // Parse XML string
                Rect faceRegion = ImageUtils.ParseRegionXml(region, imageWidth, imageHeight);
                // orientation is an INT64 in the DB
                var orient64 = reader["orientation"] as Int64?;
                int orientation = (int)orient64;
                // I want to bound the orientation to a valid ExifOrientation enum value
                orientation = (int)Mathf.Clamp(orientation, 1, 8);  
                var exitOrientation = (ExifOrientation)orientation;
                if (!string.IsNullOrEmpty(fullPathToFileName))
                {
                    var tagIdFromQuery = reader["tagId"] as Int64?;
                    int tagIdInt = (int)(tagIdFromQuery ?? -1);
                    photoInfo = new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, tagId: tagIdInt);
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
        /// Retrieves a list of all photo information for a person from the database.
        /// </summary>
        /// <param name="ownerId">The RootsMagic owner ID of the person</param>
        /// <returns>A list of PhotoInfo objects containing the photo details</returns>
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
                                photoList.Add(new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, tagId: tagId));
                            }
                        }
                    }
                }
            }

            return photoList;
        }
    }
}
