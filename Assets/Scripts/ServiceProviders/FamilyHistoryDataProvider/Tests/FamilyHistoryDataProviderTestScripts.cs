using NUnit.Framework;
using UnityEngine;
using FluentAssertions;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider.Tests
{
    public class FamilyHistoryDataProviderTestScripts
    {
        private static readonly int _ownerIDForJFK = 8;
        private string _rootsMagicDbPath;
        private IFamilyHistoryDataProvider _mockProvider;
        private IFamilyHistoryDataProvider _realProvider;

        [SetUp]
        public void Setup()
        {
            // Use the sample data paths
            string sampleDataPath = Path.Combine(Application.dataPath, "Resources", "SampleData");
            _rootsMagicDbPath = Path.Combine(sampleDataPath, "RootsMagic", "Kennedy.rmtree");

            // Initialize mock provider
            _mockProvider = new MockFamilyHistoryDataProvider();
            var mockConfig = new Dictionary<string, string>
            {
                { "RootsMagicDbPath", _rootsMagicDbPath }
            };
            _mockProvider.Initialize(mockConfig);

            // Initialize real provider
            _realProvider = new RootsMagicFamilyHistoryDataProvider();
            var realConfig = new Dictionary<string, string>
            {
                { "RootsMagicDbPath", _rootsMagicDbPath }
            };
            _realProvider.Initialize(realConfig);
        }

        [Test]
        public void GetPerson_WhenPersonExists_ReturnsCorrectPerson()
        {
            // Act
            var mockResult = _mockProvider.GetPerson(_ownerIDForJFK);
            var realResult = _realProvider.GetPerson(_ownerIDForJFK);

            // Assert
            mockResult.Should().NotBeNull("because the person exists in mock data");
            mockResult.Count.Should().Be(1, "because we expect exactly one person");
            mockResult[0].dataBaseOwnerId.Should().Be(_ownerIDForJFK, "because we requested JFK's ID");
            mockResult[0].givenName.Should().Be("John", "because that's JFK's given name");
            mockResult[0].surName.Should().Be("Kennedy", "because that's JFK's surname");

            realResult.Should().NotBeNull("because the person exists in the database");
            realResult.Count.Should().Be(1, "because we expect exactly one person");
            realResult[0].dataBaseOwnerId.Should().Be(_ownerIDForJFK, "because we requested JFK's ID");
        }

        [Test]
        public void GetPerson_WhenPersonDoesNotExist_ReturnsEmptyList()
        {
            // Act
            var mockResult = _mockProvider.GetPerson(999999);
            var realResult = _realProvider.GetPerson(999999);

            // Assert
            mockResult.Should().NotBeNull("because we should always get a list");
            mockResult.Count.Should().Be(0, "because the person ID does not exist in mock data");

            realResult.Should().NotBeNull("because we should always get a list");
            realResult.Count.Should().Be(0, "because the person ID does not exist in the database");
        }

        [Test]
        public void GetPersonList_WhenNoFilter_ReturnsListOfPersons()
        {
            // Act
            var mockResult = _mockProvider.GetPersonList(10);
            var realResult = _realProvider.GetPersonList(10);

            // Assert
            mockResult.Should().NotBeNull("because we should always get a list");
            mockResult.Count.Should().BeGreaterThan(0, "because we expect some mock data");

            realResult.Should().NotBeNull("because we should always get a list");
            realResult.Count.Should().BeGreaterThan(0, "because we expect some data in the database");
        }

        [Test]
        public void GetPersonListByLastName_WhenLastNameExists_ReturnsMatchingPersons()
        {
            // Act
            var mockResult = _mockProvider.GetPersonListByLastName("Kennedy", 10);
            var realResult = _realProvider.GetPersonListByLastName("Kennedy", 10);

            // Assert
            mockResult.Should().NotBeNull("because we should always get a list");
            mockResult.Count.Should().BeGreaterThan(0, "because we expect some Kennedys in mock data");
            mockResult.Should().OnlyContain(p => p.surName.Contains("Kennedy"), "because we filtered for Kennedys");

            realResult.Should().NotBeNull("because we should always get a list");
            realResult.Count.Should().BeGreaterThan(0, "because we expect some Kennedys in the database");
            realResult.Should().OnlyContain(p => p.surName.Contains("Kennedy"), "because we filtered for Kennedys");
        }

        [Test]
        public void ValidateDatabaseIntegrity_WhenDatabaseExists_ReturnsTrue()
        {
            // Act
            var mockResult = _mockProvider.ValidateDatabaseIntegrity();
            var realResult = _realProvider.ValidateDatabaseIntegrity();

            // Assert
            mockResult.Should().BeTrue("because mock always returns true");
            realResult.Should().BeTrue("because the database exists and is valid");
        }

        [TearDown]
        public void Cleanup()
        {
            _mockProvider = null;
            _realProvider = null;
        }
    }
} 