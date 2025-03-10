using Assets.Scripts.Enums;
using System;

namespace Assets.Scripts.DataObjects
{
    class EventDate
    {
		public EventDateType EventType;
		//	nullable Specific Date
		public DateTime? SpecificDate;
		// Qualifier "About", means the same as: Estimate, Calculated, Circa, Say, certainly, probably, possibly, likely, perhaps, maybe)
		public string Qualifier;
		//	Rough date, Year, Month
		public (int year, int month) RoughDate;
		// Non understood Date String or string clue, no year indicated "23" "Feb 23", or non handled "300bc"
		public string OriginalDateString;
		// Quality of Date
		public EventQualityType QualityType;
		// Unknown(absolutly no information)
		// Range(future story)
		// Inferred(future story)

		public EventDate(EventDateType eventType, string originalDateString)
        {
			EventType = eventType;
			OriginalDateString = originalDateString;
			DateTime ResultingDate = new DateTime();

			if (!DateTime.TryParse(originalDateString, out ResultingDate))
			{
				//That Parse did not work, so lets check for other variants
				// Was there a qualifier word? (remove it and try again)
				QualityType = EventQualityType.EstimateQualifierUsed;
				// was it just a year (rough date)
				// was it a year and a month (rough date)
				QualityType = EventQualityType.RoughDate;
				// was it a range ? - separate out 'start' and 'end' the run seperatly (future story)
				QualityType = EventQualityType.Unknown;

			}
			else
			{
				SpecificDate = ResultingDate;
				QualityType = EventQualityType.PresumedCorrect;
			}
		}
	}
}
