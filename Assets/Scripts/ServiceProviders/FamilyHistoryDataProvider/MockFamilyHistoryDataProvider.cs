using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    public class MockFamilyHistoryDataProvider : IFamilyHistoryDataProvider
    {
        private Dictionary<string, string> _configuration;
        private List<Person> _mockPersons;

        public void Initialize(Dictionary<string, string> configuration)
        {
            _configuration = configuration;
            _mockPersons = new List<Person>
            {
                new Person(
                    arrayIndex: 0,
                    ownerId: 8,
                    gender: PersonGenderType.Male,
                    given: "John",
                    surname: "Kennedy",
                    birthMonth: 5,
                    birthDay: 29,
                    birthYear: 1917,
                    isLiving: false,
                    deathMonth: 11,
                    deathDay: 22,
                    deathYear: 1963,
                    generation: 0,
                    xOffset: 0,
                    spouseNumber: 0
                )
            };
        }

        public List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var person = _mockPersons.Find(p => p.dataBaseOwnerId == ownerId);
            if (person != null)
            {
                person.generation = generation;
                person.xOffset = xOffset;
                person.spouseNumber = spouseNumber;
                return new List<Person> { person };
            }
            return new List<Person>();
        }

        public List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            if (justThisOwnerId.HasValue)
            {
                var person = _mockPersons.Find(p => p.dataBaseOwnerId == justThisOwnerId.Value);
                if (person != null)
                {
                    person.generation = generation;
                    person.xOffset = xOffset;
                    person.spouseNumber = spouseNumber;
                    result.Add(person);
                }
            }
            else
            {
                foreach (var person in _mockPersons)
                {
                    if (result.Count >= limitListSizeTo) break;
                    var newPerson = new Person(
                        arrayIndex: result.Count,
                        ownerId: person.dataBaseOwnerId,
                        gender: person.gender,
                        given: person.givenName,
                        surname: person.surName,
                        birthMonth: person.originalBirthEventDateMonth,
                        birthDay: person.originalBirthEventDateDay,
                        birthYear: person.originalBirthEventDateYear,
                        isLiving: person.isLiving,
                        deathMonth: person.originalDeathEventDateMonth,
                        deathDay: person.originalDeathEventDateDay,
                        deathYear: person.originalDeathEventDateYear,
                        generation: generation,
                        xOffset: xOffset,
                        spouseNumber: spouseNumber
                    );
                    result.Add(newPerson);
                }
            }
            return result;
        }

        public List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            foreach (var person in _mockPersons)
            {
                if (result.Count >= limitListSizeTo) break;
                if (person.surName.Contains(lastNameFilter))
                {
                    var newPerson = new Person(
                        arrayIndex: result.Count,
                        ownerId: person.dataBaseOwnerId,
                        gender: person.gender,
                        given: person.givenName,
                        surname: person.surName,
                        birthMonth: person.originalBirthEventDateMonth,
                        birthDay: person.originalBirthEventDateDay,
                        birthYear: person.originalBirthEventDateYear,
                        isLiving: person.isLiving,
                        deathMonth: person.originalDeathEventDateMonth,
                        deathDay: person.originalDeathEventDateDay,
                        deathYear: person.originalDeathEventDateYear,
                        generation: generation,
                        xOffset: xOffset,
                        spouseNumber: spouseNumber
                    );
                    result.Add(newPerson);
                }
            }
            return result.OrderBy(x => x.surName + " " + x.givenName).ToList();
        }

        public bool ValidateDatabaseIntegrity()
        {
            return true; // Mock always returns true
        }
    }
} 