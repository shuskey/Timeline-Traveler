using Assets.Scripts.Enums;

namespace Assets.Scripts.DataObjects
{
    public class FamilyPhoto
    {
        public string year;
        public string itemLabel;
        public string picturePathInArchive;
        public string description;
        public string locations;
        public string countries;
        public string pointInTime;
        public string eventStartDate;
        public string eventEndDate;       

        public FamilyPhoto(string year,
            string itemLabel, string picturePathInArchive, string description,
            string locations, string countries, string pointInTime,
            string eventStartDate, string eventEndDate)
        {
            this.year = year; 
            this.itemLabel = itemLabel;
            this.picturePathInArchive = picturePathInArchive;
            this.description = description;
            this.locations = locations;
            this.countries = countries;
            this.pointInTime = pointInTime;
            this.eventStartDate = eventStartDate;
            this.eventEndDate = eventEndDate;
        }
    }
}
