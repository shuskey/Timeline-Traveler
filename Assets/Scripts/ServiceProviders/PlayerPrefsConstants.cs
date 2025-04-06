namespace Assets.Scripts.ServiceProviders
{
    /// <summary>
    /// Contains shared constants for PlayerPrefs keys used across different handlers
    /// </summary>
    public static class PlayerPrefsConstants
    {
        /// <summary>
        /// Key for the last used RootsMagic database file path
        /// </summary>
        public const string LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH = "LastUsedRootsMagicDataFilePath";

        /// <summary>
        /// Key for the last selected RootsMagic base person ID
        /// </summary>
        public const string LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_ID = "LastSelectedRootsMagicBasePersonId";

        /// <summary>
        /// Key for the last selected RootsMagic base person full name
        /// </summary>
        public const string LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_FULL_NAME = "LastSelectedRootsMagicBasePersonFullName";

        /// <summary>
        /// Key for the last used DigiKam database file path
        /// </summary>
        public const string LAST_USED_DIGIKAM_DATA_FILE_PATH = "LastUsedDigiKamDataFilePath";
    }
} 