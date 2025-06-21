using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Utilities;
namespace Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider
{
    public interface IFamilyHistoryPictureProvider
    {
        /// <summary>
        /// Initializes the provider with configuration data
        /// </summary>
        /// <param name="configuration">Dictionary containing configuration data like database paths</param>
        void Initialize(Dictionary<string, string> configuration);

        /// <summary>
        /// Checks if all required database files are present
        /// </summary>
        /// <returns>True if all required database files exist, false otherwise</returns>
        bool AreAllDatabaseFilesPresent();

        /// <summary>
        /// Gets photos with associated metadata for a person in a specific year
        /// </summary>
        /// <param name="personId">The ID of the person</param>
        /// <param name="year">The year to get photos for (nullable to allow all years)</param>
        /// <returns>List of tuples containing the photo texture and its associated metadata</returns>
        List<PhotoInfo> GetPhotoInfoListForPerson(int personId, int? year);

        PhotoInfo GetThumbnailPhotoInfoForPerson(int personId, int year);

        /// <summary>
        /// Updates photo information back to the database
        /// </summary>
        /// <param name="modifiedPhotoInfo">The modified photo information to update</param>
        void UpdatePhotoInfo(PhotoInfo modifiedPhotoInfo);
    }
} 