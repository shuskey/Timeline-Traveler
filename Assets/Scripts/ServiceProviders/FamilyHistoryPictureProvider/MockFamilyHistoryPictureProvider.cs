using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Utilities;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public class MockFamilyHistoryPictureProvider : IFamilyHistoryPictureProvider
    {
        private Dictionary<string, string> _configuration;
        private string _rootsMagicDbPath;
    private string _digiKamDbPath;
        private string _digiKamFolder;

        public void Initialize(Dictionary<string, string> configuration)
        {
            _configuration = configuration;
            if (!configuration.TryGetValue("RootsMagicDbPath", out _rootsMagicDbPath))
            {
                Debug.LogError("RootsMagicDbPath not found in configuration");
            }
            if (!configuration.TryGetValue("DigiKamDbPath", out _digiKamDbPath))
            {
                Debug.LogError("DigiKamDbPath not found in configuration");
            }
            _digiKamFolder = Path.GetDirectoryName(_digiKamDbPath);
        }

        public bool AreAllDatabaseFilesPresent()
        {
            // Mock implementation always returns true
            return true;
        }

        public List<PhotoInfo> GetPhotoInfoListForPerson(int personId, int year)
        {
              return new List<PhotoInfo>
            {
                new("MockPhoto.jpg", new Rect(0, 0, 100, 100), ExifOrientation.TopLeft, "MockPhoto.jpg", "Mock Photo")
            };
        }

        public PhotoInfo GetThumbnailPhotoInfoForPerson(int personId, int year, bool verbose = false)
        {
            return new PhotoInfo("MockPhoto.jpg", new Rect(0, 0, 100, 100), ExifOrientation.TopLeft, "MockPhoto.jpg", "Mock Photo");
        }
    }
} 