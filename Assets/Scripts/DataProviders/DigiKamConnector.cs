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
        public int LocationsTagId { get; private set; }
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
        /// <param name="locationsTagName">Name of the base locations tag (default: "Locations")</param>
        public DigiKamConnector(string RootMagicDataBaseFileName, string DigiKamDataBaseFileName, string locationsTagName = "Locations")           
        {
            _rootsMagicDataBaseFileNameWithFullPath = RootMagicDataBaseFileName;
            _digiKamDataBaseFileNameWithFullPath = DigiKamDataBaseFileName;
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            _digiKamThumbnailsDataBaseFileNameWithFullPath = justThePath + "\\" + DigiKam_Thumbnails_DataBaseFileNameOnly;
            faceTagList = new List<DigiKamFaceTag>();
            _ownerIdToTagIdMap = new Dictionary<int, int>();
            BuildOwnerIdToTagIdMap();
            LocationsTagId = GetBaseTagIdByName(locationsTagName);
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
        /// Retrieves the tag ID for a base tag (pid = 0) with the specified name.
        /// </summary>
        /// <param name="tagName">The name of the base tag to find</param>
        /// <returns>The tag ID if found, or -1 if not found</returns>
        private int GetBaseTagIdByName(string tagName)
        {
            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT id 
                        FROM Tags 
                        WHERE name = @tagName AND (pid = 0 OR pid IS NULL)";
                    
                    var parameter = new SqliteParameter("@tagName", tagName);
                    cmd.Parameters.Add(parameter);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
            }
            
            Debug.LogWarning($"Base tag with name '{tagName}' not found");
            return -1;
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
        /// Retrieves all tags associated with an image from the DigiKam database.
        /// </summary>
        /// <param name="imageId">The image ID to get tags for</param>
        /// <returns>A dictionary of tags keyed by TagId</returns>
        private Dictionary<int, PhotoTag> GetTagsForImage(int imageId)
        {
            var tags = new Dictionary<int, PhotoTag>();

            if (imageId <= 0)
            {
                return tags; // Return empty dictionary for invalid image IDs
            }

            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT t.id as TagId, 
                               t.name as TagName,
                               COALESCE(t.pid, 0) as ParentTagId
                        FROM ImageTags it
                        JOIN Tags t ON it.tagid = t.id
                        WHERE it.imageid = @imageId";
                    
                    var parameter = new SqliteParameter("@imageId", imageId);
                    cmd.Parameters.Add(parameter);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int tagId = reader.GetInt32("TagId");
                            string tagName = reader.GetString("TagName");
                            int parentTagId = reader.GetInt32("ParentTagId");
                            
                            tags[tagId] = new PhotoTag(tagName, tagId, parentTagId);
                        }
                        // Add the base Locations tag, this is were the Reverse Geocoding is stored
                        tags[LocationsTagId] = new PhotoTag("Locations", LocationsTagId, 0);
                    }
                }
            }

            return tags;
        }

        /// <summary>
        /// Retrieves and assembles the description for an image from the ImageComments table.
        /// Combines Description (type 3), Headline (type 2), and Comments (type 1) with ". " separator.
        /// </summary>
        /// <param name="imageId">The image ID to get description for</param>
        /// <returns>Assembled description string, or null if no comments found</returns>
        private string GetDescriptionForImage(int imageId)
        {
            if (imageId <= 0)
            {
                return null;
            }

            string description = null;
            string headline = null;
            var comments = new List<string>();

            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT type, comment
                        FROM ImageComments
                        WHERE imageid = @imageId
                        ORDER BY type, id";
                    
                    var parameter = new SqliteParameter("@imageId", imageId);
                    cmd.Parameters.Add(parameter);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int type = reader.GetInt32("type");
                            string comment = reader.GetString("comment");
                            
                            if (!string.IsNullOrWhiteSpace(comment))
                            {
                                switch (type)
                                {
                                    case 1: // Comments
                                        comments.Add(comment.Trim());
                                        break;
                                    case 2: // Headline
                                        headline = comment.Trim();
                                        break;
                                    case 3: // Description
                                        description = comment.Trim();
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            // Assemble the final description string
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(description))
                parts.Add(description);
            
            if (!string.IsNullOrEmpty(headline))
                parts.Add(headline);
            
            if (comments.Count > 0)
                parts.AddRange(comments);

            return parts.Count > 0 ? string.Join(". ", parts) : null;
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
                    info.rating as imageRating,
                    info.creationDate as creationDate,
                    info.digitizationDate as digitizationDate,
                    region.value as ""region"",
                    tnails.data as 'PGFImageData',
                    metadata.make as cameraMake,
                    metadata.model as cameraModel,
                    metadata.lens as cameraLens,
                    positions.latitudeNumber as positionLatitude,
                    positions.longitudeNumber as positionLongitude,
                    positions.altitude as positionAltitude
                FROM Tags tags
                LEFT JOIN Images images ON tags.icon = images.id
                LEFT JOIN ImageInformation info ON images.id = info.imageid
                LEFT JOIN ImageMetadata metadata ON images.id = metadata.imageid
                LEFT JOIN ImagePositions positions ON images.id = positions.imageid
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
                var imageRating = (int)((reader["imageRating"] as Int64?) ?? 0);
                
                // Handle datetime fields
                DateTime? creationDate = null;
                DateTime? digitizationDate = null;
                if (reader["creationDate"] != DBNull.Value && reader["creationDate"] != null)
                {
                    if (DateTime.TryParse(reader["creationDate"].ToString(), out DateTime creation))
                        creationDate = creation;
                }
                if (reader["digitizationDate"] != DBNull.Value && reader["digitizationDate"] != null)
                {
                    if (DateTime.TryParse(reader["digitizationDate"].ToString(), out DateTime digitization))
                        digitizationDate = digitization;
                }
                
                // Handle camera metadata
                string cameraMake = reader["cameraMake"] as string ?? "";
                string cameraModel = reader["cameraModel"] as string ?? "";
                string cameraLens = reader["cameraLens"] as string ?? "";
                
                // Handle GPS coordinates
                float positionLatitude = (float)((reader["positionLatitude"] as float?) ?? 0.0f);
                float positionLongitude = (float)((reader["positionLongitude"] as float?) ?? 0.0f);
                float positionAltitude = (float)((reader["positionAltitude"] as float?) ?? 0.0f);
                
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
                    var imageId = (int)((reader["imageId"] as Int64?) ?? -1);
                    
                    // Get all tags for this image
                    var imageTags = GetTagsForImage(imageId);
                    
                    // Get description for this image
                    var imageDescription = GetDescriptionForImage(imageId);
                    
                    photoInfo = new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, 
                                            tagId: tagIdInt, imageId: imageId, imageRating: imageRating,
                                            creationDate: creationDate, digitizationDate: digitizationDate,
                                            cameraMake: cameraMake, cameraModel: cameraModel, cameraLens: cameraLens,
                                            positionLatitude: positionLatitude, positionLongitude: positionLongitude, 
                                            positionAltitude: positionAltitude, tags: imageTags, description: imageDescription);
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
        /// <param name="yearFilter">Optional year filter for filtering photos (null for all, -1 for no creation date, int for specific year)</param>
        /// <returns>A list of PhotoInfo objects containing the photo details</returns>
        public List<PhotoInfo> GetPhotoInfoListForPersonFromDataBase(int ownerId, int? yearFilter = null)
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
                            images.id as ""imageId"",
                            region.value as ""region"",
                            info.width as imageWidth,
                            info.height as imageHeight,
                            info.orientation as orientation,
                            info.rating as imageRating,
                            info.creationDate as creationDate,
                            info.digitizationDate as digitizationDate,
                            metadata.make as cameraMake,
                            metadata.model as cameraModel,
                            metadata.lens as cameraLens,
                            positions.latitudeNumber as positionLatitude,
                            positions.longitudeNumber as positionLongitude,
                            positions.altitude as positionAltitude
                      FROM ImageTags imagetags
                      LEFT JOIN Images images ON imagetags.imageid = images.id
                      LEFT JOIN ImageInformation info ON images.id = info.imageid 
                      LEFT JOIN ImageMetadata metadata ON images.id = metadata.imageid
                      LEFT JOIN ImagePositions positions ON images.id = positions.imageid
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
                            var imageRating = (int)((reader["imageRating"] as Int64?) ?? 0);
                            var imageId = (int)((reader["imageId"] as Int64?) ?? -1);
                            
                            // Handle datetime fields
                            DateTime? creationDate = null;
                            DateTime? digitizationDate = null;
                            if (reader["creationDate"] != DBNull.Value && reader["creationDate"] != null)
                            {
                                if (DateTime.TryParse(reader["creationDate"].ToString(), out DateTime creation))
                                    creationDate = creation;
                            }
                            if (reader["digitizationDate"] != DBNull.Value && reader["digitizationDate"] != null)
                            {
                                if (DateTime.TryParse(reader["digitizationDate"].ToString(), out DateTime digitization))
                                    digitizationDate = digitization;
                            }
                            
                            // Apply year filtering logic
                            bool includePhoto = true;
                            if (yearFilter.HasValue)
                            {
                                if (yearFilter.Value == -1)
                                {
                                    // Option 3: Return only photos with null/empty creation date
                                    includePhoto = !creationDate.HasValue;
                                }
                                else
                                {
                                    // Option 2: Return only photos matching the year filter
                                    includePhoto = creationDate.HasValue && creationDate.Value.Year == yearFilter.Value;
                                }
                            }
                            // Option 1: If yearFilter is null, includePhoto remains true (return all)
                            
                            if (!includePhoto)
                                continue;
                            
                            // Handle camera metadata
                            string cameraMake = reader["cameraMake"] as string ?? "";
                            string cameraModel = reader["cameraModel"] as string ?? "";
                            string cameraLens = reader["cameraLens"] as string ?? "";
                            
                            // Handle GPS coordinates
                            float positionLatitude = (float)((reader["positionLatitude"] as float?) ?? 0.0f);
                            float positionLongitude = (float)((reader["positionLongitude"] as float?) ?? 0.0f);
                            float positionAltitude = (float)((reader["positionAltitude"] as float?) ?? 0.0f);
                            
                            // Parse XML string
                            Rect faceRegion = ImageUtils.ParseRegionXml(region, imageWidth, imageHeight);
                            var orient64 = reader["orientation"] as Int64?;
                            // orientation is an INT64 in the DB
                            int orientation = (int)orient64;
                            // I want to bound the orientation to a valid ExifOrientation enum value
                            orientation = (int)Mathf.Clamp(orientation, 1, 8);  
                            var exitOrientation = (ExifOrientation)orientation;
                           
                            if (!string.IsNullOrEmpty(fullPathToFileName))
                            {
                                // Get all tags for this image
                                var imageTags = GetTagsForImage(imageId);
                                
                                // Get description for this image
                                var imageDescription = GetDescriptionForImage(imageId);
                                
                                photoList.Add(new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, 
                                                          tagId: tagId, imageId: imageId, imageRating: imageRating,
                                                          creationDate: creationDate, digitizationDate: digitizationDate,
                                                          cameraMake: cameraMake, cameraModel: cameraModel, cameraLens: cameraLens,
                                                          positionLatitude: positionLatitude, positionLongitude: positionLongitude, 
                                                          positionAltitude: positionAltitude, tags: imageTags, description: imageDescription));
                            }
                        }
                    }
                }
            }

            return photoList;
        }
    }
}
