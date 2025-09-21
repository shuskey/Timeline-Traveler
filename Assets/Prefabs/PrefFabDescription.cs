    using UnityEngine;

    public class PrefabDescription : MonoBehaviour
    {
        [TextArea(3, 10)] // Optional: Makes the field a multi-line text area in the Inspector
        public string description = "Add your prefab description here.";
    }