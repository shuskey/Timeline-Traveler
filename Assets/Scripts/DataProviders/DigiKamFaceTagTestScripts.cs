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

        [TearDown]
        public void Cleanup()
        {
            _connector = null;
        }
    }
} 