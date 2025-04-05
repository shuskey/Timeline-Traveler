using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGrabPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = gameObject.transform.parent;
            var personNodeScript = GetComponentInParent<PersonNode>();
            personNodeScript.UpdatePersonDetailsWithThisPerson((int)other.gameObject.transform.position.z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = null;
            var personNodeScript = GetComponentInParent<PersonNode>();
            personNodeScript.ClearPersonDetails();
        }
    }
} 