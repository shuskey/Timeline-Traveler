using UnityEngine;
using System;
using System.IO;
using Assets.Scripts.Enums;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Xml;

namespace Assets.Scripts.Utilities
{
    public class PhotoInfo
    {
        public string FullPathToFileName { get; set; }
        public Rect Region { get; set; }
        public ExifOrientation ExifOrientation { get; set; }
        public string PicturePathInArchive { get; set; }
        public string ItemLabel { get; set; }

        public PhotoInfo(string fullPathToFileName, Rect region, ExifOrientation orientation, string picturePathInArchive = null, string itemLabel = null)
        {
            FullPathToFileName = fullPathToFileName;
            Region = region;
            ExifOrientation = orientation;
            PicturePathInArchive = picturePathInArchive ?? fullPathToFileName;
            ItemLabel = itemLabel ?? Path.GetFileNameWithoutExtension(fullPathToFileName);
        }
    }

    public static class ImageUtils
    {
        /// <summary>
        /// Calculates a square region around a given rectangle while respecting texture bounds.
        /// The square is centered on the input region and sized to encompass it while maintaining aspect ratio.
        /// If the square would exceed texture bounds, it's adjusted accordingly.
        /// </summary>
        /// <param name="fullTexture">The texture containing the region</param>
        /// <param name="faceRegion">The original rectangular region to convert to square</param>
        /// <returns>A RectInt representing the square region that fits within texture bounds</returns>
        public static RectInt GetSquareBoundedRegion(Texture2D fullTexture, Rect faceRegion)
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
            if (left < 0) sidesExceeded++;
            if (right > fullTexture.width) sidesExceeded++;
            if (top < 0) sidesExceeded++;
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

        /// <summary>
        /// Gets the sprite rectangle and rotation angle for a texture based on its EXIF orientation.
        /// </summary>
        /// <param name="texture">The source texture</param>
        /// <param name="orientation">The EXIF orientation of the image</param>
        /// <returns>A tuple containing the sprite rectangle and rotation angle</returns>
        public static (Rect spriteRect, float rotation) GetSpriteRectAndRotationForOrientation(Texture2D texture, ExifOrientation orientation)
        {
            switch (orientation)
            {
                case ExifOrientation.TopLeft:     // Normal
                    return (new Rect(0, 0, texture.width, texture.height), 0f);
                case ExifOrientation.TopRight:    // Mirrored
                    return (new Rect(0, 0, -texture.width, texture.height), 0f);
                case ExifOrientation.BottomRight: // Rotated 180
                    return (new Rect(0, 0, -texture.width, -texture.height), 0f);
                case ExifOrientation.BottomLeft:  // Mirrored and rotated 180
                    return (new Rect(0, 0, texture.width, -texture.height), 0f);
                case ExifOrientation.LeftTop:     // Mirrored and rotated 270
                    return (new Rect(0, 0, texture.height, texture.width), -270f);
                case ExifOrientation.RightTop:    // Rotated 90
                    return (new Rect(0, 0, texture.height, texture.width), -90f);
                case ExifOrientation.RightBottom: // Mirrored and rotated 90
                    return (new Rect(0, 0, -texture.height, texture.width), -90f);
                case ExifOrientation.LeftBottom:  // Rotated 270
                    return (new Rect(0, 0, texture.height, texture.width), 270f);
                default:
                    return (new Rect(0, 0, texture.width, texture.height), 0f);
            }
        }

        /// <summary>
        /// Resizes a texture while maintaining its aspect ratio.
        /// </summary>
        /// <param name="source">The source texture to resize</param>
        /// <param name="targetWidth">The desired width of the resized texture</param>
        /// <returns>A new Texture2D with the specified width and proportional height</returns>
        public static Texture2D ResizeTexture(Texture2D source, int targetWidth)
        {
            // Calculate height maintaining aspect ratio
            float aspectRatio = (float)source.height / source.width;
            int targetHeight = Mathf.RoundToInt(targetWidth * aspectRatio);

            // Create a new texture with the target dimensions
            Texture2D resized = new Texture2D(targetWidth, targetHeight, source.format, false);
            
            // Create a temporary RenderTexture to do the resizing
            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            rt.Create();

            // Store the current render texture
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            // Copy the source texture to the render texture
            Graphics.Blit(source, rt);

            // Read the pixels from the render texture
            resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            resized.Apply();

            // Restore the previous render texture
            RenderTexture.active = previous;
            rt.Release();

            return resized;
        }

        /// <summary>
        /// Creates a sprite from a texture with optional cropping and orientation adjustment.
        /// </summary>
        /// <param name="textureToSet">The source texture</param>
        /// <param name="orientation">The EXIF orientation of the image</param>
        /// <param name="crop">Whether to crop the image to a square</param>
        /// <param name="maxTextureSize">Maximum size for the texture (will be resized if larger)</param>
        /// <returns>A tuple containing the created sprite and its rotation angle</returns>
        public static (Sprite sprite, float rotation) CreateSpriteFromTexture(Texture2D textureToSet, ExifOrientation orientation = ExifOrientation.TopLeft, bool cropToRegion = false, int maxTextureSize = 780)
        {
            try
            {
                // Resize the texture if it's too large
                Texture2D finalTexture = textureToSet;
                
                if (textureToSet.width > maxTextureSize || textureToSet.height > maxTextureSize)
                {
                    finalTexture = ResizeTexture(textureToSet, maxTextureSize);
                }

                if (cropToRegion)
                {
                                      
                    int cropSize = Math.Min(finalTexture.width, finalTexture.height);
                    int xStart = (finalTexture.width - cropSize) / 2;
                    int yStart = (finalTexture.height - cropSize) / 2;

                    Color[] pixels = finalTexture.GetPixels(xStart, yStart, cropSize, cropSize);
                    finalTexture = new Texture2D(cropSize, cropSize, finalTexture.format, false);
                    finalTexture.SetPixels(pixels);
                    finalTexture.Apply();
                }

                // Get sprite rect and rotation based on orientation
                var (spriteRect, rotation) = GetSpriteRectAndRotationForOrientation(finalTexture, orientation);

                // Create sprite with the adjusted rect that may mirror the image based on orientation  
                Sprite sprite = Sprite.Create(finalTexture, spriteRect, new Vector2(0.5f, 0.5f), 100f);

                return (sprite, rotation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in CreateSpriteFromTexture: {ex.Message}");
                Debug.LogError("Texture attempted to be created: " + textureToSet.name);
                Debug.LogError("Texture orientation attempted to be created: " + orientation);
                return (null, 0f);
            }
        }

        /// <summary>
        /// Downloads and processes an image from a photo archive.
        /// </summary>
        /// <param name="photoInfo">The photo information containing path and orientation</param>
        /// <param name="fallbackTexture">Texture to use if download fails</param>
        /// <returns>Coroutine that yields the downloaded texture</returns>
        public static IEnumerator DownloadAndProcessImage(PhotoInfo photoInfo, Texture2D fallbackTexture)
        {        
            Texture2D downloaded = null;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(photoInfo.PicturePathInArchive);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"Error downloading photo:{photoInfo.PicturePathInArchive} error: {request.error}");
                downloaded = fallbackTexture;
                photoInfo.ExifOrientation = ExifOrientation.TopLeft;
                photoInfo.Region = new Rect(0, 0, 0, 0);
            }
            else
            {
                downloaded = ((DownloadHandlerTexture)request.downloadHandler).texture;
                if (downloaded == null)
                {
                    Debug.LogWarning($"Error downloading photo:{photoInfo.PicturePathInArchive} error: downloaded is null"); 
                    downloaded = fallbackTexture;
                    photoInfo.ExifOrientation = ExifOrientation.TopLeft;
                    photoInfo.Region = new Rect(0, 0, 0, 0);
                }
            }
            yield return (downloaded, photoInfo);
        }

        /// <summary>
        /// Sets a texture on a UI Image component with proper orientation and cropping.
        /// </summary>
        /// <param name="destinationImagePanel">The UI Image component to set the texture on</param>
        /// <param name="textureToSet">The texture to set</param>
        /// <param name="orientation">EXIF orientation of the image</param>
        /// <param name="cropToRegion">Whether to crop the image to a square</param>
        public static void SetImagePanelTexture(Image destinationImagePanel, Texture2D textureToSet, PhotoInfo resultingPhotoInfo = null, bool cropToRegion = false)
        {
            if (destinationImagePanel == null)
            {
                Debug.LogWarning("Destination image panel is null");
                return;
            }
            var orientation = ExifOrientation.TopLeft;
            var cropRegion = new Rect(0, 0, textureToSet.width, textureToSet.height);
            if (resultingPhotoInfo != null)
            {
                orientation = resultingPhotoInfo.ExifOrientation;
                cropRegion =  resultingPhotoInfo.Region;
            }

            // Use the utility method to create the sprite and get rotation
            (Sprite sprite, float rotation) = CreateSpriteFromTexture(textureToSet, orientation, cropToRegion);

            if (sprite == null)
            {
                Debug.LogWarning("Failed to create sprite from texture");
                return;
            }

            destinationImagePanel.sprite = sprite; 
            // Apply the sprite and rotation
            destinationImagePanel.transform.localRotation = Quaternion.Euler(0, 0, rotation);      
        }

        /// <summary>
        /// Downloads an image from a photo archive and sets it on a UI Image component.
        /// </summary>
        /// <param name="destinationImagePanel">The UI Image component to set the texture on</param>
        /// <param name="photoInfo">The photo information</param>
        /// <param name="fallbackTexture">Texture to use if download fails</param>
        /// <returns>Coroutine that handles the download and setting of the texture</returns>
        public static IEnumerator SetImagePanelTextureFromPhotoArchive(Image destinationImagePanel, PhotoInfo photoInfo, Texture2D fallbackTexture, bool cropToRegion = false)
        {        
            var downloadCoroutine = DownloadAndProcessImage(photoInfo, fallbackTexture);
            yield return downloadCoroutine;
            
            var (downloaded, resultingPhotoInfo) = ((Texture2D, PhotoInfo))downloadCoroutine.Current;
            // Now do some magic: the downloaded texture is the full resolution image, but we need to crop it to the region of the person
            SetImagePanelTexture(destinationImagePanel, downloaded, resultingPhotoInfo, cropToRegion);
        }

        /// <summary>
        /// Parses an XML region string into a Rect.
        /// </summary>
        /// <param name="regionXml">XML string containing region information with x, y, width, and height attributes</param>
        /// <returns>A Rect representing the region, or a default Rect(0,0,0,0) if parsing fails</returns>
        public static Rect ParseRegionXml(string regionXml)
        {
            try
            {
                if (string.IsNullOrEmpty(regionXml))
                    return new Rect(0, 0, 0, 0);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(regionXml);
                var rectElement = doc.DocumentElement;

                return new Rect(
                    float.Parse(rectElement.GetAttribute("x")),
                    float.Parse(rectElement.GetAttribute("y")),
                    float.Parse(rectElement.GetAttribute("width")),
                    float.Parse(rectElement.GetAttribute("height"))
                );
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse region XML: {ex.Message}");
                return new Rect(0, 0, 0, 0);
            }
        }
    }
} 