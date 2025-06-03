using Assets.Scripts.DataObjects;

namespace Assets.Scripts.DataObjects
{
    /// <summary>
    /// Represents a tag associated with a photo, containing hierarchical tag information.
    /// </summary>
    public class PhotoTag
    {
        public string TagName { get; set; }
        public int TagId { get; set; }
        public int ParentTagId { get; set; }

        public PhotoTag(string tagName, int tagId, int parentTagId)
        {
            TagName = tagName;
            TagId = tagId;
            ParentTagId = parentTagId;
        }

     public override string ToString()
        {
            return $"Tag: {TagName} (ID: {TagId}, Parent: {ParentTagId})";
        }
    }
} 