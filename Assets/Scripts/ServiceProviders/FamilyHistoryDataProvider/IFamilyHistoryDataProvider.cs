using System.Collections.Generic;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    public interface IFamilyHistoryDataProvider
    {
        void Initialize(Dictionary<string, string> configuration);
        List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);
        List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);
        List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);
        bool ValidateDatabaseIntegrity();
    }
} 