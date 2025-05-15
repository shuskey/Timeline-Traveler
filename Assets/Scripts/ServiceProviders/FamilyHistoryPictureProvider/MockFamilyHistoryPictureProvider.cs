using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.DataProviders;

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

        public List<DigiKamConnector.PhotoInfo> GetPhotoInfoListForPerson(int personId, int year)
        {
              return new List<DigiKamConnector.PhotoInfo>
            {
                new DigiKamConnector.PhotoInfo("MockPhoto.jpg", "Mock Location", 1)
            };
        }

        public DigiKamConnector.PhotoInfo GetThumbnailPhotoInfoForPerson(int personId, int year)
        {
            return new DigiKamConnector.PhotoInfo("MockPhoto.jpg", "Mock Location", 1);
        }
    }
} 