using UnityEngine;
using System.Collections.Generic;
using System.IO;

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

        public List<Texture2D> GetThumbnailForPerson(int personId, int year)
        {
            // Create a mock texture
            var texture = new Texture2D(100, 100);
            var colors = new Color[100 * 100];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.gray;
            }
            texture.SetPixels(colors);
            texture.Apply();
            return new List<Texture2D> { texture };
        }

        public List<(string FullPathToFileName, Dictionary<string, string> Metadata)> GetPhotoListForPerson(int personId, int year)
        {
            var result = new List<(string FullPathToFileName, Dictionary<string, string> Metadata)>();
            

            // Create mock metadata
            var metadata = new Dictionary<string, string>
            {
                { "Date", $"{year}-01-01" },
                { "Location", "Mock Location" },
                { "Description", "Mock Description" },
                { "Source", "Mock Source" }
            };

            result.Add(("MockPhoto.jpg", metadata));
            return result;
        }
    }
} 