using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;

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
            var configuration = new Dictionary<string, string>
            {
                { "RootsMagicDbPath", _rootsMagicDbPath },
                { "DigiKamDbPath", _digiKamDbPath }
            };
            _provider.Initialize(configuration);
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
        public void GetPhotoListForPerson_WhenPersonExists_ReturnsMockData()
        {
            // Act
            var result = _provider.GetPhotoListForPerson(_ownerIDForJFK, 1963);

            // Assert
            result.Should().NotBeNull("because we should always get a list");
            result.Count.Should().Be(1, "because we expect one mock photo");
            
            var (photo, metadata) = result[0];
            photo.Should().NotBeNull("because we should have a valid photo");
            metadata.Should().NotBeNull("because we should have metadata");
            metadata.Count.Should().Be(4, "because we expect 4 metadata fields");
            metadata["Date"].Should().Be("1963-01-01", "because the year should be reflected in the date");
        }

        [TearDown]
        public void Cleanup()
        {
            _provider = null;
        }
    }
} 