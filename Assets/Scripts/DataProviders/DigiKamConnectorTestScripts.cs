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
        private string _testRootsMagicDbPath;
        private string _testDigiKamDbPath;
        private string _testDigiKamFolder;
        private DigiKamConnector _connector;

        [SetUp]
        public void Setup()
        {
            // Create a temporary test directory
            _testDigiKamFolder = Path.Combine(Path.GetTempPath(), "DigiKamTest_" + System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDigiKamFolder);

            // Set up test file paths
            _testRootsMagicDbPath = Path.Combine(_testDigiKamFolder, "test.rmtree");
            _testDigiKamDbPath = Path.Combine(_testDigiKamFolder, "digikam4.db");

            // Create the connector
            _connector = new DigiKamConnector(_testRootsMagicDbPath, _testDigiKamDbPath);
        }

        [Test]
        public void AreAllDatabaseFilesPresent_WhenAllFilesExist_ReturnsTrue()
        {
            // Arrange
            File.WriteAllText(_testRootsMagicDbPath, "test");
            File.WriteAllText(_testDigiKamDbPath, "test");
            File.WriteAllText(Path.Combine(_testDigiKamFolder, "rootsmagic-digikam.db"), "test");
            File.WriteAllText(Path.Combine(_testDigiKamFolder, "thumbnails-digikam.db"), "test");

            // Act
            var result = _connector.AreAllDatabaseFilesPresent();

            // Assert
            result.Should().BeTrue("because all required database files exist");
        }

        [Test]
        [TestCase("test.rmtree", false, "RootsMagic database")]
        [TestCase("digikam4.db", false, "DigiKam database")]
        [TestCase("rootsmagic-digikam.db", true, "RootsMagic to DigiKam database")]
        [TestCase("thumbnails-digikam.db", true, "DigiKam Thumbnails database")]
        public void AreAllDatabaseFilesPresent_WhenOneFileMissing_ReturnsFalse(string missingFile, bool isInDigiKamFolder, string fileDescription)
        {
            // Arrange
            if (missingFile != "test.rmtree")
                File.WriteAllText(_testRootsMagicDbPath, "test");
            if (missingFile != "digikam4.db")
                File.WriteAllText(_testDigiKamDbPath, "test");
            if (missingFile != "rootsmagic-digikam.db")
                File.WriteAllText(Path.Combine(_testDigiKamFolder, "rootsmagic-digikam.db"), "test");
            if (missingFile != "thumbnails-digikam.db")
                File.WriteAllText(Path.Combine(_testDigiKamFolder, "thumbnails-digikam.db"), "test");

            // Act
            var result = _connector.AreAllDatabaseFilesPresent();

            // Assert
            result.Should().BeFalse($"because the {fileDescription} is missing");
        }

        [Test]
        public void GetListOfFaceTagIdsFromDataBase_WhenDataExists_ReturnsCorrectFaceTags()
        {
            // Arrange
            var testDbPath = Path.Combine(_testDigiKamFolder, "rootsmagic-digikam.db");
            CreateTestDatabase(testDbPath);

            // Act
            _connector.GetListOfFaceTagIdsFromDataBase();

            // Assert
            _connector.faceTagList.Should().HaveCount(2, "because we inserted 2 test records");
            _connector.faceTagList.Should().Contain(x => x.personId == 1 && x.tagId == 100 && x.tagName == "Test Person 1");
            _connector.faceTagList.Should().Contain(x => x.personId == 2 && x.tagId == 200 && x.tagName == "Test Person 2");
        }

        private void CreateTestDatabase(string dbPath)
        {
            using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Create the table
                    cmd.CommandText = @"
                        CREATE TABLE PersonDigiKamTag (
                            PersonID INTEGER,
                            TagID INTEGER,
                            Name TEXT
                        )";
                    cmd.ExecuteNonQuery();

                    // Insert test data
                    cmd.CommandText = @"
                        INSERT INTO PersonDigiKamTag (PersonID, TagID, Name) VALUES 
                        (1, 100, 'Test Person 1'),
                        (2, 200, 'Test Person 2')";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CreateDigiKamDatabase()
        {
            using (var conn = new SqliteConnection($"URI=file:{_testDigiKamDbPath}"))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Create required tables
                    cmd.CommandText = @"
                        CREATE TABLE PersonDigiKamTag (
                            PersonID INTEGER,
                            TagID INTEGER,
                            Name TEXT
                        );
                        CREATE TABLE Tags (
                            id INTEGER PRIMARY KEY,
                            name TEXT,
                            icon INTEGER
                        );
                        CREATE TABLE Images (
                            id INTEGER PRIMARY KEY,
                            name TEXT,
                            album INTEGER
                        );
                        CREATE TABLE Albums (
                            id INTEGER PRIMARY KEY,
                            relativePath TEXT
                        );
                        CREATE TABLE ImageTagProperties (
                            imageid INTEGER,
                            tagid INTEGER,
                            value TEXT
                        );
                        CREATE TABLE AlbumRoots (
                            id INTEGER PRIMARY KEY,
                            label TEXT,
                            specificPath TEXT
                        );";
                    cmd.ExecuteNonQuery();

                    // Insert test data
                    cmd.CommandText = @"
                        INSERT INTO PersonDigiKamTag (PersonID, TagID, Name) VALUES (1, 100, 'Test Person');
                        INSERT INTO Tags (id, name, icon) VALUES (100, 'Test Person', 1);
                        INSERT INTO Images (id, name, album) VALUES (1, 'test_image.jpg', 1);
                        INSERT INTO Albums (id, relativePath) VALUES (1, '/TestAlbum');
                        INSERT INTO AlbumRoots (id, label, specificPath) VALUES (1, 'Pictures', '/Pictures');
                        INSERT INTO ImageTagProperties (imageid, tagid, value) VALUES (1, 100, 
                            '<region x=""0.1"" y=""0.1"" width=""0.2"" height=""0.2""/>');";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [TearDown]
        public void Cleanup()
        {
            _connector = null;
            
            // Clean up test files
            if (Directory.Exists(_testDigiKamFolder))
            {
                try
                {
                    Directory.Delete(_testDigiKamFolder, true);
                }
                catch (IOException)
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
} 