using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.DataObjects
{
    [System.Serializable]
    public class DigiKamDatabaseAssociation
    {
        public string database_name;
        public string digikam_root_folder;
    }

    [System.Serializable]
    public class DigiKamAssociations
    {
        public DigiKamDatabaseAssociation[] digikam_associations;
        
        public DigiKamAssociations()
        {
            digikam_associations = new DigiKamDatabaseAssociation[0];
        }
        
        // Helper methods to work like a dictionary
        public void AddAssociation(string databaseName, string rootFolder)
        {
            var newAssociation = new DigiKamDatabaseAssociation
            {
                database_name = databaseName,
                digikam_root_folder = rootFolder
            };
            
            // Remove existing if found
            var list = new List<DigiKamDatabaseAssociation>(digikam_associations);
            list.RemoveAll(x => x.database_name == databaseName);
            list.Add(newAssociation);
            digikam_associations = list.ToArray();
        }
        
        public string GetRootFolder(string databaseName)
        {
            foreach (var assoc in digikam_associations)
            {
                if (assoc.database_name == databaseName)
                    return assoc.digikam_root_folder;
            }
            return null;
        }
    }

    /// <summary>
    /// Utility class for managing DigiKam configuration files that associate RootsMagic databases with DigiKam root folders
    /// </summary>
    public static class DigiKamConfigManager
    {
        private const string CONFIG_FILE_NAME = "digikam.config";

        /// <summary>
        /// Gets the path to the DigiKam config file based on the RootsMagic database path
        /// </summary>
        /// <param name="rootsMagicPath">Full path to the RootsMagic .rmtree file</param>
        /// <returns>Full path to the digikam.config file</returns>
        public static string GetConfigPath(string rootsMagicPath)
        {
            if (string.IsNullOrEmpty(rootsMagicPath))
                return null;
                
            string directory = Path.GetDirectoryName(rootsMagicPath);
            return Path.Combine(directory, CONFIG_FILE_NAME);
        }

        /// <summary>
        /// Creates or updates a DigiKam config file with the specified database and root folder
        /// </summary>
        /// <param name="rootsMagicPath">Full path to the RootsMagic .rmtree file</param>
        /// <param name="digiKamRootFolder">Root folder path for DigiKam photos</param>
        public static void CreateOrUpdateDigiKamConfig(string rootsMagicPath, string digiKamRootFolder)
        {
            if (string.IsNullOrEmpty(rootsMagicPath) || string.IsNullOrEmpty(digiKamRootFolder))
                return;

            string databaseName = Path.GetFileNameWithoutExtension(rootsMagicPath);
            string configPath = GetConfigPath(rootsMagicPath);
            
            DigiKamAssociations config = LoadConfig(configPath);
            config.AddAssociation(databaseName, digiKamRootFolder);
            SaveConfig(configPath, config);
        }

        /// <summary>
        /// Creates or updates a DigiKam config file with database path and root folder inferred from the database
        /// </summary>
        /// <param name="rootsMagicPath">Full path to the RootsMagic .rmtree file</param>
        /// <param name="digiKamDatabasePath">Full path to the DigiKam database file</param>
        public static void CreateOrUpdateDigiKamConfigFromDatabasePath(string rootsMagicPath, string digiKamDatabasePath)
        {
            if (string.IsNullOrEmpty(rootsMagicPath) || string.IsNullOrEmpty(digiKamDatabasePath))
                return;

            // The DigiKam root folder is typically the parent directory of the database
            string digiKamRootFolder = Path.GetDirectoryName(digiKamDatabasePath);
            CreateOrUpdateDigiKamConfig(rootsMagicPath, digiKamRootFolder);
        }

        /// <summary>
        /// Gets the DigiKam root folder associated with a RootsMagic database
        /// </summary>
        /// <param name="rootsMagicPath">Full path to the RootsMagic .rmtree file</param>
        /// <returns>DigiKam root folder path, or null if not found</returns>
        public static string GetDigiKamRootFolderFromConfig(string rootsMagicPath)
        {
            if (string.IsNullOrEmpty(rootsMagicPath))
                return null;

            string databaseName = Path.GetFileNameWithoutExtension(rootsMagicPath);
            string configPath = GetConfigPath(rootsMagicPath);
            
            if (!File.Exists(configPath))
                return null;

            DigiKamAssociations config = LoadConfig(configPath);
            return config.GetRootFolder(databaseName);
        }

        /// <summary>
        /// Gets the DigiKam database path using config and PlayerPrefs fallback
        /// </summary>
        /// <param name="rootsMagicPath">Full path to the RootsMagic .rmtree file</param>
        /// <param name="playerPrefsKey">PlayerPrefs key for fallback database path</param>
        /// <returns>DigiKam database path, or null if not found</returns>
        public static string GetDigiKamDatabasePath(string rootsMagicPath, string playerPrefsKey)
        {
            // First try to get from config
            string rootFolder = GetDigiKamRootFolderFromConfig(rootsMagicPath);
            if (!string.IsNullOrEmpty(rootFolder))
            {
                string databasePath = Path.Combine(rootFolder, "digikam4.db");
                if (File.Exists(databasePath))
                    return databasePath;
            }

            // Fallback to PlayerPrefs
            if (UnityEngine.PlayerPrefs.HasKey(playerPrefsKey))
            {
                string fallbackPath = UnityEngine.PlayerPrefs.GetString(playerPrefsKey);
                if (!string.IsNullOrEmpty(fallbackPath) && File.Exists(fallbackPath))
                    return fallbackPath;
            }

            return null;
        }

        /// <summary>
        /// Loads the DigiKam associations from config file
        /// </summary>
        /// <param name="configPath">Full path to config file</param>
        /// <returns>DigiKamAssociations object</returns>
        private static DigiKamAssociations LoadConfig(string configPath)
        {
            if (!File.Exists(configPath))
                return new DigiKamAssociations();

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonUtility.FromJson<DigiKamAssociations>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load DigiKam config from {configPath}: {ex.Message}");
                return new DigiKamAssociations();
            }
        }

        /// <summary>
        /// Saves the DigiKam associations to config file
        /// </summary>
        /// <param name="configPath">Full path to config file</param>
        /// <param name="config">DigiKamAssociations to save</param>
        private static void SaveConfig(string configPath, DigiKamAssociations config)
        {
            try
            {
                string json = JsonUtility.ToJson(config, true);
                string directory = Path.GetDirectoryName(configPath);
                
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                    
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save DigiKam config to {configPath}: {ex.Message}");
            }
        }
    }
} 