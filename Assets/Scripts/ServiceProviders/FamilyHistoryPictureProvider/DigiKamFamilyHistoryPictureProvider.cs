using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.DataProviders;
using System;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public class DigiKamFamilyHistoryPictureProvider : IFamilyHistoryPictureProvider
    {
        private string _rootsMagicDbPath;
        private string _digiKamDbPath;
        private DigiKamConnector _connector;
        private bool _isInitialized;

        public void Initialize(Dictionary<string, string> configuration)
        {
            if (!configuration.TryGetValue("RootsMagicDbPath", out _rootsMagicDbPath))
            {
                throw new ArgumentException("RootsMagicDbPath not found in configuration");
            }

            if (!configuration.TryGetValue("DigiKamDbPath", out _digiKamDbPath))
            {
                throw new ArgumentException("DigiKamDbPath not found in configuration");
            }

            _connector = new DigiKamConnector(_rootsMagicDbPath, _digiKamDbPath);
            _isInitialized = true;
        }

        public List<Texture2D> GetThumbnailForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("DigiKamFamilyHistoryPictureProvider not initialized");
                return new List<Texture2D>();
            }

            var thumbnails = new List<Texture2D>();
            byte[] imageData = _connector.GetPrimaryThumbnailForPersonFromDataBase(personId);

            if (imageData != null)
            {
                var thumbnail = new Texture2D(2, 2);
                thumbnail.LoadImage(imageData);
                thumbnails.Add(thumbnail);
            }

            return thumbnails;
        }

        public List<(Texture2D Photo, Dictionary<string, string> Metadata)> GetPhotoListForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("DigiKamFamilyHistoryPictureProvider not initialized");
                return new List<(Texture2D, Dictionary<string, string>)>();
            }

            // For now, return mock data as requested
            var mockPhoto = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    mockPhoto.SetPixel(x, y, ((x + y) % 2 == 0) ? Color.white : Color.black);
                }
            }
            mockPhoto.Apply();

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