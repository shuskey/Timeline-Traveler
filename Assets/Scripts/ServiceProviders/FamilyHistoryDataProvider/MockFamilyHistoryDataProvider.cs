using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    /// <summary>
    /// Mock implementation of IFamilyHistoryDataProvider for testing purposes
    /// </summary>
    public class MockFamilyHistoryDataProvider : IFamilyHistoryDataProvider
    {
        private Dictionary<string, string> _configuration;
        private List<Person> _mockPersons;
        private List<Parentage> _mockParentages;
        private List<Marriage> _mockMarriages;

        public MockFamilyHistoryDataProvider()
        {
            _mockPersons = new List<Person>
            {
                new Person(
                    arrayIndex: 0,
                    ownerId: 8,
                    gender: PersonGenderType.Male,
                    given: "John",
                    surname: "Kennedy",
                    isLiving: false,
                    birthMonth: 1,
                    birthDay: 1,
                    birthYear: 1900,
                    deathMonth: 1,
                    deathDay: 1,
                    deathYear: 1960,
                    generation: 1,
                    xOffset: 0,
                    spouseNumber: 0
                ),
                new Person(
                    arrayIndex: 1,
                    ownerId: 2,
                    gender: PersonGenderType.Female,
                    given: "Jane",
                    surname: "Smith",
                    isLiving: false,
                    birthMonth: 1,
                    birthDay: 1,
                    birthYear: 1905,
                    deathMonth: 1,
                    deathDay: 1,
                    deathYear: 1965,
                    generation: 1,
                    xOffset: 0,
                    spouseNumber: 0
                ),
                new Person(
                    arrayIndex: 2,
                    ownerId: 3,
                    gender: PersonGenderType.Male,
                    given: "Bob",
                    surname: "Johnson",
                    isLiving: false,
                    birthMonth: 1,
                    birthDay: 1,
                    birthYear: 1930,
                    deathMonth: 1,
                    deathDay: 1,
                    deathYear: 1990,
                    generation: 2,
                    xOffset: 0,
                    spouseNumber: 0
                )
            };

            _mockParentages = new List<Parentage>
            {
                new Parentage(1, 1, 2, 3, ChildRelationshipType.Biological, ChildRelationshipType.Biological)
            };

            _mockMarriages = new List<Marriage>
            {
                new Marriage(1, 1, 2, 6, 15, 1925, 0, 0)
            };
        }

        public void Initialize(Dictionary<string, string> configuration)
        {
            Debug.Log("Initializing Mock Family History Data Provider");
            // Log how many Persons are in the _mockPersons list
            Debug.Log($"Number of Persons in _mockPersons: {_mockPersons.Count}");
            _configuration = configuration;
        }

        public List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var person = _mockPersons.Find(p => p.dataBaseOwnerId == ownerId);
            return person != null ? new List<Person> { person } : new List<Person>();
        }

        public List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            foreach (var person in _mockPersons)
            {
                if (justThisOwnerId.HasValue && person.dataBaseOwnerId != justThisOwnerId.Value)
                    continue;

                result.Add(person);
                if (result.Count >= limitListSizeTo)
                    break;
            }
            return result;
        }

        public List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            foreach (var person in _mockPersons)
            {
                if (person.surName != lastNameFilter)
                    continue;

                result.Add(person);
                if (result.Count >= limitListSizeTo)
                    break;
            }
            return result;
        }

        public List<Parentage> GetChildren(int familyId)
        {
            return _mockParentages.FindAll(p => p.familyId == familyId);
        }

        public List<Marriage> GetMarriages(int ownerId, bool useHusbandQuery = true)
        {
            return _mockMarriages.FindAll(m => 
                (useHusbandQuery && m.husbandId == ownerId) || 
                (!useHusbandQuery && m.wifeId == ownerId));
        }

        public List<Parentage> GetParents(int childId)
        {
            return _mockParentages.FindAll(p => p.childId == childId);
        }

        public bool ValidateDatabaseIntegrity()
        {
            return true; // Mock implementation always returns true
        }
    }
} 