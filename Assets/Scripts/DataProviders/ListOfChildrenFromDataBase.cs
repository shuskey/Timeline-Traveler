using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class ListOfChildrenFromDataBase
    {
        public List<Parentage> childList;
        private string _dataBaseFileName;

        public ListOfChildrenFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            childList = new List<Parentage>();
        }

        public void GetListOfChildrenFromDataBase(int familyId)
        {
            string conn = "URI=file:" + _dataBaseFileName;

            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText =
                "SELECT family.FamilyID, family.FatherID, family.MotherID, children.ChildID, children.RelFather, children.RelMother \n" +
                "FROM FamilyTable family \n" +
                "JOIN NameTable father \n" +
                "   ON family.FatherID = father.OwnerID \n" +
                "JOIN NameTable mother \n" +
                "   ON family.MotherID = mother.OwnerID \n" +
                "JOIN ChildTable children \n" +
                "   ON family.FamilyID = children.FamilyID \n" +
                "   JOIN NameTable child \n" +
                "      ON children.ChildID = child.OwnerID \n" +
                $"WHERE family.familyID = \"{familyId}\" \n" +
                "ORDER BY children.ChildOrder ASC; ";

            IDataReader reader = dbcmd.ExecuteReader();            
            while (reader.Read())
            {
                var parantage = new Parentage(
                    familyId: reader.GetInt32(0),
                    fatherId: reader.GetInt32(1),
                    motherId: reader.GetInt32(2),
                    childId: reader.GetInt32(3),
                    relationToFather: reader.GetInt32(4) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted,
                    relationToMother: reader.GetInt32(5) == 0 ? ChildRelationshipType.Biological : ChildRelationshipType.Adopted);

                childList.Add(parantage);                
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
