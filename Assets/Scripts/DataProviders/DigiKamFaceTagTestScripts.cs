using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using Assets.Scripts.DataProviders;
using System.IO;
using Mono.Data.Sqlite;
using System.Data;

namespace Assets.Scripts.DataProviders
{
    public class DigiKamFaceTagTestScripts
    {
        private static readonly int _ownerIDForJFK = 8;
        private string _rootsMagicDbPath;
        private string _digiKamDbPath;
        private string _digiKamFolder;
        private DigiKamConnector _connector;

        [SetUp]
        public void Setup()
        {
            // Use the sample data paths
            string sampleDataPath = Path.Combine(Application.dataPath, "Resources", "SampleData");
            _digiKamFolder = Path.Combine(sampleDataPath, "DigiKam");
            _rootsMagicDbPath = Path.Combine(sampleDataPath, "RootsMagic", "Kennedy.rmtree");
            _digiKamDbPath = Path.Combine(_digiKamFolder, "digikam4.db");

            // Create the connector with sample data
            _connector = new DigiKamConnector(_rootsMagicDbPath, _digiKamDbPath);
        }

        [Test]
        public void GetPrimaryThumbnailForPersonFromDataBase_WhenPersonExists_ReturnsCroppedImage()
        {
            // Act
            // Use a known person ID from your Kennedy database
            var result = _connector.GetPrimaryThumbnailForPersonFromDataBase(_ownerIDForJFK);

            // Assert
            result.Should().NotBeNull("because the person exists and has a face region defined");
            
            // Verify the image data
            var resultTexture = new Texture2D(2, 2);
            resultTexture.LoadImage(result);
            resultTexture.Should().NotBeNull("because the returned data should be valid PNG image data");
            
            // The resulting image should be a reasonable face crop size
            resultTexture.width.Should().BeGreaterThan(0, "because we should have a valid face region");
            resultTexture.height.Should().BeGreaterThan(0, "because we should have a valid face region");
              UnityEngine.Object.DestroyImmediate(resultTexture);
        }

        [Test]
        public void GetPrimaryThumbnailForPersonFromDataBase_WhenPersonDoesNotExist_ReturnsNull()
        {
            // Act
            // Use an ID we know doesn't exist in the sample database
            var result = _connector.GetPrimaryThumbnailForPersonFromDataBase(999999);

            // Assert
            result.Should().BeNull("because the person ID does not exist in the database");
        }


        [TearDown]
        public void Cleanup()
        {
            _connector = null;
        }
    }
} 