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
    public class PrimaryThumbnailForPersonFromDigiKam : DataProviderBase
    {
        private string _digiKamDataBaseFileName;
        private DigiKamConnector _connector;

        public PrimaryThumbnailForPersonFromDigiKam(string DataBaseFileName, string DigiKamFileName)
        {
            _digiKamDataBaseFileName = DigiKamFileName;
            _connector = new DigiKamConnector(DataBaseFileName, DigiKamFileName);
        }

        public byte[] GetSquarePrimaryPhotoForPersonFromDataBase(int ownerId)
        {
            byte[] imageToReturn = null;
            IDbConnection dbconn = null;

            try
            {
                // Verify database file exists
                if (!System.IO.File.Exists(_digiKamDataBaseFileName))
                {
                    Debug.LogError($"DigiKam database file not found: {_digiKamDataBaseFileName}");
                    return null;
                }

                // Get the tagId for this ownerId
                int tagId = _connector.GetTagIdForOwnerId(ownerId);
                if (tagId == -1)
                {
                    Debug.LogWarning($"No DigiKam tag found for ownerId: {ownerId}");
                    return null;
                }

                string conn = "URI=file:" + _digiKamDataBaseFileName;
                dbconn = new SqliteConnection(conn);
                dbconn.Open();
                
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    //attach the thumbnails-digikam database
                    string thumbnailsDigiKamDataBaseFolderPath = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(_digiKamDataBaseFileName), 
                        "thumbnails-digikam.db");
                    string attachCmd = $"ATTACH DATABASE '{thumbnailsDigiKamDataBaseFolderPath}' AS thumbnailsdigikam;";
                    dbcmd.CommandText = attachCmd;
                    dbcmd.ExecuteNonQuery();

                    string QUERYTHUMBNAILS =
                        "SELECT tags.name, paths.thumbId, images.id as \"imageId\",\n" +
                        "tnails.type, tnails.modificationDate, tnails.orientationHint,\n" +
                        "\"C:\" || (SELECT specificPath FROM AlbumRoots WHERE AlbumRoots.label = \"Pictures\") ||\n" +
                        "albums.relativePath || \"/\" || images.name as \"fullPathToFileName\",\n" +
                        "region.value as \"region\", tnails.data as 'PGFImageData'\n" +
                        "FROM Tags tags\n" +
                        "LEFT JOIN Images images ON tags.icon = images.id\n" +
                        "LEFT JOIN Albums albums ON images.album = albums.id\n" +
                        "LEFT JOIN ImageTagProperties region ON tags.icon = region.imageid AND tags.id = region.tagid\n" +
                        "INNER JOIN thumbnailsdigikam.FilePaths paths ON fullPathToFileName = paths.path\n" +
                        "INNER JOIN thumbnailsdigikam.Thumbnails tnails ON paths.thumbId = tnails.id\n" +
                        $"WHERE tags.id = {tagId} AND images.album IS NOT NULL;";

                    dbcmd.CommandText = QUERYTHUMBNAILS;
                    
                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        int currentArrayIndex = 0;
                        int limitListSizeTo = 1;
                        while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                            string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                            // now read in the string value for region
                            string region = (string)reader["region"];
                            
                            Rect faceRegion;
                            if (!string.IsNullOrEmpty(region))
                            {
                                // Parse XML string
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(region);
                                var rectElement = doc.DocumentElement;

                                // Create Rect from XML attributes
                                faceRegion = new Rect(
                                    float.Parse(rectElement.GetAttribute("x")),
                                    float.Parse(rectElement.GetAttribute("y")),
                                    float.Parse(rectElement.GetAttribute("width")),
                                    float.Parse(rectElement.GetAttribute("height"))
                                );
                            }
                            else
                            {
                                // Default to full image if no region specified
                                faceRegion = new Rect(0, 0, 1, 1);
                            }
                            
                            if (System.IO.File.Exists(pathToFullResolutionImage)) {
                                try {
                                    // Load the full image into a byte array
                                    RectInt squareFaceRegionRectInt;
                                    byte[] fullImageBytes = System.IO.File.ReadAllBytes(pathToFullResolutionImage);
                                    
                                    // Create a texture from the bytes
                                    Texture2D fullTexture = new Texture2D(2, 2);
                                    fullTexture.LoadImage(fullImageBytes);

                                    squareFaceRegionRectInt = ImageUtils.GetSquareBoundedRegion(fullTexture, faceRegion);
                                    
                                    // Create a new texture for the square cropped region
                                    Texture2D croppedTexture = new Texture2D(
                                        squareFaceRegionRectInt.width,
                                        squareFaceRegionRectInt.height);
                                    
                                    // Copy the pixels from the square region we want
                                    Color[] pixels = fullTexture.GetPixels(
                                        squareFaceRegionRectInt.x, squareFaceRegionRectInt.y, 
                                        squareFaceRegionRectInt.width, squareFaceRegionRectInt.height);
                                    croppedTexture.SetPixels(0, 0, squareFaceRegionRectInt.width, squareFaceRegionRectInt.height, pixels);
                                    croppedTexture.Apply();
                                    
                                    // Convert back to bytes
                                    imageToReturn = croppedTexture.EncodeToPNG();
                                    
                                    // Clean up
                                    UnityEngine.Object.Destroy(fullTexture);
                                    UnityEngine.Object.Destroy(croppedTexture);
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
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Database error: {ex.Message}\nStack trace: {ex.StackTrace}");
                return null;
            }
            return imageToReturn;
        }
    }
}

