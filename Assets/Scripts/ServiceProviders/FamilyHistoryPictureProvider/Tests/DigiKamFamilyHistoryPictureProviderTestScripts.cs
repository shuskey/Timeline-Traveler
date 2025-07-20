using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.DataObjects;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using System.Linq;
using Assets.Scripts.Utilities;
using Assets.Scripts.Enums;
using System.Threading.Tasks;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public class DigiKamFamilyHistoryPictureProviderTestScripts
    {
        private static readonly int _ownerIDForJFK = 8;
        private string _rootsMagicDbPath;
        private string _digiKamDbPath;
        private string _digiKamFolder;
        private DigiKamFamilyHistoryPictureProvider _provider;

        [SetUp]
        public void Setup()
        {
            // Use the sample data paths
            string sampleDataPath = Path.Combine(Application.dataPath, "Resources", "SampleData");
            _digiKamFolder = Path.Combine(sampleDataPath, "DigiKam");
            _rootsMagicDbPath = Path.Combine(sampleDataPath, "RootsMagic", "Kennedy.rmtree");
            _digiKamDbPath = Path.Combine(_digiKamFolder, "digikam4.db");

            // Create and initialize the provider with sample data
            _provider = new DigiKamFamilyHistoryPictureProvider();
            var config = new Dictionary<string, string>
            {
                { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, _rootsMagicDbPath },
                { PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, _digiKamDbPath }
            };
            _provider.Initialize(config);
        }

        [Test]
        public void GetThumbnailForPerson_WhenPersonExists_ReturnsPhotoInfo()
        {
            // Act
            var result = _provider.GetThumbnailPhotoInfoForPerson(_ownerIDForJFK, 1963);

            // Assert
            result.Should().NotBeNull("because the person exists and has a face region defined");
            result.FullPathToFileName.Should().NotBeNull("because the person exists and has a face region defined");

             }

        [Test]
        public void GetThumbnailForPerson_WhenPersonDoesNotExist_ReturnsNothing()
        {
            // Act
            var result = _provider.GetThumbnailPhotoInfoForPerson(999999, 1963);

            // Assert
            result.Should().BeNull("because the person ID does not exist in the database");
        }

        [Test]
        public void GetPhotoListForPerson_WhenPersonExists_ReturnsSampleData()
        {
            // Act
            var result = _provider.GetPhotoInfoListForPerson(_ownerIDForJFK, null);  // null means all photos

            // Assert
            result.Should().NotBeNull("because we should always get a list");
            result.Count.Should().Be(3, "because we expect 3 sample photos photo");
            
            var photoInfo = result[0];
            photoInfo.Should().NotBeNull("because we should have a valid photo");
            photoInfo.Region.Should().NotBeNull("because we expect a region to be defined");
        }

        [TearDown]
        public void Cleanup()
        {
            _provider = null;
        }
    }
} 