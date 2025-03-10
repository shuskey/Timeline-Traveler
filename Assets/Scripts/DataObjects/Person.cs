using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DataObjects
{
    public class Person
    {
        public int dataBaseOwnerId;
        public int tribeArrayIndex;
        public string surName;
        public string givenName;
        public PersonGenderType gender;
        public int birthEventDate;   //TODO upgrade to EventDate in the future
		public int originalBirthEventDateMonth;
		public int originalBirthEventDateDay;
		public int originalBirthEventDateYear;
        public int deathEventDate;   //TODO upgrade to EventDate in the future
		public int originalDeathEventDateMonth;
		public int originalDeathEventDateDay;
		public int originalDeathEventDateYear;
		public string dateQualityInformationString;
		public bool isLiving;
		private bool originalIsLiving;
		public int generation;
		public int numberOfPersonsInThisGeneration;   
		public int indexIntoPersonsInThisGeneration;  // my 'zero based' postion/index into the number of persons in this generation
		public float xOffset;  // assists in recursive ordering of descendency trees
		public int spouseNumber;
        public GameObject personNodeGameObject;
        List<(PersonRelationshipType Relationship, Person RelatedPerson)> familyRelationships;

        public Person(int arrayIndex, int ownerId, PersonGenderType gender, string given, string surname,
				bool isLiving, int birthYear, int deathYear, int generation, float xOffset, int spouseNumber, int birthMonth = 0, int birthDay = 0, int deathMonth = 0, int deathDay = 0)
        {
            this.tribeArrayIndex = arrayIndex;
            this.dataBaseOwnerId = ownerId;
            this.gender = gender;
            givenName = given;
            surName = surname;
			originalBirthEventDateMonth = birthMonth;
			originalBirthEventDateDay = birthDay;
            originalBirthEventDateYear = birthEventDate = birthYear;
			originalDeathEventDateMonth = deathMonth;
			originalDeathEventDateDay = deathDay;
            originalDeathEventDateYear = deathEventDate = deathYear;
            originalIsLiving = this.isLiving = isLiving;
			dateQualityInformationString = "";
			this.generation = generation;
			this.xOffset = xOffset;
			this.spouseNumber = spouseNumber;
		}

		public int FixUpAndReturnMarriageDate(int marriageEventDate)
        {
			// zero or Bogus MarriageEventDate
			int dateToReturn = marriageEventDate;
			if (marriageEventDate == 0)
			{
				dateToReturn = birthEventDate + 20;
				dateQualityInformationString += $"MarriageDate is {marriageEventDate}. Was zero, so setting to {birthEventDate} + 20. New MarriageDate {dateToReturn}.";
			} else if (marriageEventDate < birthEventDate) {
				dateToReturn = birthEventDate + 20;
				dateQualityInformationString += $"MarriageDate is {marriageEventDate}. Was less than birthdate {birthEventDate}. New MarriageDate {dateToReturn}.";
			}
			return dateToReturn;
		}

		public void FixUpDatesForViewingWithMarriageDate(int marriageEventDate, Person otherSpouseForSomeDateClues)
        {
			//  OringinalBirthDate   OriginalDeathDate  IsLiving     Cleanup Action
			//  0	birth += delta	 0  death += delta   1 or 0        delta = marriageEventDate - (birth + 20)
			//  0   birth += delta   good
			// good                  0                            if death < marraige, then death = marriage + 5

			if (marriageEventDate == 0 && originalBirthEventDateYear == 0 && originalDeathEventDateYear == 0 && otherSpouseForSomeDateClues != null && otherSpouseForSomeDateClues.birthEventDate != 0)
            {
				birthEventDate = otherSpouseForSomeDateClues.birthEventDate;
				dateQualityInformationString = $"Fixing up birthDate and matching it to spouse date. ";
			}

			int fixedUpMarriageDate = FixUpAndReturnMarriageDate(marriageEventDate);

			var deltaFromMarriageAtTwenty = fixedUpMarriageDate - birthEventDate - 20;
			if (originalBirthEventDateYear == 0 && birthEventDate == 0)
			{
				dateQualityInformationString += $"Fixing up birthDate using known marriageDate by adding {deltaFromMarriageAtTwenty}. ";
				birthEventDate += deltaFromMarriageAtTwenty;
			}
			if (originalDeathEventDateYear == 0 && !isLiving)
			{
				dateQualityInformationString += $"Fixing up deathDate using known marriageDate by adding {deltaFromMarriageAtTwenty}. ";
				deathEventDate += deltaFromMarriageAtTwenty;
			}
			if (originalBirthEventDateYear != 0 && originalDeathEventDateYear == 0 && !isLiving && deathEventDate < fixedUpMarriageDate)
			{
				dateQualityInformationString += $"Fixing up deathDate because MarriageDate is after DeathDate.  New deathDate is {fixedUpMarriageDate + 5}. ";
				deathEventDate = fixedUpMarriageDate + 5;
			}
		}

		public void FixUpDatesForViewing()
        {
			var currentYear = DateTime.Now.Year;
			// DATA Clean up for a Visual Clean Display
			//  BirthDate   DeathDate  IsLiving calculatedAge     Cleanup Action
			//  0 (calc)	0			1        100             - Make them born 100 years ago, from today
			//  0 (calc)    0 (calc)    0		  90			 - Make them born 100 years ago, and died 10 years ago
			//  0 (calc)    given       0         90             - make them born 90 years before thier death
			//  0           given       1 (calc)                 - death date indicates not living, so flip Isliving
			if (birthEventDate == 0 || 
				((birthEventDate > deathEventDate) &&
				(deathEventDate != 0)))
			{
				if (deathEventDate == 0)
				{
					if (isLiving)
					{
						dateQualityInformationString += "BirthDate set to 100 years ago because, they are marked isLiving and birth and death dates are invalid.";
						birthEventDate = currentYear - 100;
					}
					else
					{
						dateQualityInformationString += "BirthDate set to 100 years ago, deathDate to 10 years ago because, they are marked NOT isLiving and birth and death dates are invalid.";
						birthEventDate = currentYear - 100;
						deathEventDate = currentYear - 10;
					}
				}
				else
				{
					birthEventDate = deathEventDate - 90;
					dateQualityInformationString += "BirthDate set to 90 years ago because, they are marked isLiving and birth and death dates are invalid.";
					if (isLiving)
						isLiving = false;
				}
			}
			// DATA Clean up for a Visual Clean Display
			//  BirthDate   DeathDate  IsLiving calculatedAge     Cleanup Action
			//  given       0           1 (calc)  min(100,today-given) - Set IsLiving to false is over 100
			//  given       0 (calc)    0         calc           - Calc death date to be (if older then 10): 10 years ago
			//                                                      if 1 to 10 years old: 1 year after birth
			//                                                      if 0 years old then deathDate = birthDate
			//														if more then 100 years old then cap deathdate at birthDate + 90
			else if (deathEventDate == 0)
			{
				if (isLiving && (currentYear - birthEventDate > 100))
				{
					dateQualityInformationString += $"isLiving is being set to false because it is currently true and this person is {currentYear - birthEventDate} years old. ";
					isLiving = false;
				}
				if (!isLiving)
				{
					if ((currentYear - birthEventDate) > 100)
					{
						deathEventDate = birthEventDate + 90;
						dateQualityInformationString += $"DeathDate set to {deathEventDate} (Death at age 90) because, deathDate is 0, they are marked Not isLiving, and birth was more than 100 years ago.";
					}
					else if ((currentYear - birthEventDate) > 10)
					{
						deathEventDate = currentYear - 10;
						dateQualityInformationString += $"DeathDate set to {deathEventDate} (10 years ago) because, deathDate is 0, they are marked Not isLiving, and birth was more then 10 and less then 100 years ago..";
					}
					else if ((currentYear - birthEventDate) > 0)
					{
						deathEventDate = birthEventDate + 1;
						dateQualityInformationString += $"DeathDate set to {deathEventDate} (Death at age 1) because, deathDate is 0, they are marked Not isLiving, and birth was within the last 9 years. ";
					}
					else
					{
						deathEventDate = birthEventDate;
						dateQualityInformationString += $"DeathDate set to now because, deathDate is 0, they are marked as Not isLiving, and birth was this year. ";
					}
				}
			}
		}
    }
}
