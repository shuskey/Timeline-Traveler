using Assets.Scripts.Enums;

namespace Assets.Scripts.DataObjects
{
    public class Parentage
    {
        public int familyId;
        public int fatherId;
        public int motherId;
        public int childId;
        public ChildRelationshipType relationToFather;
        public ChildRelationshipType relationToMother;
  
        public Parentage(int familyId, int fatherId, int motherId, int childId, ChildRelationshipType relationToFather, ChildRelationshipType relationToMother)
        {
            this.familyId = familyId;
            this.fatherId = fatherId;
            this.motherId = motherId;
            this.childId = childId;
            this.relationToFather = relationToFather;
            this.relationToMother = relationToMother;
        }
    }
}
