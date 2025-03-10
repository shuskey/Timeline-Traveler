using Assets.Scripts.Enums;

namespace Assets.Scripts.DataObjects
{
    public class TopEvent
    {
        public int id;
        public string year;
        public int linkCount;
        public string item;
        public string itemLabel;
        public string picture;
        public string wikiLink;
        public string description;
        public string aliases;
        public string locations;
        public string countries;
        public string pointInTime;
        public string eventStartDate;
        public string eventEndDate;       

        public TopEvent(int id, string year, int linkCount, string item,
            string itemLabel, string picture, string wikiLink, string description,
            string aliases, string locations, string countries, string pointInTime,
            string eventStartDate, string eventEndDate)
        {
            this.id = id;
            this.year = year; 
            this.linkCount = linkCount;
            this.item = item;
            this.itemLabel = itemLabel;
            this.picture = picture;
            this.wikiLink = wikiLink;
            this.description = description;
            this.aliases = aliases;
            this.locations = locations;
            this.countries = countries;
            this.pointInTime = pointInTime;
            this.eventStartDate = eventStartDate;
            this.eventEndDate = eventEndDate;
        }
    }
}
