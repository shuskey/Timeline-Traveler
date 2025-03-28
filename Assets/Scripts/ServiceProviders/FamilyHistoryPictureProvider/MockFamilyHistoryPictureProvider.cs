using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public class MockFamilyHistoryPictureProvider : IFamilyHistoryPictureProvider
    {
        private Dictionary<string, string> _configuration;
        private bool _isInitialized;

        public void Initialize(Dictionary<string, string> configuration)
        {
            _configuration = configuration;
            _isInitialized = true;
        }

        public List<Texture2D> GetThumbnailForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("MockFamilyHistoryPictureProvider not initialized");
                return new List<Texture2D>();
            }

            // Create a mock thumbnail (2x2 checkerboard)
            var mockThumbnail = new Texture2D(2, 2);
            mockThumbnail.SetPixel(0, 0, Color.white);
            mockThumbnail.SetPixel(0, 1, Color.black);
            mockThumbnail.SetPixel(1, 0, Color.black);
            mockThumbnail.SetPixel(1, 1, Color.white);
            mockThumbnail.Apply();

            return new List<Texture2D> { mockThumbnail };
        }

        public List<(Texture2D Photo, Dictionary<string, string> Metadata)> GetPhotoListForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("MockFamilyHistoryPictureProvider not initialized");
                return new List<(Texture2D, Dictionary<string, string>)>();
            }

            // Create a mock photo (4x4 checkerboard)
            var mockPhoto = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    mockPhoto.SetPixel(x, y, ((x + y) % 2 == 0) ? Color.white : Color.black);
                }
            }
            mockPhoto.Apply();

            // Create mock metadata
            var mockMetadata = new Dictionary<string, string>
            {
                { "Date", $"{year}-01-01" },
                { "Location", "Mock Location" },
                { "Description", "Mock photo description" },
                { "Tags", "Family,Portrait" }
            };

            return new List<(Texture2D, Dictionary<string, string>)> 
            { 
                (mockPhoto, mockMetadata) 
            };
        }
    }
} 