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
        // Timeline-Traveler tag constants
        public static readonly string TIMELINE_TRAVELER_PARENT_TAG = "Timeline-Traveler";
        public static readonly string IS_NOT_DATED_TAG = "IsNotDated";
        public static readonly string IS_PRIVATE_TAG = "IsPrivate";
        public static readonly string HAS_TODO_CAPTION_TAG = "HasTodoCaption";
        public static readonly string DIGIKAM_TODO_AUTHOR = "DigiKam Todo";

        // Database and property constants
        public static readonly string ROOTSMAGIC_OWNER_ID_PROPERTY = "rootsmagic_owner_id";
        public static readonly string THUMBNAILS_DIGIKAM_DB_NAME = "thumbnails-digikam";
        public static readonly string LOCATIONS_PARENT_TAG = "Locations";

        // Default folder names to exclude from photo results (converted duplicates)
        public static readonly List<string> DefaultExcludedFolderNames = new List<string> 
        { 
            "BMP_Originals", 
            "HEIC_Originals" 
        };

        public List<DigiKamFaceTag> faceTagList;
        public int LocationsTagId { get; private set; }
        private Dictionary<int, int> _ownerIdToTagIdMap;
        private string _rootsMagicDataBaseFileNameWithFullPath;  // usually *.rmtree, *.rmgc, or *.sqlite
        private string _digiKamDataBaseFileNameWithFullPath;     // usually digikam4.db
        private string _digiKamThumbnailsDataBaseFileNameWithFullPath;  // usually thumbnails-digikam.db
        private List<string> _excludedFolderNames; // Folder names to exclude from photo results
        static string DigiKam_Thumbnails_DataBaseFileNameOnly = "thumbnails-digikam.db";

        /// <summary>
        /// Initializes a new instance of the DigiKamConnector with paths to the RootsMagic and DigiKam databases.
        /// </summary>
        /// <param name="RootMagicDataBaseFileName">Full path to the RootsMagic database file</param>
        /// <param name="DigiKamDataBaseFileName">Full path to the DigiKam database file</param>
        /// <param name="locationsTagName">Name of the base locations tag (default: "Locations")</param>
        /// <param name="excludedFolderNames">List of folder names to exclude from photo results (default: BMP_Originals, HRIC_Originals)</param>
        public DigiKamConnector(string RootMagicDataBaseFileName, string DigiKamDataBaseFileName, string locationsTagName = "Locations", List<string> excludedFolderNames = null)           
        {
            _rootsMagicDataBaseFileNameWithFullPath = RootMagicDataBaseFileName;
            _digiKamDataBaseFileNameWithFullPath = DigiKamDataBaseFileName;
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            _digiKamThumbnailsDataBaseFileNameWithFullPath = justThePath + "\\" + DigiKam_Thumbnails_DataBaseFileNameOnly;
            faceTagList = new List<DigiKamFaceTag>();
            _ownerIdToTagIdMap = new Dictionary<int, int>();
            
            // Initialize excluded folder names with defaults if not provided
            _excludedFolderNames = excludedFolderNames ?? new List<string>(DefaultExcludedFolderNames);
            // This is a mapping between the RootsMagic owner ID and the DigiKam tag ID
            BuildOwnerIdToTagIdMap();
            // This is a special tag that is used to store the timeline traveler tags IsNotDated IsPrivate and HasTodoCaption   
            EnsureBaseTimelineTravelerTagsAreAvailable();
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
                        WHERE p.property = @propertyName";
                    
                    var propertyParam = new SqliteParameter("@propertyName", ROOTSMAGIC_OWNER_ID_PROPERTY);
                    cmd.Parameters.Add(propertyParam);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int tagId = reader.GetInt32(reader.GetOrdinal("tagId"));
                            int ownerId = reader.GetInt32(reader.GetOrdinal("ownerId"));
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
        /// Checks if a file path should be excluded based on the configured excluded folder names.
        /// </summary>
        /// <param name="filePath">The file path to check</param>
        /// <returns>True if the path should be excluded, false otherwise</returns>
        private bool ShouldExcludePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || _excludedFolderNames == null)
                return false;

            return _excludedFolderNames.Any(folderName => filePath.Contains(folderName));
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
            dbcmd.CommandText = $"SELECT name FROM pragma_database_list WHERE name = '{THUMBNAILS_DIGIKAM_DB_NAME}';";
            bool isAlreadyAttached = false;
            using (var reader = dbcmd.ExecuteReader())
            {
                isAlreadyAttached = reader.Read();
            }

            // Attach the thumbnails database if not already attached
            if (!isAlreadyAttached)
            {
                dbcmd.CommandText = $"ATTACH DATABASE '{_digiKamThumbnailsDataBaseFileNameWithFullPath}' as '{THUMBNAILS_DIGIKAM_DB_NAME}';";
                dbcmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retrieves all tags associated with an image from the DigiKam database.
        /// </summary>
        /// <param name="imageId">The image ID to get tags for</param>
        /// <param name="dbconn">The database connection to use (optional, creates new if not provided)</param>
        /// <returns>A dictionary of tags keyed by TagId</returns>
        private Dictionary<int, PhotoTag> GetLocationTagsForImage(int imageId, IDbConnection dbconn = null)
        {
            var tags = new Dictionary<int, PhotoTag>();

            if (imageId <= 0)
            {
                return tags; // Return empty dictionary for invalid image IDs
            }

            bool shouldCloseConnection = dbconn == null;
            if (dbconn == null)
            {
                dbconn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}");
                dbconn.Open();
            }

            try
            {
                using (var cmd = dbconn.CreateCommand())
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
                            int tagId = reader.GetInt32(0);
                            string tagName = reader.GetString(reader.GetOrdinal("TagName"));
                            int parentTagId = reader.GetInt32(2);
                            
                            tags[tagId] = new PhotoTag(tagName, tagId, parentTagId);
                        }
                        // Add the base Locations tag, this is were the Reverse Geocoding is stored
                        tags[LocationsTagId] = new PhotoTag(LOCATIONS_PARENT_TAG, LocationsTagId, 0);
                    }
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    dbconn.Close();
                    dbconn.Dispose();
                }
            }

            return tags;
        }

        /// <summary>
        /// Retrieves and assembles the description for an image from the ImageComments table.
        /// Combines Description (type 3), Headline (type 2), and Comments (type 1) with ". " separator.
        /// </summary>
        /// <param name="imageId">The image ID to get description for</param>
        /// <param name="dbconn">The database connection to use (optional, creates new if not provided)</param>
        /// <param name="skipTodoCaption">Whether to skip todo captions (default: true)</param>
        /// <returns>Assembled description string, or null if no comments found</returns>
        private string GetDescriptionForImage(int imageId, IDbConnection dbconn = null, bool skipTodoCaption = true)
        {
            if (imageId <= 0)
            {
                return null;
            }

            string description = null;
            string headline = null;
            var comments = new List<string>();

            bool shouldCloseConnection = dbconn == null;
            if (dbconn == null)
            {
                dbconn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}");
                dbconn.Open();
            }

            try
            {
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT type, comment, author
                        FROM ImageComments
                        WHERE imageid = @imageId
                        ORDER BY type, id";
                    
                    var parameter = new SqliteParameter("@imageId", imageId);
                    cmd.Parameters.Add(parameter);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int type = reader.GetInt32(0);
                            string comment = reader.GetString(reader.GetOrdinal("comment"));
                            string author = reader.IsDBNull(2) ? null : reader.GetString(reader.GetOrdinal("author"));
                            
                            if (skipTodoCaption && author == DIGIKAM_TODO_AUTHOR)
                            {
                                continue; // Skip todo captions when requested
                            }
                            
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
            finally
            {
                if (shouldCloseConnection)
                {
                    dbconn.Close();
                    dbconn.Dispose();
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
            
            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
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
                        INNER JOIN [{THUMBNAILS_DIGIKAM_DB_NAME}].FilePaths paths ON fullPathToFileName = paths.path
                        INNER JOIN [{THUMBNAILS_DIGIKAM_DB_NAME}].Thumbnails tnails ON paths.thumbId = tnails.id
                        WHERE tags.id = {tagId} 
                            AND images.album IS NOT NULL;";

                    string sqlQuery = QUERYTHUMBNAILS;
                    dbcmd.CommandText = sqlQuery;
                    
                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
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
                                // Filter out images from excluded folders (e.g., converted duplicates)
                                if (ShouldExcludePath(fullPathToFileName))
                                {
                                    Debug.Log($"Skipping image from excluded folder: {fullPathToFileName}");
                                    continue;
                                }
                                
                                var tagIdFromQuery = reader["tagId"] as Int64?;
                                int tagIdInt = (int)(tagIdFromQuery ?? -1);
                                var imageId = (int)((reader["imageId"] as Int64?) ?? -1);
                                
                                // Get all tags for this image
                                var imageTags = GetLocationTagsForImage(imageId, dbconn);
                                
                                // Get description for this image
                                var imageDescription = GetDescriptionForImage(imageId, dbconn);
                                
                                // Get Timeline-Traveler tag values
                                GetTimelineTravelerTagValues(imageId, out bool isNotDated, out bool isPrivate, out bool hasTodoCaption, out string todoCaptionText, dbconn);
                                
                                photoInfo = new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, 
                                                        tagId: tagIdInt, imageId: imageId, imageRating: imageRating,
                                                        creationDate: creationDate, digitizationDate: digitizationDate,
                                                        cameraMake: cameraMake, cameraModel: cameraModel, cameraLens: cameraLens,
                                                        positionLatitude: positionLatitude, positionLongitude: positionLongitude, 
                                                        positionAltitude: positionAltitude, tags: imageTags, description: imageDescription,
                                                        isNotDated: isNotDated, isPrivate: isPrivate, hasTodoCaption: hasTodoCaption);
                            }
                            else
                            {
                                Debug.LogWarning($"Primary Thumbnail: No full path to file name found for owner ID {ownerId}");
                            }
                            currentArrayIndex++;
                        }
                    }
                }
            }
            
            return photoInfo;
        }

        /// <summary>
        /// Retrieves a list of all photo information for a person from the database.
        /// </summary>
        /// <param name="ownerId">The RootsMagic owner ID of the person</param>
        /// <param name="yearFilter">Optional year filter for filtering photos (null for all, -1 for no creation date or isNotDated, int for specific year and no IsNotDated nor has a creationDate)</param>
        /// <returns>A list of PhotoInfo objects containing the photo details</returns>
        public List<PhotoInfo> GetPhotoInfoListForPersonFromDataBase(int ownerId, int? yearFilter = null)
        {
            List<PhotoInfo> photoList = new List<PhotoInfo>();
            HashSet<int> processedImageIds = new HashSet<int>(); // Track processed images to avoid duplicates

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
                        SELECT DISTINCT
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
                                                        // Get Timeline-Traveler tag values
                            GetTimelineTravelerTagValues(imageId, out bool isNotDated, out bool isPrivate, out bool hasTodoCaption, out string todoCaptionText, dbconn);

                            // Apply year filtering logic
                            bool includePhoto = true;
                            if (yearFilter.HasValue)
                            {
                                // If we pass in -1, we want to return all photos that are not dated
                                if (yearFilter.Value == -1)
                                {
                                    // We will also flip the isNotDated flag to true if the creation date is null
                                    if (!creationDate.HasValue) isNotDated = true;
                                    // Option 3: Return only photos with null/empty creation date
                                    includePhoto = isNotDated;
                                }
                                else
                                {
                                    // Option 2: Return only photos matching the year filter AND IsNotDated is false    
                                    includePhoto = creationDate.HasValue && creationDate.Value.Year == yearFilter.Value && !isNotDated;
                                }
                            }
                            // Option 1: If yearFilter is null, includePhoto remains true (return all)
                            
                            if (!includePhoto)
                                continue;
                            
                            // Skip if we've already processed this image
                            if (processedImageIds.Contains(imageId))
                            {
                                continue;
                            }
                            processedImageIds.Add(imageId);
                            
                            string fullPathToFileName = reader["fullPathToFileName"] as string;
                            if (string.IsNullOrEmpty(fullPathToFileName))
                            {
                                continue;
                            }
                                                     // Filter out images from excluded folders (e.g., converted duplicates)
                            if (ShouldExcludePath(fullPathToFileName))
                            {
                                continue;
                            }
                            string region = reader["region"] as string;
                            var imageWidth = (float)((reader["imageWidth"] as Int64?) ?? 0);
                            var imageHeight = (float)((reader["imageHeight"] as Int64?) ?? 0);
                            var imageRating = (int)((reader["imageRating"] as Int64?) ?? 0);
                            
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
                            // Get all tags for this image
                            var imageTags = GetLocationTagsForImage(imageId, dbconn);
                            
                            // Get description for this image
                            var imageDescription = GetDescriptionForImage(imageId, dbconn);
                                                        
                            photoList.Add(new PhotoInfo(fullPathToFileName, faceRegion, exitOrientation, 
                                                        tagId: tagId, imageId: imageId, imageRating: imageRating,
                                                        creationDate: creationDate, digitizationDate: digitizationDate,
                                                        cameraMake: cameraMake, cameraModel: cameraModel, cameraLens: cameraLens,
                                                        positionLatitude: positionLatitude, positionLongitude: positionLongitude, 
                                                        positionAltitude: positionAltitude, tags: imageTags, description: imageDescription,
                                                        isNotDated: isNotDated, isPrivate: isPrivate, hasTodoCaption: hasTodoCaption, todoCaptionText: todoCaptionText));

                        }
                    }
                }
            }

            return photoList;
        }

        /// <summary>
        /// Ensures that the base Timeline-Traveler tag and its child tags exist in the database.
        /// Creates them if they don't exist, along with their corresponding TagsTree entries.
        /// </summary>
        public void EnsureBaseTimelineTravelerTagsAreAvailable()
        {
            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check if Timeline-Traveler base tag exists
                        int timelineTravelerTagId = GetOrCreateTag(TIMELINE_TRAVELER_PARENT_TAG, 0, transaction);
                        
                        // Check and create child tags
                        int isNotDatedTagId = GetOrCreateTag(IS_NOT_DATED_TAG, timelineTravelerTagId, transaction);
                        int isPrivateTagId = GetOrCreateTag(IS_PRIVATE_TAG, timelineTravelerTagId, transaction);
                        int hasTodoCaptionTagId = GetOrCreateTag(HAS_TODO_CAPTION_TAG, timelineTravelerTagId, transaction);
                        
                        transaction.Commit();
                        Debug.Log("Timeline-Traveler base tags ensured successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error ensuring Timeline-Traveler tags: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an existing tag or creates a new one if it doesn't exist.
        /// Also creates the corresponding TagsTree entries.
        /// </summary>
        /// <param name="tagName">The name of the tag</param>
        /// <param name="parentId">The parent tag ID (0 for root tags)</param>
        /// <param name="transaction">The database transaction to use</param>
        /// <returns>The tag ID (existing or newly created)</returns>
        private int GetOrCreateTag(string tagName, int parentId, IDbTransaction transaction)
        {
            using (var cmd = transaction.Connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                
                // Check if tag already exists
                cmd.CommandText = @"
                    SELECT id 
                    FROM Tags 
                    WHERE name = @tagName AND pid = @parentId";
                
                var nameParam = new SqliteParameter("@tagName", tagName);
                var parentParam = new SqliteParameter("@parentId", parentId);
                cmd.Parameters.Add(nameParam);
                cmd.Parameters.Add(parentParam);
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int existingTagId = reader.GetInt32(0);
                        Debug.Log($"Tag '{tagName}' already exists with ID {existingTagId}");
                        return existingTagId;
                    }
                }
                
                // Tag doesn't exist, create it
                cmd.CommandText = @"
                    INSERT INTO Tags (name, pid) 
                    VALUES (@tagName, @parentId);
                    SELECT last_insert_rowid();";
                
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int newTagId = (int)reader.GetInt64(0);
                        Debug.Log($"Created new tag '{tagName}' with ID {newTagId}");
                        
                        // Create TagsTree entries for the new tag
                        CreateTagsTreeEntries(newTagId, parentId, transaction);
                        
                        return newTagId;
                    }
                }
                
                throw new InvalidOperationException($"Failed to create tag '{tagName}'");
            }
        }

        /// <summary>
        /// Creates TagsTree entries for a tag, including all its ancestors.
        /// </summary>
        /// <param name="tagId">The tag ID</param>
        /// <param name="parentId">The immediate parent ID</param>
        /// <param name="transaction">The database transaction to use</param>
        private void CreateTagsTreeEntries(int tagId, int parentId, IDbTransaction transaction)
        {
            using (var cmd = transaction.Connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                
                // Insert the immediate parent relationship
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO TagsTree (id, pid) 
                    VALUES (@tagId, @parentId)";
                
                var tagParam = new SqliteParameter("@tagId", tagId);
                var parentParam = new SqliteParameter("@parentId", parentId);
                cmd.Parameters.Add(tagParam);
                cmd.Parameters.Add(parentParam);
                cmd.ExecuteNonQuery();
                
                // If this isn't a root tag (parentId != 0), get all ancestors of the parent
                if (parentId != 0)
                {
                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO TagsTree (id, pid)
                        SELECT @tagId, tt.pid
                        FROM TagsTree tt
                        WHERE tt.id = @parentId";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Checks if an image has specific Timeline-Traveler tags and returns the corresponding values.
        /// </summary>
        /// <param name="imageId">The image ID to check</param>
        /// <param name="isNotDated">Output parameter for IsNotDated tag</param>
        /// <param name="isPrivate">Output parameter for IsPrivate tag</param>
        /// <param name="hasTodoCaption">Output parameter for HasTodoCaption tag</param>
        /// <param name="todoCaptionText">Output parameter for todo caption text</param>
        /// <param name="dbconn">The database connection to use (optional, creates new if not provided)</param>
        private void GetTimelineTravelerTagValues(int imageId, out bool isNotDated, out bool isPrivate, out bool hasTodoCaption, out string todoCaptionText, IDbConnection dbconn = null)
        {
            isNotDated = false;
            isPrivate = false;
            hasTodoCaption = false;
            todoCaptionText = null;

            if (imageId <= 0)
            {
                return;
            }

            bool shouldCloseConnection = dbconn == null;
            if (dbconn == null)
            {
                dbconn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}");
                dbconn.Open();
            }

            try
            {
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT t.name as TagName
                        FROM ImageTags it
                        JOIN Tags t ON it.tagid = t.id
                        JOIN TagsTree tt ON t.id = tt.id
                        WHERE it.imageid = @imageId 
                        AND tt.pid IN (
                            SELECT id FROM Tags WHERE name = @timelineTravelerTag AND pid = 0
                        )";
                    
                    var parameter = new SqliteParameter("@imageId", imageId);
                    var timelineTravelerParam = new SqliteParameter("@timelineTravelerTag", TIMELINE_TRAVELER_PARENT_TAG);
                    cmd.Parameters.Add(parameter);
                    cmd.Parameters.Add(timelineTravelerParam);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tagName = reader.GetString(reader.GetOrdinal("TagName"));
                            if (tagName == IS_NOT_DATED_TAG)
                            {
                                isNotDated = true;
                            }
                            else if (tagName == IS_PRIVATE_TAG)
                            {
                                isPrivate = true;
                            }
                            else if (tagName == HAS_TODO_CAPTION_TAG)
                            {
                                hasTodoCaption = true;
                            }
                        }
                        // Get the actual todo caption text
                        todoCaptionText = GetTodoCaptionText(imageId, dbconn);
                        //if todoCaptionTest is not null or empty then check if hatTodoCaption is true, if it is false then Log a warning and set it to true, in the warning log that we are setting it to true because it was not set to true in the database
                        if (!string.IsNullOrEmpty(todoCaptionText) && !hasTodoCaption)
                        {
                            Debug.LogWarning($"For imageId {imageId} Todo caption text is not null or empty but HasTodoCaption is false. Setting HasTodoCaption to true.");
                            hasTodoCaption = true;
                        }
                    }
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    dbconn.Close();
                    dbconn.Dispose();
                }
            }
        }

        /// <summary>
        /// Updates the photo info back to the DigiKam database.
        /// </summary>
        /// <param name="modifiedPhotoInfo">The photo info to update</param>
        public void UpdatePhotoInfo(PhotoInfo modifiedPhotoInfo)
        {
            // We are only interested in the Timeline-Traveler tags and the todo caption comment
            // So we need to get the image ID from the modifiedPhotoInfo
            int imageId = modifiedPhotoInfo.ImageId;
            // We need to get the Timeline-Traveler tags and the todo caption comment
            bool isNotDated = modifiedPhotoInfo.IsNotDated;
            bool isPrivate = modifiedPhotoInfo.IsPrivate;
            bool hasTodoCaption = modifiedPhotoInfo.HasTodoCaption;
            string todoCaptionText = modifiedPhotoInfo.TodoCaptionText;
            SetTimelineTravelerTagValues(imageId, isNotDated, isPrivate, hasTodoCaption, todoCaptionText);
            // I need a function to update the photo's CreationDate in the database
            UpdatePhotoCreationDate(imageId, modifiedPhotoInfo.CreationDate);
        }

        /// <summary>
        /// Updates the creation date of a photo in the database.
        /// </summary>
        /// <param name="imageId">The image ID to update</param>
        /// <param name="creationDate">The new creation date (can be null to clear the date)</param>
        private void UpdatePhotoCreationDate(int imageId, DateTime? creationDate)
        {
            if (imageId <= 0)
            {
                Debug.LogError("Invalid image ID provided to UpdatePhotoCreationDate");
                return;
            }

            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            
                            if (creationDate.HasValue)
                            {
                                // Update with the new creation date
                                cmd.CommandText = @"
                                    UPDATE ImageInformation 
                                    SET creationDate = @creationDate 
                                    WHERE imageid = @imageId";
                                
                                var imageParam = new SqliteParameter("@imageId", imageId);
                                var dateParam = new SqliteParameter("@creationDate", creationDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                                cmd.Parameters.Add(imageParam);
                                cmd.Parameters.Add(dateParam);
                            }
                            else
                            {
                                // Clear the creation date
                                cmd.CommandText = @"
                                    UPDATE ImageInformation 
                                    SET creationDate = NULL 
                                    WHERE imageid = @imageId";
                                
                                var imageParam = new SqliteParameter("@imageId", imageId);
                                cmd.Parameters.Add(imageParam);
                            }
                            
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Debug.Log($"Updated creation date for image {imageId} to {(creationDate.HasValue ? creationDate.Value.ToString() : "NULL")}");
                            }
                            else
                            {
                                Debug.LogWarning($"No rows were updated for image {imageId} - image may not exist in ImageInformation table");
                            }
                        }
                        
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error updating creation date for image {imageId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Sets Timeline-Traveler tag values for an image. Adds tags if values are true, removes them if false.
        /// Also manages the todo caption comment with author "DigiKam Todo".
        /// </summary>
        /// <param name="imageId">The image ID to set tags for</param>
        /// <param name="isNotDated">Whether the image should be marked as not dated</param>
        /// <param name="isPrivate">Whether the image should be marked as private</param>
        /// <param name="hasTodoCaption">Whether the image should have a todo caption</param>
        /// <param name="todoCaptionText">The todo caption text (required if hasTodoCaption is true)</param>
        private void SetTimelineTravelerTagValues(int imageId, bool isNotDated, bool isPrivate, bool hasTodoCaption, string todoCaptionText = null)
        {
            if (imageId <= 0)
            {
                Debug.LogError("Invalid image ID provided to SetTimelineTravelerTagValues");
                return;
            }

            // Validate todo caption parameters
            if (hasTodoCaption && string.IsNullOrEmpty(todoCaptionText))
            {
                //Strange case, but recoverable
                Debug.LogWarning($"for imageId {imageId} We found a case where hasTodoCaption is true, but todoCaptionText is null. Setting hasTodoCaption to false.");
                hasTodoCaption = false;
                return;
            }

            // Ensure base tags exist
            EnsureBaseTimelineTravelerTagsAreAvailable();
            
            // Get the Timeline-Traveler base tag ID
            int timelineTravelerTagId = GetBaseTagIdByName(TIMELINE_TRAVELER_PARENT_TAG);
            if (timelineTravelerTagId == -1)
            {
                throw new InvalidOperationException($"{TIMELINE_TRAVELER_PARENT_TAG} base tag not found");
            }

            // Handle IsNotDated tag
            HandleTagForImage(imageId, IS_NOT_DATED_TAG, timelineTravelerTagId, isNotDated);
            
            // Handle IsPrivate tag
            HandleTagForImage(imageId, IS_PRIVATE_TAG, timelineTravelerTagId, isPrivate);
            
            // Handle HasTodoCaption tag
            HandleTagForImage(imageId, HAS_TODO_CAPTION_TAG, timelineTravelerTagId, hasTodoCaption);
            
            // Handle todo caption comment
            HandleTodoCaptionComment(imageId, hasTodoCaption, todoCaptionText);
            
            Debug.Log($"Successfully updated Timeline-Traveler tags for image {imageId}");
        }

        /// <summary>
        /// Handles adding or removing a specific tag for an image.
        /// </summary>
        /// <param name="imageId">The image ID</param>
        /// <param name="tagName">The name of the tag to handle</param>
        /// <param name="parentTagId">The parent tag ID</param>
        /// <param name="shouldHaveTag">Whether the image should have this tag</param>
        private void HandleTagForImage(int imageId, string tagName, int parentTagId, bool shouldHaveTag)
        {
            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get the tag ID for this specific tag
                        int tagId = GetOrCreateTag(tagName, parentTagId, transaction);
                        
                        // Check if the image currently has this tag
                        bool currentlyHasTag = false;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                SELECT COUNT(*) 
                                FROM ImageTags 
                                WHERE imageid = @imageId AND tagid = @tagId";
                            
                            var imageParam = new SqliteParameter("@imageId", imageId);
                            var tagParam = new SqliteParameter("@tagId", tagId);
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(imageParam);
                            cmd.Parameters.Add(tagParam);
                            
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    currentlyHasTag = reader.GetInt32(0) > 0;
                                }
                            }
                        }
                        
                        // Add or remove the tag as needed
                        if (shouldHaveTag && !currentlyHasTag)
                        {
                            // Add the tag
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"
                                    INSERT INTO ImageTags (imageid, tagid) 
                                    VALUES (@imageId, @tagId)";
                                
                                var imageParam = new SqliteParameter("@imageId", imageId);
                                var tagParam = new SqliteParameter("@tagId", tagId);
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(imageParam);
                                cmd.Parameters.Add(tagParam);
                                cmd.ExecuteNonQuery();
                                Debug.Log($"Added '{tagName}' tag to image {imageId}");
                            }
                        }
                        else if (!shouldHaveTag && currentlyHasTag)
                        {
                            // Remove the tag
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"
                                    DELETE FROM ImageTags 
                                    WHERE imageid = @imageId AND tagid = @tagId";
                                
                                var imageParam = new SqliteParameter("@imageId", imageId);
                                var tagParam = new SqliteParameter("@tagId", tagId);
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(imageParam);
                                cmd.Parameters.Add(tagParam);
                                cmd.ExecuteNonQuery();
                                Debug.Log($"Removed '{tagName}' tag from image {imageId}");
                            }
                        }
                        
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error setting Timeline-Traveler tags for image {imageId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Handles adding, updating, or removing the todo caption comment.
        /// </summary>
        /// <param name="imageId">The image ID</param>
        /// <param name="hasTodoCaption">Whether the image should have a todo caption</param>
        /// <param name="todoCaptionText">The todo caption text</param>
        private void HandleTodoCaptionComment(int imageId, bool hasTodoCaption, string todoCaptionText)
        {
            using (var conn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}"))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check if a todo caption comment already exists
                        int existingCommentId = -1;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"
                                SELECT id 
                                FROM ImageComments 
                                WHERE imageid = @imageId AND author = @todoAuthor";
                            
                            var imageParam = new SqliteParameter("@imageId", imageId);
                            var todoAuthorParam = new SqliteParameter("@todoAuthor", DIGIKAM_TODO_AUTHOR);
                            cmd.Parameters.Add(imageParam);
                            cmd.Parameters.Add(todoAuthorParam);
                            
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    existingCommentId = reader.GetInt32(0);
                                }
                            }
                        }
                        
                        if (hasTodoCaption)
                        {
                            if (existingCommentId != -1)
                            {
                                // Update existing comment
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = @"
                                        UPDATE ImageComments 
                                        SET comment = @comment 
                                        WHERE id = @commentId";
                                    
                                    var commentParam = new SqliteParameter("@comment", todoCaptionText);
                                    var commentIdParam = new SqliteParameter("@commentId", existingCommentId);
                                    cmd.Parameters.Add(commentParam);
                                    cmd.Parameters.Add(commentIdParam);
                                    cmd.ExecuteNonQuery();
                                    Debug.Log($"Updated todo caption for image {imageId}");
                                }
                            }
                            else
                            {
                                // Insert new comment
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = @"
                                        INSERT INTO ImageComments (imageid, type, comment, author) 
                                        VALUES (@imageId, 1, @comment, @todoAuthor)";
                                    
                                    var imageParam = new SqliteParameter("@imageId", imageId);
                                    var commentParam = new SqliteParameter("@comment", todoCaptionText);
                                    var todoAuthorParam = new SqliteParameter("@todoAuthor", DIGIKAM_TODO_AUTHOR);
                                    cmd.Parameters.Add(imageParam);
                                    cmd.Parameters.Add(commentParam);
                                    cmd.Parameters.Add(todoAuthorParam);
                                    cmd.ExecuteNonQuery();
                                    Debug.Log($"Added todo caption for image {imageId}");
                                }
                            }
                        }
                        else if (existingCommentId != -1)
                        {
                            // Remove existing comment
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"
                                    DELETE FROM ImageComments 
                                    WHERE id = @commentId";
                                
                                var commentIdParam = new SqliteParameter("@commentId", existingCommentId);
                                cmd.Parameters.Add(commentIdParam);
                                cmd.ExecuteNonQuery();
                                Debug.Log($"Removed todo caption from image {imageId}");
                            }
                        }
                        
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Debug.LogError($"Error setting todo caption for image {imageId}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the todo caption text for an image from ImageComments with author "DigiKam Todo".
        /// </summary>
        /// <param name="imageId">The image ID</param>
        /// <param name="dbconn">The database connection to use (optional, creates new if not provided)</param>
        /// <returns>The todo caption text, or null if not found</returns>
        private string GetTodoCaptionText(int imageId, IDbConnection dbconn = null)
        {
            bool shouldCloseConnection = dbconn == null;
            if (dbconn == null)
            {
                dbconn = new SqliteConnection($"URI=file:{_digiKamDataBaseFileNameWithFullPath}");
                dbconn.Open();
            }

            try
            {
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT comment
                        FROM ImageComments
                        WHERE imageid = @imageId 
                        AND author = @todoAuthor";
                    
                    var parameter = new SqliteParameter("@imageId", imageId);
                    var todoAuthorParam = new SqliteParameter("@todoAuthor", DIGIKAM_TODO_AUTHOR);
                    cmd.Parameters.Add(parameter);
                    cmd.Parameters.Add(todoAuthorParam);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(reader.GetOrdinal("comment"));
                        }
                    }
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    dbconn.Close();
                    dbconn.Dispose();
                }
            }

            return null;
        }
    }
}
