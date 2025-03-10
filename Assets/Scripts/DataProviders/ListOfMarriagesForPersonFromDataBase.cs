using Assets.Scripts.DataObjects;
using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace Assets.Scripts.DataProviders
{
    class ListOfMarriagesForPersonFromDataBase : DataProviderBase
    {
        public List<Marriage> marriageList;
        private string _dataBaseFileName;

        public ListOfMarriagesForPersonFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            marriageList = new List<Marriage>();
        }

        public void GetListOfMarriagesWithEventsForPersonFromDataBase(int ownerId, bool useHusbandQuery = true)
        {
            string whereIdTypeToUse = useHusbandQuery ? "FatherID" : "MotherID";

            string conn = "URI=file:" + _dataBaseFileName;

            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText =
                "SELECT FM.FamilyID, FM.FatherID AS HusbandID \n" +
                "    , FM.MotherID AS WifeID \n" +
                "    , CASE WHEN SUBSTR(Emar.Date, 8, 2) THEN SUBSTR(Emar.Date, 8, 2) ELSE \"0\" END AS MarriedMonth \n" +
                "    , CASE WHEN SUBSTR(Emar.Date, 10, 2) THEN SUBSTR(Emar.Date, 10, 2) ELSE \"0\" END AS MarriedDay \n" +
                "    , CASE WHEN SUBSTR(Emar.Date, 4, 4) THEN SUBSTR(Emar.Date, 4, 4) ELSE \"0\" END AS MarriedYear \n" +
                "    , CASE WHEN SUBSTR(Eanl.Date,4,4) THEN SUBSTR(Eanl.Date,4,4) ELSE \"0\" END AS AnnulledDate \n" +
                "    , CASE WHEN SUBSTR(Ediv.Date, 4, 4) THEN SUBSTR(Ediv.Date,4,4) ELSE \"0\" END AS DivorcedDate \n" +
                "FROM FamilyTable FM \n" +
                "LEFT JOIN EventTable Emar ON FM.FamilyID = Emar.OwnerID AND Emar.EventType = 300\n" +
                "LEFT JOIN EventTable Eanl ON FM.FamilyID = Eanl.OwnerID AND Eanl.EventType = 301-- to get Annullment event\n" +
                "LEFT JOIN EventTable Ediv ON FM.FamilyID = Ediv.OwnerID AND Ediv.EventType = 302-- to get Divorce event\n" +
                $"Where FM.{whereIdTypeToUse} = \"{ownerId}\";";

            IDataReader reader = dbcmd.ExecuteReader();   
            while (reader.Read())
            {
                var familyId = reader.GetInt32(0);
                var MarriageName = new Marriage(
                    familyId: familyId,
                    husbandId: reader.GetInt32(1),
                    wifeId: reader.GetInt32(2),
                    marriageMonth: StringToNumberProtected(reader.GetString(3), $"marriageMonth as GetString(3) for OwnerId/FamilyId: {ownerId}/{familyId}."),
                    marriageDay: StringToNumberProtected(reader.GetString(4), $"marriageDay as GetString(4) for OwnerId/FamilyId: {ownerId}/{familyId}."),
                    marriageYear: StringToNumberProtected(reader.GetString(5), $"marriageYear as GetString(5) for OwnerId/FamilyId: {ownerId}/{familyId}."),
                    annulledYear: StringToNumberProtected(reader.GetString(6), $"annulledYear as GetString(6) for OwnerId/FamilyId: {ownerId}/{familyId}."),
                    divorcedYear: StringToNumberProtected(reader.GetString(7), $"divorcedYear as GetString(7) for OwnerId/FamilyId: {ownerId}/{familyId}.")
                    );

                marriageList.Add(MarriageName);                
            }
            if (ownerId == 8)
                Debug.WriteLine("Got here");
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }        
}
