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
    public class PrimaryThumbnailForPersonFromDigiKam : DataProviderBase
    {
        private string _rootsMagicDataBaseFileName;
        private string _digiKamDataBaseFileName;
        private string _rootsMagicToDigiKamDataBaseFolderPath;
     

        public PrimaryThumbnailForPersonFromDigiKam(string DataBaseFileName, string DigiKamFileName)
        {
            _rootsMagicDataBaseFileName = DataBaseFileName;
            _digiKamDataBaseFileName = DigiKamFileName;
            // Get path and appent filename of rootsmagic-digikam.db and append the filename  "rootsmagic-digikam.db" with a call to Path.Combine
            _rootsMagicToDigiKamDataBaseFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_digiKamDataBaseFileName), "rootsmagic-digikam.db"  );
          

        }

        public byte[] GetSquarePrimaryPhotoForPersonFromDataBase(int ownerId)
        {
            byte[] imageToReturn = null;
            IDbConnection dbconn = null;

            try
            {
                // Verify database files exist
                if (!System.IO.File.Exists(_digiKamDataBaseFileName))
                {
                    Debug.LogError($"DigiKam database file not found: {_digiKamDataBaseFileName}");
                    return null;
                }
                if (!System.IO.File.Exists(_rootsMagicDataBaseFileName))
                {
                    Debug.LogError($"RootsMagic database file not found: {_rootsMagicDataBaseFileName}");
                    return null;
                }
                if (!System.IO.File.Exists(_rootsMagicToDigiKamDataBaseFolderPath))
                {
                    Debug.LogError($"RootsMagic to DigiKam database file not found: {_rootsMagicToDigiKamDataBaseFolderPath}");
                    return null;
                }

                string conn = "URI=file:" + _digiKamDataBaseFileName;
                dbconn = new SqliteConnection(conn);
                dbconn.Open();
                
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    // Attach the RootsMagic database and the RootsMagic To DigiKam database    
                    try
                    {
                        dbcmd.CommandText = $"ATTACH DATABASE '{_rootsMagicDataBaseFileName}' AS rootsmagic;";
                        dbcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to attach RootsMagic database: {ex.Message}");
                        return null;
                    }
                    try 
                    {
                        dbcmd.CommandText  = $"ATTACH DATABASE '{_rootsMagicToDigiKamDataBaseFolderPath}' AS rootsmagictodigikam;";
                        dbcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to attach RootsMagic To DigiKam database: {ex.Message}");
                        return null;
                    }
                    //also need to attach the thumbnails-digikam database
                    string thumbnailsDigiKamDataBaseFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_digiKamDataBaseFileName), "thumbnails-digikam.db"  );
                    string attachCmd = $"ATTACH DATABASE '{thumbnailsDigiKamDataBaseFolderPath}' AS thumbnailsdigikam;";
                    dbcmd.CommandText = attachCmd;
                    dbcmd.ExecuteNonQuery();

                    string QUERYTHUMBNAILS =
                        "SELECT r2d.PersonID, r2d.tagid, tags.name, paths.thumbId, images.id as \"imageId\",\n" +
                        "tnails.type, tnails.modificationDate, tnails.orientationHint,\n" +
                        "\"C:\" || (SELECT specificPath FROM AlbumRoots WHERE AlbumRoots.label = \"Pictures\") ||\n" +
                        "albums.relativePath || \"/\" || images.name as \"fullPathToFileName\",\n" +
                        "region.value as \"region\", tnails.data as 'PGFImageData'\n" +
                        "FROM rootsmagictodigikam.PersonDigiKamTag r2d \n" +
                        "JOIN Tags tags ON r2d.tagid = tags.id\n" +
                        "LEFT JOIN Images images ON tags.icon = images.id\n" +
                        "LEFT JOIN Albums albums ON images.album = albums.id\n" +
                        "LEFT JOIN ImageTagProperties region ON tags.icon = region.imageid AND tags.id = region.tagid\n" +
                        "INNER JOIN thumbnailsdigikam.FilePaths paths ON fullPathToFileName = paths.path\n" +
                        "INNER JOIN thumbnailsdigikam.Thumbnails tnails ON paths.thumbId = tnails.id\n";
                    QUERYTHUMBNAILS +=
                        $"WHERE r2d.PersonID = \"{ownerId}\" AND images.album IS NOT NULL;";

                    dbcmd.CommandText = QUERYTHUMBNAILS;
                    
                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        int currentArrayIndex = 0;
                        int limitListSizeTo = 1;
                        while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                            string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                            // now read in the string value for region
                            string region = (string)reader["region"];
                            Debug.Log($"Region: {region}");
                            
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

                                    squareFaceRegionRectInt = GetSquareFaceRegionRectInt(fullTexture, faceRegion);
                                    
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
        // let create a new local function here named GetSquareFaceRegionRect
        public RectInt GetSquareFaceRegionRectInt(Texture2D fullTexture, Rect faceRegion)
        {
            // Calculate center point and initial square size
            int centerX = (int)(faceRegion.x + faceRegion.width / 2);
            int centerY = (int)(faceRegion.y + faceRegion.height / 2);
            int squareSize = Math.Max((int)faceRegion.width, (int)faceRegion.height);
            int halfSize = squareSize / 2;

            // Calculate initial square bounds
            int left = centerX - halfSize;
            int right = centerX + halfSize;
            int top = centerY - halfSize;
            int bottom = centerY + halfSize;

            // Count how many sides exceed bounds
            int sidesExceeded = 0;
            if (left < 0 ) sidesExceeded++;
            if (right > fullTexture.width) sidesExceeded++;
            if (top < 0 ) sidesExceeded++;
            if (bottom > fullTexture.height) sidesExceeded++;

            // Calculate required size reduction
            int horizontalOverflow = Math.Max(0, -left) + Math.Max(0, right - fullTexture.width);
            int verticalOverflow = Math.Max(0, -top) + Math.Max(0, bottom - fullTexture.height);
            int maxOverflow = Math.Max(horizontalOverflow, verticalOverflow);

            // Adjust square size if needed (divide by 2 if only one side exceeded)
            if (maxOverflow > 0)
            {
                int reduction = sidesExceeded == 1 ? maxOverflow * 2 : maxOverflow;
                squareSize -= reduction;
                halfSize = squareSize / 2;
            }

            // Return the final square region
            return new RectInt(
                centerX - halfSize,
                centerY - halfSize,
                squareSize,
                squareSize
            );
        }
    }
}

