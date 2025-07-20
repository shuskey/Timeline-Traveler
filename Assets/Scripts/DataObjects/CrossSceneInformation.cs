using Assets.Scripts.Enums;

namespace Assets.Scripts.DataObjects
{
    /// <summary>
    /// Static class to store information that needs to persist across scene changes.
    /// </summary>
    public static class CrossSceneInformation
    {
        /// <summary>
        /// The ID of the person to start the family tree from
        /// </summary>
        public static int startingDataBaseId { get; set; }

        /// <summary>
        /// The type of family tree being displayed
        /// </summary>
        public static TribeType myTribeType { get; set; }

        /// <summary>
        /// The number of generations to display in the family tree
        /// </summary>
        public static int numberOfGenerations { get; set; }

        /// <summary>
        /// The full path to the RootsMagic database file
        /// </summary>
        public static string rootsMagicDataFileNameWithFullPath { get; set; }

        /// <summary>
        /// The full path to the DigiKam database file
        /// </summary>
        public static string digiKamDataFileNameWithFullPath { get; set; }
    }
} 