using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class ListOfPersonsFromDataBase : DataProviderBase
    {
        public List<Person> personsList;
        private string _rootsMagicDataBaseFileName;

        public ListOfPersonsFromDataBase(string RootMagicDataBaseFileName)           
        {
            _rootsMagicDataBaseFileName = RootMagicDataBaseFileName;
            personsList = new List<Person>();
        }

        public void GetSinglePersonFromDataBase(int ownerId, int generation, float xOffset, int spouseNumber)
        {
            // only if this person is not in the Tribe yet
            if (!personsList.Exists(x => x.dataBaseOwnerId == ownerId))
                GetListOfPersonsFromDataBase(limitListSizeTo: 1, ownerId, generation, xOffset, spouseNumber);
        }

        public void GetListOfPersonsFromDataBase(int limitListSizeTo, int? JustThisOwnerId = null, int generation = 0,
            float xOffset = 0.0f, int spouseNumber = 0)
        {
            string conn = "URI=file:" + _rootsMagicDataBaseFileName;
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYNAMES =
                "SELECT  name.OwnerID \n" +
                "     , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end \n" +
                "     , name.Given, name.Surname \n" +
                "     , CASE WHEN SUBSTR(eventBirth.Date,8,2) THEN SUBSTR(eventBirth.Date,8,2) ELSE \"0\" END AS BirthMonth \n"+
                "     , CASE WHEN SUBSTR(eventBirth.Date, 10, 2) THEN SUBSTR(eventBirth.Date,10,2) ELSE \"0\" END AS BirthdDay \n" +
                "     , CASE WHEN SUBSTR(eventBirth.Date,4,4) THEN \n" +
                "           CASE WHEN SUBSTR(eventBirth.Date, 4, 4) != \"0\" THEN SUBSTR(eventBirth.Date,4,4) END \n" +
                "           ELSE CAST(name.BirthYear as varchar(10)) END AS BirthYear \n" +
                "     , person.Living \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,8,2) THEN SUBSTR(eventDeath.Date,8,2) ELSE \"0\" END AS DeathMonth \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,10,2) THEN SUBSTR(eventDeath.Date,10,2) ELSE \"0\" END AS DeathdDay \n" +
                "     , CASE WHEN SUBSTR(eventDeath.Date,4,4) THEN SUBSTR(eventDeath.Date,4,4) ELSE \"0\" END AS DeathYear \n" +
                "FROM NameTable name \n" +
                "JOIN PersonTable person \n" +
                "    ON name.OwnerID = person.PersonID \n" +
                "LEFT JOIN EventTable eventBirth ON name.OwnerID = eventBirth.OwnerID AND eventBirth.EventType = 1 \n" +
                "LEFT JOIN EventTable eventDeath \n" +
                "    ON name.OwnerID = eventDeath.OwnerID AND eventDeath.EventType = 2 \n";
            if (JustThisOwnerId != null)
            {
                QUERYNAMES +=
                    $"WHERE name.OwnerID = \"{JustThisOwnerId}\" LIMIT 1;";
            }

            string sqlQuery = QUERYNAMES;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo)
            {
                var ownerId = reader.GetInt32(0);
                var nextName = new Person(
                    arrayIndex: currentArrayIndex,
                    ownerId: ownerId,
                    gender: charToPersonGenderType(reader.GetString(1)[0]),
                    given: reader.GetString(2),
                    surname: reader.GetString(3),
                    birthMonth: StringToNumberProtected(reader.GetString(4), $"birthMonth as GetString(4) for OwnerId: {ownerId}."),
                    birthDay: StringToNumberProtected(reader.GetString(5), $"birthDay as GetString(5) for OwnerId: {ownerId}."),
                    birthYear: StringToNumberProtected(reader.GetString(6), $"birthYear as GetString(6) for OwnerId: {ownerId}."),
                    isLiving: reader.GetBoolean(7),
                    deathMonth: StringToNumberProtected(reader.GetString(8), $"deathMonth as GetString(8) for OwnerId: {ownerId}."),
                    deathDay: StringToNumberProtected(reader.GetString(9), $"deathDay as GetString(9) for OwnerId: {ownerId}."),
                    deathYear: StringToNumberProtected(reader.GetString(10), $"deathYear as GetString(10) for OwnerId: {ownerId}."),
                    generation: generation,
                    xOffset: xOffset,
                    spouseNumber: spouseNumber);

                nextName.FixUpDatesForViewing();

                personsList.Add(nextName);
                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;

            PersonGenderType charToPersonGenderType(char sex) =>
                sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);
        }

        public void GetListOfPersonsFromDataBaseWithLastNameFilter(int limitListSizeTo, int? JustThisOwnerId = null, int generation = 0,
    float xOffset = 0.0f, int spouseNumber = 0, string lastNameFilterString = null)
        {
            string conn = "URI=file:" + _rootsMagicDataBaseFileName;
            List<Person> unsortedPersonList = new List<Person>();

            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYNAMES =
                "SELECT  name.OwnerID \n" +
                "     , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end \n" +
                "     , name.Given, name.Surname \n" +                
                "     , CAST(name.BirthYear as varchar(10)) AS BirthYear \n" +                
                "FROM NameTable name \n" +
                "JOIN PersonTable person \n" +
                "    ON name.OwnerID = person.PersonID \n";                
                QUERYNAMES +=
                    $"WHERE name.Surname LIKE \"%{lastNameFilterString}%\";";

            string sqlQuery = QUERYNAMES;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo)
            {
                var ownerId = reader.GetInt32(0);
                var nextName = new Person(
                    arrayIndex: currentArrayIndex,
                    ownerId: ownerId,
                    gender: charToPersonGenderType(reader.GetString(1)[0]),
                    given: reader.GetString(2),
                    surname: reader.GetString(3),
                    birthYear: StringToNumberProtected(reader.GetString(4), $"birthYear as GetString(4) for OwnerId: {ownerId}."),   
                    deathYear: 0,
                    isLiving: false,
                    generation: generation,
                    xOffset: xOffset,
                    spouseNumber: spouseNumber);

               // nextName.FixUpDatesForViewing();

                unsortedPersonList.Add(nextName);
                currentArrayIndex++;
            }
            personsList = unsortedPersonList.OrderBy(x=> x.surName + " " + x.givenName).ToList();

            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;

            PersonGenderType charToPersonGenderType(char sex) =>
                sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);
        }

        public bool QuickDataBaseIntergetyCheck()
        {
            bool returnThisStatus = true;

            string conn = "URI=file:" + _rootsMagicDataBaseFileName;

            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            //IDbCommand dbcmd = dbconn.CreateCommand();
            //string QUERYNAMES =
            //    "PRAGMA quick_check;";

            //string sqlQuery = QUERYNAMES;
            //dbcmd.CommandText = sqlQuery;
            //IDataReader reader = dbcmd.ExecuteReader();

            //reader.Read();
            //returnThisStatus = (reader.GetString(0).ToLower() == "ok");

            //reader.Close();
            //reader = null;
            //dbcmd.Dispose();
            //dbcmd = null;
            dbconn.Close();
            dbconn = null;

            return returnThisStatus;
        }


    }
}
