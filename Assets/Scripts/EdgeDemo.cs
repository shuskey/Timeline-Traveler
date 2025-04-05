using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeDemo : MonoBehaviour
{
    [SerializeField]
    private Transform cylinderPrefab;

    private GameObject leftSphere;
    private GameObject rightSphere;
    private GameObject anotherSphere;

    private GameObject cylinderEdge;
    private GameObject cylinderEdge1;
    private GameObject cylinderEdge2;

    private void Start()
    {
        leftSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftSphere.transform.localScale = Vector3.one * 2f;
        rightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightSphere.transform.localScale = Vector3.one * 2f;

        anotherSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anotherSphere.transform.localScale = Vector3.one * 2f;

        leftSphere.transform.position = new Vector3(-10, 0, 0);
        rightSphere.transform.position = new Vector3(10, 0, 0);
        anotherSphere.transform.position = new Vector3(0, -10, 0);

        cylinderEdge = Instantiate<GameObject>(cylinderPrefab.gameObject, Vector3.zero, Quaternion.identity);
        var edgeScript = cylinderEdge.GetComponent<Edge>();
        edgeScript.CreateEdge(leftSphere, rightSphere);

        cylinderEdge1 = Instantiate<GameObject>(cylinderPrefab.gameObject, Vector3.zero, Quaternion.identity);
        edgeScript = cylinderEdge1.GetComponent<Edge>();
        edgeScript.CreateEdge(leftSphere, anotherSphere);

        cylinderEdge2 = Instantiate<GameObject>(cylinderPrefab.gameObject, Vector3.zero, Quaternion.identity);
        edgeScript = cylinderEdge2.GetComponent<Edge>();
        edgeScript.CreateEdge(anotherSphere, rightSphere);
    }

    private void Update()
    {
        leftSphere.transform.position = new Vector3(-10, -20f * Mathf.Sin(Time.time), 0);
        rightSphere.transform.position = new Vector3(10, 20f * Mathf.Sin(Time.time), 0);
        anotherSphere.transform.position = new Vector3(20f * Mathf.Sin(Time.time), -10, 0);
    }
}
