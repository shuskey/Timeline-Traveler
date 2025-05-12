using System.Collections.Generic;
using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;

namespace Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider
{
    /// <summary>
    /// Interface for providers that handle family history data operations
    /// </summary>
    public interface IFamilyHistoryDataProvider
    {
        /// <summary>
        /// Initializes the provider with configuration data
        /// </summary>
        /// <param name="configuration">Dictionary containing configuration data like database paths</param>
        void Initialize(Dictionary<string, string> configuration);

        /// <summary>
        /// Gets a single person by their ID
        /// </summary>
        /// <param name="ownerId">The ID of the person to retrieve</param>
        /// <param name="generation">The generation number for display purposes</param>
        /// <param name="xOffset">X offset for display purposes</param>
        /// <param name="spouseNumber">Spouse number for display purposes</param>
        /// <returns>List containing the person if found, empty list otherwise</returns>
        List<Person> GetPerson(int ownerId, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);

        /// <summary>
        /// Gets a list of persons with optional filtering
        /// </summary>
        /// <param name="limitListSizeTo">Maximum number of persons to return</param>
        /// <param name="justThisOwnerId">Optional ID to filter by</param>
        /// <param name="generation">The generation number for display purposes</param>
        /// <param name="xOffset">X offset for display purposes</param>
        /// <param name="spouseNumber">Spouse number for display purposes</param>
        /// <returns>List of persons matching the criteria</returns>
        List<Person> GetPersonList(int limitListSizeTo, int? justThisOwnerId = null, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);

        /// <summary>
        /// Gets a list of persons filtered by last name
        /// </summary>
        /// <param name="lastNameFilter">The last name to filter by</param>
        /// <param name="limitListSizeTo">Maximum number of persons to return</param>
        /// <param name="generation">The generation number for display purposes</param>
        /// <param name="xOffset">X offset for display purposes</param>
        /// <param name="spouseNumber">Spouse number for display purposes</param>
        /// <param name="alphaSorted">Sort by</param>
        /// <returns>List of persons matching the last name</returns>
        List<Person> GetPersonListByLastName(string lastNameFilter, int limitListSizeTo, int generation = 0, float xOffset = 0.0f, int spouseNumber = 0);

        /// <summary>
        /// Gets a list of children for a family
        /// </summary>
        /// <param name="familyId">The ID of the family</param>
        /// <returns>List of parentage records for the children</returns>
        List<Parentage> GetChildren(int familyId);

        /// <summary>
        /// Gets a list of marriages for a person
        /// </summary>
        /// <param name="ownerId">The ID of the person</param>
        /// <param name="useHusbandQuery">True to search as husband, false to search as wife</param>
        /// <returns>List of marriage records</returns>
        List<Marriage> GetMarriages(int ownerId, bool useHusbandQuery = true);

        /// <summary>
        /// Gets a list of parents for a person
        /// </summary>
        /// <param name="childId">The ID of the person to get parents for</param>
        /// <returns>List of parentage records for the parents</returns>
        List<Parentage> GetParents(int childId);

        /// <summary>
        /// Validates the integrity of the database
        /// </summary>
        /// <returns>True if the database is valid, false otherwise</returns>
        bool ValidateDatabaseIntegrity();
    }
} 