using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using Assets.Scripts.ServiceProviders;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using System.Linq;

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
        public void GetThumbnailForPerson_WhenPersonExists_ReturnsCroppedImage()
        {
            // Act
            var result = _provider.GetThumbnailForPerson(_ownerIDForJFK, 1963);

            // Assert
            result.Should().NotBeNull("because the person exists and has a face region defined");
            result.Count.Should().Be(1, "because we expect exactly one thumbnail");
            
            // Verify the image data
            var resultTexture = result[0];
            resultTexture.Should().NotBeNull("because the returned data should be valid PNG image data");
            
            // The resulting image should be a reasonable face crop size
            resultTexture.width.Should().BeGreaterThan(0, "because we should have a valid face region");
            resultTexture.height.Should().BeGreaterThan(0, "because we should have a valid face region");
        }

        [Test]
        public void GetThumbnailForPerson_WhenPersonDoesNotExist_ReturnsEmptyList()
        {
            // Act
            var result = _provider.GetThumbnailForPerson(999999, 1963);

            // Assert
            result.Should().NotBeNull("because we should always get a list");
            result.Count.Should().Be(0, "because the person ID does not exist in the database");
        }

        [Test]
        public void GetPhotoListForPerson_WhenPersonExists_ReturnsSampleData()
        {
            // Act
            var result = _provider.GetPhotoInfoListForPerson(_ownerIDForJFK, 1963);

            // Assert
            result.Should().NotBeNull("because we should always get a list");
            result.Count.Should().Be(3, "because we expect 3 sample photos photo");
            
            var photoInfo = result[0];
            photoInfo.Should().NotBeNull("because we should have a valid photo");
            photoInfo.Region.Should().NotBeNull("because we expect a region to be defined");
            photoInfo.Orientation.Should().NotBeNull("because we expect a orientation to be defined");
        }

        [TearDown]
        public void Cleanup()
        {
            _provider = null;
        }
    }
} 