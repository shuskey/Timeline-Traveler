using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
    private const string DIGIKAM_DATABASE_NAME = "digikam4.db";

    /// <summary>
    /// Creates or updates the digikam.config file in the RootsMagic database directory
    /// </summary>
    /// <param name="rootsMagicDatabasePath">Full path to the RootsMagic database file</param>
    /// <param name="digiKamRootFolder">The DigiKam root folder path to associate with the RootsMagic database</param>
    public static void CreateOrUpdateDigiKamConfig(string rootsMagicDatabasePath, string digiKamRootFolder)
    {
        if (string.IsNullOrEmpty(rootsMagicDatabasePath))
        {
            Debug.LogError("RootsMagic database path is not set");
            return;
        }

        if (string.IsNullOrEmpty(digiKamRootFolder))
        {
            Debug.LogError("DigiKam root folder path is not set");
            return;
        }

        try
        {
            // Get the RootsMagic database directory and filename
            string rootsMagicDirectory = Path.GetDirectoryName(rootsMagicDatabasePath);
            string rootsMagicFileName = Path.GetFileName(rootsMagicDatabasePath);
            string configFilePath = Path.Combine(rootsMagicDirectory, CONFIG_FILE_NAME);

            DigiKamAssociations config;

            // Check if config file exists
            if (File.Exists(configFilePath))
            {
                // Read and parse existing config
                string existingJson = File.ReadAllText(configFilePath);
                try
                {
                    config = JsonUtility.FromJson<DigiKamAssociations>(existingJson);
                    if (config?.digikam_associations == null)
                    {
                        config = new DigiKamAssociations();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to parse existing config file, creating new one. Error: {ex.Message}");
                    config = new DigiKamAssociations();
                }
            }
            else
            {
                // Create new config
                config = new DigiKamAssociations();
            }

            // Update or add the association for the current RootsMagic database
            config.AddAssociation(rootsMagicFileName, digiKamRootFolder);

            // Write the updated config back to file
            string jsonOutput = JsonUtility.ToJson(config, true);
            File.WriteAllText(configFilePath, jsonOutput);

            Debug.Log($"DigiKam config updated successfully at: {configFilePath}");
            Debug.Log($"Associated {rootsMagicFileName} with DigiKam root folder: {digiKamRootFolder}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create or update DigiKam config: {ex.Message}");
        }
    }

    /// <summary>
    /// Convenience method to create or update the config using the DigiKam database directory as root folder
    /// </summary>
    /// <param name="rootsMagicDatabasePath">Full path to the RootsMagic database file</param>
    /// <param name="digiKamDatabasePath">Full path to the DigiKam database file</param>
    public static void CreateOrUpdateDigiKamConfigFromDatabasePath(string rootsMagicDatabasePath, string digiKamDatabasePath)
    {
        if (string.IsNullOrEmpty(digiKamDatabasePath))
        {
            Debug.LogWarning("DigiKam database path is not set, cannot auto-determine root folder");
            return;
        }

        // Use the DigiKam database directory as the root folder
        string digiKamDirectory = Path.GetDirectoryName(digiKamDatabasePath);
        CreateOrUpdateDigiKamConfig(rootsMagicDatabasePath, digiKamDirectory);
    }

    /// <summary>
    /// Reads the DigiKam root folder from the config file for the specified RootsMagic database
    /// </summary>
    /// <param name="rootsMagicDatabasePath">Full path to the RootsMagic database file</param>
    /// <returns>The DigiKam root folder path if found, null otherwise</returns>
    public static string GetDigiKamRootFolderFromConfig(string rootsMagicDatabasePath)
    {
        if (string.IsNullOrEmpty(rootsMagicDatabasePath))
        {
            Debug.LogWarning("RootsMagic database path is not set");
            return null;
        }

        try
        {
            // Get the RootsMagic database directory and filename
            string rootsMagicDirectory = Path.GetDirectoryName(rootsMagicDatabasePath);
            string rootsMagicFileName = Path.GetFileName(rootsMagicDatabasePath);
            string configFilePath = Path.Combine(rootsMagicDirectory, CONFIG_FILE_NAME);

            // Check if config file exists
            if (!File.Exists(configFilePath))
            {
                Debug.Log($"DigiKam config file not found at: {configFilePath}");
                return null;
            }

            // Read and parse the config file
            string configJson = File.ReadAllText(configFilePath);
            DigiKamAssociations config = JsonUtility.FromJson<DigiKamAssociations>(configJson);

            if (config?.digikam_associations != null)
            {
                string digiKamRootFolder = config.GetRootFolder(rootsMagicFileName);
                if (!string.IsNullOrEmpty(digiKamRootFolder))
                {
                    Debug.Log($"Found DigiKam root folder in config: {digiKamRootFolder} for {rootsMagicFileName}");
                    return digiKamRootFolder;
                }
            }
            
            Debug.Log($"No DigiKam association found for {rootsMagicFileName} in config file");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to read DigiKam config: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Attempts to get the DigiKam database path by checking config file first, then falling back to PlayerPrefs
    /// </summary>
    /// <param name="rootsMagicDatabasePath">Full path to the RootsMagic database file</param>
    /// <param name="playerPrefsKey">PlayerPrefs key to use as fallback</param>
    /// <returns>The DigiKam database path if found, null otherwise</returns>
    public static string GetDigiKamDatabasePath(string rootsMagicDatabasePath, string playerPrefsKey)
    {
        // First try to get DigiKam root folder from config file
        string digiKamRootFolder = GetDigiKamRootFolderFromConfig(rootsMagicDatabasePath);
        
        if (!string.IsNullOrEmpty(digiKamRootFolder))
        {
            // Construct the expected DigiKam database path
            string expectedDigiKamPath = Path.Combine(digiKamRootFolder, DIGIKAM_DATABASE_NAME);
            
            if (File.Exists(expectedDigiKamPath))
            {
                Debug.Log($"DigiKam database found from config: {expectedDigiKamPath}");
                return expectedDigiKamPath;
            }
            else
            {
                Debug.LogWarning($"DigiKam database not found at expected path from config: {expectedDigiKamPath}");
            }
        }

        // Fall back to PlayerPrefs if config didn't work
        if (PlayerPrefs.HasKey(playerPrefsKey))
        {
            string playerPrefsPath = PlayerPrefs.GetString(playerPrefsKey);
            if (File.Exists(playerPrefsPath))
            {
                Debug.Log($"DigiKam database found from PlayerPrefs: {playerPrefsPath}");
                return playerPrefsPath;
            }
            else
            {
                Debug.LogWarning($"DigiKam database not found at PlayerPrefs path: {playerPrefsPath}");
            }
        }

        Debug.Log("No DigiKam database path found in config or PlayerPrefs");
        return null;
    }
} 