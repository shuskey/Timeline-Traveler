using UnityEngine;
using System.Collections.Generic;

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
        /// Gets thumbnails for a person in a specific year
        /// </summary>
        /// <param name="personId">The ID of the person</param>
        /// <param name="year">The year to get thumbnails for</param>
        /// <returns>List of thumbnail textures</returns>
        List<Texture2D> GetThumbnailForPerson(int personId, int year);

        /// <summary>
        /// Gets photos with associated metadata for a person in a specific year
        /// </summary>
        /// <param name="personId">The ID of the person</param>
        /// <param name="year">The year to get photos for</param>
        /// <returns>List of tuples containing the photo texture and its associated metadata</returns>
        List<(Texture2D Photo, Dictionary<string, string> Metadata)> GetPhotoListForPerson(int personId, int year);
    }
} 