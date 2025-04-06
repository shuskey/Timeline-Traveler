using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using Assets.Scripts.ServiceProviders;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    /// <summary>
    /// Implementation of IFamilyHistoryDataProvider that uses the RootsMagic database
    /// </summary>
    public class RootsMagicFamilyHistoryDataProvider : IFamilyHistoryDataProvider
    {
        private string _databasePath;
        private IDbConnection _dbConnection;

        public void Initialize(Dictionary<string, string> configuration)
        {
            if (!configuration.TryGetValue(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, out _databasePath))
            {
                throw new System.ArgumentException($"{PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH} not found in configuration");
            }

            string conn = "URI=file:" + _databasePath;
            _dbConnection = new SqliteConnection(conn);
            _dbConnection.Open();
            
            // Register the RMNOCASE collation
            RegisterRMNOCASECollation();
        }
        
        private void RegisterRMNOCASECollation()
        {
            // Instead of trying to create a custom collation,
            // we'll modify our queries to use NOCASE collation which is built into SQLite
            using (var cmd = _dbConnection.CreateCommand())
            {
                // Test if we can use NOCASE collation
                cmd.CommandText = "SELECT 'test' = 'TEST' COLLATE NOCASE";
                try
                {
                    cmd.ExecuteScalar();
                    Debug.Log("Using built-in NOCASE collation as fallback for RMNOCASE");
                }
                catch (SqliteException ex)
                {
                    Debug.LogError($"Failed to setup case-insensitive collation: {ex.Message}");
                }
            }
        }

        public List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT OwnerID, Gender, Given, Surname, BirthMonth, BirthDay, BirthYear,
                           IsLiving, DeathMonth, DeathDay, DeathYear
                    FROM NameTable
                    WHERE OwnerID = @ownerId";

                var param = cmd.CreateParameter();
                param.ParameterName = "@ownerId";
                param.Value = ownerId;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result.Add(new Person(
                            arrayIndex: 0,
                            ownerId: reader.GetInt32(0),
                            gender: (PersonGenderType)reader.GetInt32(1),
                            given: reader.GetString(2),
                            surname: reader.GetString(3),
                            isLiving: reader.GetBoolean(7),
                            birthMonth: reader.GetInt32(4),
                            birthDay: reader.GetInt32(5),
                            birthYear: reader.GetInt32(6),
                            deathMonth: reader.GetInt32(8),
                            deathDay: reader.GetInt32(9),
                            deathYear: reader.GetInt32(10),
                            generation: generation,
                            xOffset: xOffset,
                            spouseNumber: spouseNumber
                        ));
                    }
                }
            }
            return result;
        }

        public List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT OwnerID, Gender, Given, Surname, BirthMonth, BirthDay, BirthYear,
                           IsLiving, DeathMonth, DeathDay, DeathYear
                    FROM NameTable
                    WHERE (@ownerId IS NULL OR OwnerID = @ownerId)
                    ORDER BY Surname, Given
                    LIMIT @limit";

                var ownerParam = cmd.CreateParameter();
                ownerParam.ParameterName = "@ownerId";
                ownerParam.Value = justThisOwnerId ?? (object)System.DBNull.Value;
                cmd.Parameters.Add(ownerParam);

                var limitParam = cmd.CreateParameter();
                limitParam.ParameterName = "@limit";
                limitParam.Value = limitListSizeTo;
                cmd.Parameters.Add(limitParam);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read() && result.Count < limitListSizeTo)
                    {
                        result.Add(new Person(
                            arrayIndex: result.Count,
                            ownerId: reader.GetInt32(0),
                            gender: (PersonGenderType)reader.GetInt32(1),
                            given: reader.GetString(2),
                            surname: reader.GetString(3),
                            isLiving: reader.GetBoolean(7),
                            birthMonth: reader.GetInt32(4),
                            birthDay: reader.GetInt32(5),
                            birthYear: reader.GetInt32(6),
                            deathMonth: reader.GetInt32(8),
                            deathDay: reader.GetInt32(9),
                            deathYear: reader.GetInt32(10),
                            generation: generation,
                            xOffset: xOffset,
                            spouseNumber: spouseNumber
                        ));
                    }
                }
            }
            return result;
        }

        private int StringToNumberProtected(string value, string errorContext)
        {
            if (string.IsNullOrEmpty(value) || value == "0")
                return 0;
            
            if (int.TryParse(value, out int result))
                return result;
                
            Debug.LogWarning($"Failed to parse number in {errorContext}. Value: {value}");
            return 0;
        }

        public List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0)
        {
            var result = new List<Person>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT name.OwnerID
                        , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end
                        , name.Given, name.Surname
                        , CAST(name.BirthYear as varchar(10)) AS BirthYear
                    FROM NameTable name
                    JOIN PersonTable person
                    ON name.OwnerID = person.PersonID
                    WHERE Surname COLLATE NOCASE LIKE @lastNameFilter
                    ORDER BY Surname COLLATE NOCASE, Given COLLATE NOCASE
                    LIMIT @limit";

                var lastNameParam = cmd.CreateParameter();
                lastNameParam.ParameterName = "@lastNameFilter";
                lastNameParam.Value = "%" + lastNameFilter + "%";
                cmd.Parameters.Add(lastNameParam);

                var limitParam = cmd.CreateParameter();
                limitParam.ParameterName = "@limit";
                limitParam.Value = limitListSizeTo;
                cmd.Parameters.Add(limitParam);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read() && result.Count < limitListSizeTo)
                    {
                        var ownerId = reader.GetInt32(0);
                        result.Add(new Person(
                            arrayIndex: result.Count,
                            ownerId: ownerId,
                            gender: charToPersonGenderType(reader.GetString(1)[0]),
                            given: reader.GetString(2),
                            surname: reader.GetString(3),
                            birthYear: StringToNumberProtected(reader.GetString(4), $"birthYear as GetString(4) for OwnerId: {ownerId}."),   
                            deathYear: 0,
                            isLiving: false,
                            generation: generation,
                            xOffset: xOffset,
                            spouseNumber: spouseNumber));
                    }
                }
            }
            return result;
            
            PersonGenderType charToPersonGenderType(char sex) =>
                sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);

        }

        public List<Parentage> GetChildren(int familyId)
        {
            var result = new List<Parentage>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT family.FamilyID, family.FatherID, family.MotherID, 
                           children.ChildID, children.RelFather, children.RelMother
                    FROM FamilyTable family
                    JOIN NameTable father ON family.FatherID = father.OwnerID
                    JOIN NameTable mother ON family.MotherID = mother.OwnerID
                    JOIN ChildTable children ON family.FamilyID = children.FamilyID
                    JOIN NameTable child ON children.ChildID = child.OwnerID
                    WHERE family.FamilyID = @familyId
                    ORDER BY children.ChildOrder ASC";

                var param = cmd.CreateParameter();
                param.ParameterName = "@familyId";
                param.Value = familyId;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Parentage(
                            familyId: reader.GetInt32(0),
                            fatherId: reader.GetInt32(1),
                            motherId: reader.GetInt32(2),
                            childId: reader.GetInt32(3),
                            relationToFather: reader.GetInt32(4) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted,
                            relationToMother: reader.GetInt32(5) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted
                        ));
                    }
                }
            }
            return result;
        }

        public List<Marriage> GetMarriages(int ownerId, bool useHusbandQuery = true)
        {
            var result = new List<Marriage>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                string whereIdTypeToUse = useHusbandQuery ? "FatherID" : "MotherID";
                cmd.CommandText = $@"
                    SELECT FM.FamilyID, FM.FatherID AS HusbandID, FM.MotherID AS WifeID,
                           CASE WHEN SUBSTR(Emar.Date, 8, 2) THEN SUBSTR(Emar.Date, 8, 2) ELSE '0' END AS MarriedMonth,
                           CASE WHEN SUBSTR(Emar.Date, 10, 2) THEN SUBSTR(Emar.Date, 10, 2) ELSE '0' END AS MarriedDay,
                           CASE WHEN SUBSTR(Emar.Date, 4, 4) THEN SUBSTR(Emar.Date, 4, 4) ELSE '0' END AS MarriedYear,
                           CASE WHEN SUBSTR(Eanl.Date, 4, 4) THEN SUBSTR(Eanl.Date, 4, 4) ELSE '0' END AS AnnulledDate,
                           CASE WHEN SUBSTR(Ediv.Date, 4, 4) THEN SUBSTR(Ediv.Date, 4, 4) ELSE '0' END AS DivorcedDate
                    FROM FamilyTable FM
                    LEFT JOIN EventTable Emar ON FM.FamilyID = Emar.OwnerID AND Emar.EventType = 300
                    LEFT JOIN EventTable Eanl ON FM.FamilyID = Eanl.OwnerID AND Eanl.EventType = 301
                    LEFT JOIN EventTable Ediv ON FM.FamilyID = Ediv.OwnerID AND Ediv.EventType = 302
                    WHERE FM.{whereIdTypeToUse} = @ownerId";

                var param = cmd.CreateParameter();
                param.ParameterName = "@ownerId";
                param.Value = ownerId;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Marriage(
                            familyId: reader.GetInt32(0),
                            husbandId: reader.GetInt32(1),
                            wifeId: reader.GetInt32(2),
                            marriageMonth: int.Parse(reader.GetString(3)),
                            marriageDay: int.Parse(reader.GetString(4)),
                            marriageYear: int.Parse(reader.GetString(5)),
                            annulledYear: int.Parse(reader.GetString(6)),
                            divorcedYear: int.Parse(reader.GetString(7))
                        ));
                    }
                }
            }
            return result;
        }

        public List<Parentage> GetParents(int childId)
        {
            var result = new List<Parentage>();
            using (var cmd = _dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT family.FamilyID, family.FatherID, family.MotherID,
                           children.ChildID, children.RelFather, children.RelMother
                    FROM FamilyTable family
                    JOIN NameTable father ON family.FatherID = father.OwnerID
                    JOIN NameTable mother ON family.MotherID = mother.OwnerID
                    JOIN ChildTable children ON family.FamilyID = children.FamilyID
                    JOIN NameTable child ON children.ChildID = child.OwnerID
                    WHERE children.ChildID = @childId";

                var param = cmd.CreateParameter();
                param.ParameterName = "@childId";
                param.Value = childId;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Parentage(
                            familyId: reader.GetInt32(0),
                            fatherId: reader.GetInt32(1),
                            motherId: reader.GetInt32(2),
                            childId: reader.GetInt32(3),
                            relationToFather: reader.GetInt32(4) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted,
                            relationToMother: reader.GetInt32(5) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted
                        ));
                    }
                }
            }
            return result;
        }

        public bool ValidateDatabaseIntegrity()
        {
            try
            {
                using (var cmd = _dbConnection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM NameTable";
                    cmd.ExecuteScalar();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Database validation failed: {ex.Message}");
                return false;
            }
        }

        ~RootsMagicFamilyHistoryDataProvider()
        {
            if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
            {
                _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }
    }
} 