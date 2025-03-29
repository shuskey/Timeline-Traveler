using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    public class RootsMagicFamilyHistoryDataProvider : IFamilyHistoryDataProvider
    {
        private Dictionary<string, string> _configuration;
        private string _rootsMagicDbPath;

        public void Initialize(Dictionary<string, string> configuration)
        {
            _configuration = configuration;
            if (!configuration.TryGetValue("RootsMagicDbPath", out _rootsMagicDbPath))
            {
                Debug.LogError("RootsMagicDbPath not found in configuration");
            }
        }

        public List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            return GetPersonList(1, ownerId, generation, xOffset, spouseNumber);
        }

        public List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            string conn = "URI=file:" + _rootsMagicDbPath;

            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    string query = BuildPersonQuery(justThisOwnerId);
                    dbcmd.CommandText = query;

                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        int currentArrayIndex = 0;
                        while (reader.Read() && currentArrayIndex < limitListSizeTo)
                        {
                            var person = CreatePersonFromReader(reader, currentArrayIndex, generation, xOffset, spouseNumber);
                            result.Add(person);
                            currentArrayIndex++;
                        }
                    }
                }
            }

            return result;
        }

        public List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            string conn = "URI=file:" + _rootsMagicDbPath;

            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    string query = BuildPersonQueryByLastName(lastNameFilter);
                    dbcmd.CommandText = query;

                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        int currentArrayIndex = 0;
                        while (reader.Read() && currentArrayIndex < limitListSizeTo)
                        {
                            var person = CreatePersonFromReader(reader, currentArrayIndex, generation, xOffset, spouseNumber);
                            result.Add(person);
                            currentArrayIndex++;
                        }
                    }
                }
            }

            return result.OrderBy(x => x.surName + " " + x.givenName).ToList();
        }

        public bool ValidateDatabaseIntegrity()
        {
            try
            {
                string conn = "URI=file:" + _rootsMagicDbPath;
                using (IDbConnection dbconn = new SqliteConnection(conn))
                {
                    dbconn.Open();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Database integrity check failed: {ex.Message}");
                return false;
            }
        }

        private string BuildPersonQuery(int? justThisOwnerId = null)
        {
            string query = @"
                SELECT  name.OwnerID 
                     , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end 
                     , name.Given, name.Surname 
                     , CASE WHEN SUBSTR(eventBirth.Date,8,2) THEN SUBSTR(eventBirth.Date,8,2) ELSE ""0"" END AS BirthMonth 
                     , CASE WHEN SUBSTR(eventBirth.Date, 10, 2) THEN SUBSTR(eventBirth.Date,10,2) ELSE ""0"" END AS BirthdDay 
                     , CASE WHEN SUBSTR(eventBirth.Date,4,4) THEN 
                           CASE WHEN SUBSTR(eventBirth.Date, 4, 4) != ""0"" THEN SUBSTR(eventBirth.Date,4,4) END 
                           ELSE CAST(name.BirthYear as varchar(10)) END AS BirthYear 
                     , person.Living 
                     , CASE WHEN SUBSTR(eventDeath.Date,8,2) THEN SUBSTR(eventDeath.Date,8,2) ELSE ""0"" END AS DeathMonth 
                     , CASE WHEN SUBSTR(eventDeath.Date,10,2) THEN SUBSTR(eventDeath.Date,10,2) ELSE ""0"" END AS DeathdDay 
                     , CASE WHEN SUBSTR(eventDeath.Date,4,4) THEN SUBSTR(eventDeath.Date,4,4) ELSE ""0"" END AS DeathYear 
                FROM NameTable name 
                JOIN PersonTable person 
                    ON name.OwnerID = person.PersonID 
                LEFT JOIN EventTable eventBirth ON name.OwnerID = eventBirth.OwnerID AND eventBirth.EventType = 1 
                LEFT JOIN EventTable eventDeath 
                    ON name.OwnerID = eventDeath.OwnerID AND eventDeath.EventType = 2";

            if (justThisOwnerId.HasValue)
            {
                query += $" WHERE name.OwnerID = \"{justThisOwnerId}\" LIMIT 1;";
            }

            return query;
        }

        private string BuildPersonQueryByLastName(string lastNameFilter)
        {
            return @"
                SELECT  name.OwnerID 
                     , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end 
                     , name.Given, name.Surname 
                     , CAST(name.BirthYear as varchar(10)) AS BirthYear 
                FROM NameTable name 
                JOIN PersonTable person 
                    ON name.OwnerID = person.PersonID" +
                $" WHERE name.Surname LIKE \"%{lastNameFilter}%\";";
        }

        private Person CreatePersonFromReader(IDataReader reader, int arrayIndex, int generation, float xOffset, int spouseNumber)
        {
            var ownerId = reader.GetInt32(0);
            var gender = charToPersonGenderType(reader.GetString(1)[0]);
            var given = reader.GetString(2);
            var surname = reader.GetString(3);

            // Handle different query results based on the query type
            if (reader.FieldCount > 5)
            {
                // Full person query
                return new Person(
                    arrayIndex: arrayIndex,
                    ownerId: ownerId,
                    gender: gender,
                    given: given,
                    surname: surname,
                    birthMonth: StringToNumberProtected(reader.GetString(4), $"birthMonth as GetString(4) for OwnerId: {ownerId}."),
                    birthDay: StringToNumberProtected(reader.GetString(5), $"birthDay as GetString(5) for OwnerId: {ownerId}."),
                    birthYear: StringToNumberProtected(reader.GetString(6), $"birthYear as GetString(6) for OwnerId: {ownerId}."),
                    isLiving: reader.GetBoolean(7),
                    deathMonth: StringToNumberProtected(reader.GetString(8), $"deathMonth as GetString(8) for OwnerId: {ownerId}."),
                    deathDay: StringToNumberProtected(reader.GetString(9), $"deathDay as GetString(9) for OwnerId: {ownerId}."),
                    deathYear: StringToNumberProtected(reader.GetString(10), $"deathYear as GetString(10) for OwnerId: {ownerId}."),
                    generation: generation,
                    xOffset: xOffset,
                    spouseNumber: spouseNumber
                );
            }
            else
            {
                // Last name filter query
                return new Person(
                    arrayIndex: arrayIndex,
                    ownerId: ownerId,
                    gender: gender,
                    given: given,
                    surname: surname,
                    birthYear: StringToNumberProtected(reader.GetString(4), $"birthYear as GetString(4) for OwnerId: {ownerId}."),
                    deathYear: 0,
                    isLiving: false,
                    generation: generation,
                    xOffset: xOffset,
                    spouseNumber: spouseNumber
                );
            }
        }

        private PersonGenderType charToPersonGenderType(char sex) =>
            sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);

        private int StringToNumberProtected(string value, string errorContext)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            Debug.LogWarning($"Failed to parse number in {errorContext}");
            return 0;
        }
    }
} 