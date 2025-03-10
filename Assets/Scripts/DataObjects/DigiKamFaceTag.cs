using Assets.Scripts.Enums;

namespace Assets.Scripts.DataObjects
{
    public class DigiKamFaceTag
    {
        public int personId;
        public int tagId;
        public string tagName;
        
        public DigiKamFaceTag(int personId, int tagId, string tagName)
        {
            this.personId = personId;
            this.tagId = tagId;
            this.tagName = tagName;
        }
    }
}
