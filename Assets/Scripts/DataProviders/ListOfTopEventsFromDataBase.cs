using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    public class ListOfTopEventsFromDataBase
    {
        public List<TopEvent> topEventsList;
        private string _dataBaseFileName;

        public ListOfTopEventsFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            topEventsList = new List<TopEvent>();
        }

        public void GetListOfTopEventsFromDataBase(int? specificYear = null)
        {
            string conn = "URI=file:" + _dataBaseFileName;


            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            var yearFilterPart = specificYear == null ? "\n" : $"AND year == {specificYear}\n";
            dbcmd.CommandText =
                "SELECT id, year, linkCount, item, itemLabel, picture, wikiLink, description, aliases, locations, countries, pointInTime, eventStartDate, eventEndDate \n" +
                "FROM topEvents \n" +
                $"WHERE description != \"\" AND itemLabel != \"\" AND wikiLink != \"\" AND linkCount > 5 {yearFilterPart}" +
                "ORDER BY pointInTime ASC, linkCount DESC;";
   
            IDataReader reader = dbcmd.ExecuteReader();            
            while (reader.Read())
            {
                var topEvent = new TopEvent(
                    id: reader.GetInt32(0),
                    year: reader.GetString(1),
                    linkCount: reader.GetInt32(2),
                    item: reader.GetString(3),
                    itemLabel: reader.GetString(4),
                    picture: reader.GetString(5),
                    wikiLink: reader.GetString(6),
                    description: reader.GetString(7),
                    aliases: reader.GetString(8),
                    locations: reader.GetString(9),
                    countries: reader.GetString(10),
                    pointInTime: reader.GetString(11),
                    eventStartDate: reader.GetString(12),
                    eventEndDate: reader.GetString(13));

                topEventsList.Add(topEvent);                
            }

            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }        
}
