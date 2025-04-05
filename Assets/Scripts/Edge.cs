using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    private GameObject leftSphere;
    private GameObject rightSphere;
    private int eventLength = 0; // 0 indicates no length
    private float edgeXscale;

    private void Update()
    {
        UpdateCylinderPosition(leftSphere.transform.position, rightSphere.transform.position);
    }

    public void CreateEdge(GameObject beginObject, GameObject endObject)
    {
        leftSphere = beginObject;
        rightSphere = endObject;
        UpdateCylinderPosition(leftSphere.transform.position, rightSphere.transform.position);
    }

    public void SetEdgeEventLength(int eventLength, float xScale)
    {
        this.eventLength = eventLength;
        this.edgeXscale = xScale;
        UpdateCylinderPosition(leftSphere.transform.position, rightSphere.transform.position);
    }

    private void UpdateCylinderPosition(Vector3 beginPoint, Vector3 endPoint)
    {
        Vector3 offset = endPoint - beginPoint;
        Vector3 position = beginPoint + (offset / 2.0f);

        transform.position = position;
        transform.LookAt(beginPoint);
        Vector3 localScale = transform.localScale;
        localScale.z = (endPoint - beginPoint).magnitude;
        if (eventLength != 0)
        {
            transform.localPosition = transform.localPosition + new Vector3(0, 0, (float)eventLength/2);
            localScale.x = (float)eventLength / edgeXscale;  // Make sure we take into accout the original prefab scaling
        }
        transform.localScale = localScale;
    }
}
