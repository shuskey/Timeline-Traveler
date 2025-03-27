using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using Assets.Scripts.DataProviders;
using System.IO;
using Mono.Data.Sqlite;
using System.Data;

namespace Assets.Scripts.DataProviders
{
    public class DigiKamConnectorTestScripts
    {
        private string _rootsMagicDbPath;
        private string _digiKamDbPath;
        private DigiKamConnector _connector;

        [SetUp]
        public void Setup()
        {
            // Use the sample data paths
            string sampleDataPath = Path.Combine(Application.dataPath, "Resources", "SampleData");
            _rootsMagicDbPath = Path.Combine(sampleDataPath, "RootsMagic", "Kennedy.rmtree");
            _digiKamDbPath = Path.Combine(sampleDataPath, "DigiKam", "digikam4.db");

            // Create the connector with sample data
            _connector = new DigiKamConnector(_rootsMagicDbPath, _digiKamDbPath);
        }

        [Test]
        public void AreAllDatabaseFilesPresent_WhenUsingRealData_ReturnsTrue()
        {
            // Act
            var result = _connector.AreAllDatabaseFilesPresent();

            // Assert
            result.Should().BeTrue("because all required database files exist in the sample data");
        }


        [TearDown]
        public void Cleanup()
        {
            _connector = null;
        }
    }
} 