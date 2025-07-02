using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTeleportToChild : MonoBehaviour
{
    public Transform teleportTargetChild;
    public Vector3 teleportOffset = Vector3.zero;
    public GameObject hallOfHistoryGameObject;
    public GameObject hallOfFamilyPhotosGameObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var playerObject = other.gameObject;

            var thirdPersonTeleporterScript = other.GetComponent<ThirdPersonTeleporter>();
            
            // Subscribe to teleportation completion event
            thirdPersonTeleporterScript.onTeleportationComplete.AddListener(OnTeleportationComplete);
            
            thirdPersonTeleporterScript.TeleportTo(teleportTargetChild, teleportOffset, 25);
        }
    }

    private void OnTeleportationComplete(Transform teleportTarget)
    {
        // Only handle if this is our target
        if (teleportTarget == teleportTargetChild)
        {
            var personObjectScript = teleportTargetChild.GetComponent<PersonNode>();
            
            // Position the Hall of Family Photos after teleportation is complete
            StartCoroutine(hallOfFamilyPhotosGameObject.GetComponent<HallOfFamilyPhotos>().SetFocusPersonNode(personObjectScript));
            
            // Unsubscribe from the event
            var thirdPersonTeleporterScript = FindFirstObjectByType<ThirdPersonTeleporter>();
            if (thirdPersonTeleporterScript != null)
            {
                thirdPersonTeleporterScript.onTeleportationComplete.RemoveListener(OnTeleportationComplete);
            }
        }
    }
} 