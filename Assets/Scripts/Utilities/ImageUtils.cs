using UnityEngine;
using System;

namespace Assets.Scripts.Utilities
{
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
    }
} 