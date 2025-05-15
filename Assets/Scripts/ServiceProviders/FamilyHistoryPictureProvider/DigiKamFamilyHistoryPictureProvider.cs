using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.DataProviders;
using System;
using System.IO;
using Assets.Scripts.ServiceProviders;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public class DigiKamFamilyHistoryPictureProvider : IFamilyHistoryPictureProvider
    {       
        private string _rootsMagicDbPath;
        private string _digiKamDbPath;
        private DigiKamConnector _connector;
        private bool _isInitialized;
        private Dictionary<string, string> _configuration;
        private string _digiKamFolder;

        public void Initialize(Dictionary<string, string> configuration)
        {
            _configuration = configuration;
            if (!configuration.TryGetValue(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, out _rootsMagicDbPath))
            {
                Debug.LogError($"{PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH} not found in configuration");
            }
            if (!configuration.TryGetValue(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, out _digiKamDbPath))
            {
                Debug.LogError($"{PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH} not found in configuration");
            }
            _digiKamFolder = Path.GetDirectoryName(_digiKamDbPath);

            _connector = new DigiKamConnector(_rootsMagicDbPath, _digiKamDbPath);
            _isInitialized = true;
        }

        public bool AreAllDatabaseFilesPresent()
        {
            if (!File.Exists(_rootsMagicDbPath))
            {
                Debug.LogError($"RootsMagic database not found at: {_rootsMagicDbPath}");
                return false;
            }
            if (!File.Exists(_digiKamDbPath))
            {
                Debug.LogError($"DigiKam database not found at: {_digiKamDbPath}");
                return false;
            }
            return true;
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

        public List<DigiKamConnector.PhotoInfo> GetPhotoInfoListForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("DigiKamFamilyHistoryPictureProvider not initialized");
                return new List<DigiKamConnector.PhotoInfo>();
            }
            return _connector.GetPhotoInfoListForPersonFromDataBase(personId);
        }

        public DigiKamConnector.PhotoInfo GetThumbnailPhotoInfoForPerson(int personId, int year)
        {
            if (!_isInitialized)
            {
                Debug.LogError("DigiKamFamilyHistoryPictureProvider not initialized");
                return null;
            }
            var photoInfo = _connector.GetPhotoInfoForPrimaryThumbnailForPersonFromDataBase(personId);
           
            return photoInfo;
        }
    }
} 