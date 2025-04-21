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
            thirdPersonTeleporterScript.TeleportTo(teleportTargetChild, teleportOffset, 25);
            var personObjectScript = teleportTargetChild.GetComponent<PersonNode>();

            StartCoroutine(hallOfHistoryGameObject.GetComponent<HallOfHistory>().SetFocusPersonNode(personObjectScript));
            StartCoroutine(hallOfFamilyPhotosGameObject.GetComponent<HallOfFamilyPhotos>().SetFocusPersonNode(personObjectScript));
        }
    }
} 