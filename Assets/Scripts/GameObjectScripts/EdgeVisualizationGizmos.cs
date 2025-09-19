using System.Collections.Generic;
using UnityEngine;

public class EdgeVisualizationGizmos : MonoBehaviour
{
    private Vector3 startPoint = new Vector3();
    private Vector3 endPoint = new Vector3();
    public void SetGizmoPoints(Vector3 startPoint, Vector3 endPoint)
    {
        // These are the world positions of the start and end points of the edge
        this.startPoint = startPoint;
        this.endPoint = endPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (startPoint != null && endPoint != null)
        {
            Gizmos.DrawLine(startPoint, endPoint);
        }

        Gizmos.color = Color.red;
        if (startPoint != null)
        {
            Gizmos.DrawSphere(startPoint, 0.25f);
        }
        if (endPoint != null)
        {
            Gizmos.DrawSphere(endPoint, 0.25f);
        }
        
    }

    
}
