using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGrabPlayer : MonoBehaviour
{
    private PersonNode _lastPersonNode = null;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.parent = gameObject.transform.parent;
            var personNodeScript = GetComponentInParent<PersonNode>();
            
            // Check if we're already on the same person's platform
            if (_lastPersonNode != null && _lastPersonNode == personNodeScript)
            {
                Debug.Log("TriggerGrabPlayer: Player already on same platform - skipping duplicate processing");
                return;
            }
            
            _lastPersonNode = personNodeScript;
            
            // log this trigger grab player
            Debug.Log("TriggerGrabPlayer: " + gameObject.name + 
            "Parent: " + gameObject.transform.parent.name +
            "Triggered on " + other.gameObject.name);
            personNodeScript.UpdatePersonDetailsWithThisPerson((int)other.gameObject.transform.position.z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            /** Triggers are coming out of order sometimes. Lets not clear the person details.  

            other.transform.parent = null;
            var personNodeScript = GetComponentInParent<PersonNode>();
            personNodeScript.ClearPersonDetails();
            **/
            
            // Clear the last person tracking when exiting
            _lastPersonNode = null;
            
            // log this trigger exit player
            Debug.Log("TriggerGrabPlayer: " + gameObject.name + 
            "Parent: " + gameObject.transform.parent.name +
            "Exited on " + other.gameObject.name);
        }
    }
} 