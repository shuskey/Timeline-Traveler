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
                //this needs to round up
                halfSize = (int)Math.Ceiling(squareSize / 2f);
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
        /// Creates a square texture from the input texture, resizing if necessary to fit within maxTextureSize.
        /// </summary>
        /// <param name="textureToSet">The source texture to process</param>
        /// <param name="maxTextureSize">Maximum size for the texture (will be resized if larger)</param>
        /// <returns>A square Texture2D that fits within the maximum size constraints</returns>
        public static Texture2D CreateSquareTextureMaxSize(Texture2D textureToSet, int maxTextureSize)
        {
            Texture2D finalTexture = textureToSet;
            
            if (textureToSet.width > maxTextureSize || textureToSet.height > maxTextureSize)
            {
                finalTexture = ResizeTexture(textureToSet, maxTextureSize);
            }

            int cropSize = Math.Min(finalTexture.width, finalTexture.height);
            int xStart = (finalTexture.width - cropSize) / 2;
            int yStart = (finalTexture.height - cropSize) / 2;

            Color[] pixels = finalTexture.GetPixels(xStart, yStart, cropSize, cropSize);
            finalTexture = new Texture2D(cropSize, cropSize, finalTexture.format, false);
            finalTexture.SetPixels(pixels);
            finalTexture.Apply();

            return finalTexture;
        }

        /// <summary>
        /// Applies EXIF orientation transformation to a Rect.
        /// </summary>
        public static Rect ApplyExifOrientation(Rect region, float imageWidth, float imageHeight, ExifOrientation orientation)
        {
            switch (orientation)
            {
                case ExifOrientation.TopLeft:     // Normal
                    return region;
                case ExifOrientation.TopRight:    // Mirrored
                    return new Rect(region.x, imageHeight - (region.y + region.height), region.width, region.height);
                case ExifOrientation.BottomRight: // Rotated 180
                    return new Rect(imageWidth - (region.x + region.width), imageHeight - (region.y + region.height), region.width, region.height);
                case ExifOrientation.BottomLeft:  // Mirrored and rotated 180
                    return new Rect(imageWidth - (region.x + region.width), region.y, region.width, region.height);
                case ExifOrientation.LeftTop:     // Mirrored and rotated 270
                    return new Rect(imageHeight - (region.y + region.height), region.x, region.height, region.width);
                case ExifOrientation.RightTop:    // Rotated 90
                    return new Rect(region.y, region.x, region.height, region.width);
                case ExifOrientation.RightBottom: // Mirrored and rotated 90
                    return new Rect(region.y, region.x, -region.height, region.width);
                case ExifOrientation.LeftBottom:  //Rotated 270
                    return new Rect(region.y, region.x, region.height, region.width);
                default:
                    return region;
            }
        }

        public static Rect ApplyInverseExifOrientation(Rect region, float imageWidth, float imageHeight, ExifOrientation orientation)
        {
            switch (orientation)
            {
                // Note, this is a bit different than a typical exif transformation because imageHeight and image Width
                // are in a 'pre-rotated' state, so for any areas where those switch places, we need to flip the x and y coordinates
                case ExifOrientation.TopLeft:     // Normal
                    return region;
                case ExifOrientation.TopRight:    // Mirrored horizontally
                    return new Rect(imageWidth - (region.x + region.width), region.y, region.width, region.height);
                case ExifOrientation.BottomRight: // Rotated 180
                    return new Rect(imageWidth - (region.x + region.width), imageHeight - (region.y + region.height), region.width, region.height);
                case ExifOrientation.BottomLeft:  // Mirrored horizontally and rotated 180
                    return new Rect(region.x, imageHeight - (region.y + region.height), region.width, region.height);
                case ExifOrientation.LeftTop:     // Rotated 90 CCW and mirrored horizontally
                   //WAS return new Rect(imageHeight - (region.y + region.height), imageWidth - (region.x + region.width), region.height, region.width);
                    return new Rect(imageWidth - (region.y + region.height), imageHeight - (region.x + region.width), region.height, region.width);
                case ExifOrientation.RightTop:    // Rotated 90 CCW
                  //WAS  return new Rect(region.y, imageWidth - (region.x + region.width), region.height, region.width);
                    return new Rect(region.y, imageHeight - (region.x + region.width), region.height, region.width);
                case ExifOrientation.RightBottom: // Rotated 270 CCW and mirrored horizontally
                    //WAS  return new Rect(imageHeight - (region.y + region.height), region.x, region.height, region.width);
                    return new Rect(imageWidth - (region.y + region.height), region.x, region.height, region.width);
                case ExifOrientation.LeftBottom:  // Rotated 270 CCW
                    return new Rect(region.y, region.x, region.height, region.width);
                default:
                    return region;
            }
        }

        public static Texture2D CreateSquareTextureFaceRegionMaxSize(Texture2D textureToSet, PhotoInfo photoInfo, int maxTextureSize)
        {
            var inverseRegion = ImageUtils.ApplyInverseExifOrientation(photoInfo.Region, textureToSet.width, textureToSet.height, photoInfo.ExifOrientation);
            
            RectInt squareRegion = ImageUtils.GetSquareBoundedRegion(textureToSet, inverseRegion);
            
            // Create a new texture for the square cropped region
            Texture2D croppedTexture = new Texture2D(squareRegion.width, squareRegion.height);

            //We need to flip the image vertically because the coo
            int flippedY = textureToSet.height - (squareRegion.y + squareRegion.height);
            
            // Copy the pixels from the square region
            Color[] pixels = textureToSet.GetPixels(squareRegion.x, flippedY, 
                                                 squareRegion.width, squareRegion.height);
            croppedTexture.SetPixels(0, 0, squareRegion.width, squareRegion.height, pixels);
            croppedTexture.Apply();

            return croppedTexture;
        }

        /// <summary>
        /// Creates a sprite from a texture with optional cropping and orientation adjustment.
        /// </summary>
        /// <param name="textureToSet">The source texture</param>
        /// <param name="orientation">The EXIF orientation of the image</param>
        /// <param name="crop">Whether to crop the image to a square</param>
        /// <param name="maxTextureSize">Maximum size for the texture (will be resized if larger)</param>
        /// <returns>A tuple containing the created sprite and its rotation angle</returns>
        public static (Sprite sprite, float rotation) CreateSpriteFromTexture(Texture2D textureToSet, PhotoInfo photoInfo, bool cropToFaceRegion = false, int maxTextureSize = 780, bool verbose = false)
        {
            try
            {
                var orientation = photoInfo.ExifOrientation;
                Texture2D finalTexture;

                if (cropToFaceRegion)
                {
                    if (verbose) 
                    {
                        Debug.Log($"CreateSpriteFromTexture Called with cropToFaceRegion true");
                        Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
                        Debug.Log($"Photo Info: {photoInfo.ExifOrientation}");
                        Debug.Log($"Photo Info: {photoInfo.Region}");
                        Debug.Log($"Photo Info: {photoInfo.ItemLabel}");
                        Debug.Log($"Photo Info: {photoInfo.FullPathToFileName}");
                        Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
                        Debug.Log($"Texture To Set: {textureToSet.name}");
                        Debug.Log($"Max Texture Size: {maxTextureSize}");
                    }
                    finalTexture = CreateSquareTextureFaceRegionMaxSize(textureToSet, photoInfo, maxTextureSize);
                } else {
                    finalTexture = CreateSquareTextureMaxSize(textureToSet, maxTextureSize);
                }

                // Get sprite rect and rotation based on orientation
                if (verbose) {
                    Debug.Log($"GetSpriteRectAndRotationForOrientation Called");
                    Debug.Log($"Orientation: {orientation}");
                    Debug.Log($"Final Texture: {finalTexture.name}");
                }

                var (spriteRect, rotation) = GetSpriteRectAndRotationForOrientation(finalTexture, orientation);
                if (verbose) {
                    Debug.Log($"GetSpriteRectAndRotationForOrientation returns spriteRect: {spriteRect.x}, {spriteRect.y}, {spriteRect.width}, {spriteRect.height} and rotation: {rotation}");
                }

                // Create sprite with the adjusted rect that may mirror the image based on orientation  
                Sprite sprite = Sprite.Create(finalTexture, spriteRect, new Vector2(0.5f, 0.5f), 100f);
                if (verbose) {
                    Debug.Log($"CreateSpriteFromTexture returns sprite name: {sprite.name} and rotation: {rotation}");
                }
                return (sprite, rotation);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in CreateSpriteFromTexture: {ex.Message}");
                Debug.LogError("Texture attempted to be created: " + textureToSet.name);
                Debug.LogError("Texture orientation attempted to be created: " + photoInfo.ExifOrientation);
                return (null, 0f);
            }
        }

        /// <summary>
        /// Downloads and processes an image from a photo archive.
        /// </summary>
        /// <param name="photoInfo">The photo information containing path and orientation</param>
        /// <param name="fallbackTexture">Texture to use if download fails</param>
        /// <returns>Coroutine that yields the downloaded texture</returns>
        public static IEnumerator DownloadAndProcessImage(PhotoInfo photoInfo, Texture2D fallbackTexture, bool verbose = false)
        {        
            Texture2D downloaded = null;
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(photoInfo.PicturePathInArchive);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                if (verbose) Debug.Log($"Error downloading photo:{photoInfo.PicturePathInArchive} error: {request.error}");
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
        /// <param name="cropToFaceRegion">Whether to crop the image to a square</param>
        public static void SetImagePanelTexture(Image destinationImagePanel, Texture2D textureToSet, PhotoInfo resultingPhotoInfo = null, bool cropToFaceRegion = false, bool verbose = false)
        {
            if (destinationImagePanel == null)
            {
                if (verbose) Debug.Log($"SetImagePanelTexture Called with destinationImagePanel null");
                Debug.LogWarning("Destination image panel is null");
                return;
            }            
            if (resultingPhotoInfo == null)
            {
                if (verbose) Debug.Log($"SetImagePanelTexture Called with resultingPhotoInfo null");
                resultingPhotoInfo = new PhotoInfo(textureToSet.name, new Rect(0, 0, textureToSet.width, textureToSet.height), ExifOrientation.TopLeft);                
            }

            // Use the utility method to create the sprite and get rotation
            (Sprite sprite, float rotation) = CreateSpriteFromTexture(textureToSet, resultingPhotoInfo, cropToFaceRegion, verbose: verbose);
            if (verbose) Debug.Log($"CreateSpriteFromTexture returns sprite name: {sprite.name} and rotation: {rotation}");

            if (sprite == null)
            {   
                if (verbose) Debug.Log($"SetImagePanelTexture Called with sprite null");
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
        public static IEnumerator SetImagePanelTextureFromPhotoArchive(Image destinationImagePanel, PhotoInfo photoInfo, Texture2D fallbackTexture, bool cropToFaceRegion = false, bool verbose = false)
        {        
            if (verbose) {
                // log the photo info
                Debug.Log($"SetImagePanelTextureFromPhotoArchive Called");
                Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
                Debug.Log($"Photo Info: {photoInfo.ExifOrientation}");
                Debug.Log($"Photo Info: {photoInfo.Region}");
                Debug.Log($"Photo Info: {photoInfo.ItemLabel}");
                Debug.Log($"Photo Info: {photoInfo.FullPathToFileName}");
                Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
            }
            var downloadCoroutine = DownloadAndProcessImage(photoInfo, fallbackTexture);
            yield return downloadCoroutine;
            if (verbose) Debug.Log($"DownloadAndProcessImage Coroutine Completed");
            var (downloaded, resultingPhotoInfo) = ((Texture2D, PhotoInfo))downloadCoroutine.Current;
            if (verbose) {
                Debug.Log($"DownloadAndProcessImage complete, returning with photo info:");
                Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
                Debug.Log($"Photo Info: {photoInfo.ExifOrientation}");
                Debug.Log($"Photo Info: {photoInfo.Region}");
                Debug.Log($"Photo Info: {photoInfo.ItemLabel}");
                Debug.Log($"Photo Info: {photoInfo.FullPathToFileName}");
                Debug.Log($"Photo Info: {photoInfo.PicturePathInArchive}");
            }
            // Now do some magic: the downloaded texture is the full resolution image, but we need to crop it to the region of the person
            SetImagePanelTexture(destinationImagePanel, downloaded, resultingPhotoInfo, cropToFaceRegion, verbose: verbose);
        }

        /// <summary>
        /// Parses an XML region string into a Rect.
        /// </summary>
        /// <param name="regionXml">XML string containing region information with x, y, width, and height attributes</param>
        /// <returns>A Rect representing the region, or a default Rect(0,0,0,0) if parsing fails</returns>
        public static Rect ParseRegionXml(string regionXml, float imageWidth, float imageHeight, bool verbose = false)
        {
            //yFlipped is needed because the y coordinate in the XML is the top of the image, but we need the bottom of the image for Unity Texture2D
            try
            {
                if (string.IsNullOrEmpty(regionXml)) {
                    if (verbose)    {
                        Debug.Log($"Region XML is empty");
                    }
                    return new Rect(0, 0, 0, 0);
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(regionXml);
                var rectElement = doc.DocumentElement;
                if (verbose)    {
                    Debug.Log($"Region XML: {regionXml}");
                    Debug.Log($"Parameters Passed in: Image Width: {imageWidth}, Image Height: {imageHeight}");                            
                    Debug.Log($"XML Doc ExtractedRect Element Face Region(x,y,width,height): {rectElement.GetAttribute("x")}, {rectElement.GetAttribute("y")}, {rectElement.GetAttribute("width")}, {rectElement.GetAttribute("height")}");
                }
                
                var y = float.Parse(rectElement.GetAttribute("y"));
                var height = float.Parse(rectElement.GetAttribute("height"));
                var yFlipped = imageHeight - (y + height);
                if (verbose)    {
                    Debug.Log($"Y Flipped: {yFlipped}");
                }

                var returnRect = new Rect(
                    float.Parse(rectElement.GetAttribute("x")),
                    float.Parse(rectElement.GetAttribute("y")),//yFlipped,
                    float.Parse(rectElement.GetAttribute("width")),
                    float.Parse(rectElement.GetAttribute("height"))
                );
                if (verbose)    {
                    Debug.Log($"Returned Rect (NO flipped Y) (x,y,width,height): {returnRect.x}, {returnRect.y}, {returnRect.width}, {returnRect.height}");
                }
                return returnRect;  
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse region XML: {ex.Message}");
                return new Rect(0, 0, 0, 0);
            }
        }
    }
} 