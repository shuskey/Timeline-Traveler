namespace Assets.Scripts.DataObjects
{
    public class Marriage
    {
        public int familyId;
        public int husbandId;
        public int wifeId;
        public int marriageMonth;
        public int marriageDay;
        public int marriageYear;
        public int annulledYear;
        public int divorcedYear;

        public Marriage(int familyId, int husbandId, int wifeId, int marriageMonth, int marriageDay, int marriageYear, int annulledYear, int divorcedYear)
        {
            this.familyId = familyId;
            this.husbandId = husbandId;
            this.wifeId = wifeId;
            this.marriageMonth = marriageMonth;
            this.marriageDay = marriageDay;
            this.marriageYear = marriageYear;
            this.annulledYear = annulledYear;
            this.divorcedYear = divorcedYear;
        }
    }
}
