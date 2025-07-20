using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataObjects;

public class Edge : MonoBehaviour
{
    private GameObject leftSphere;
    private GameObject rightSphere;
    private int eventLength = 0; // 0 indicates no length
    private float edgeXscale;
    
    void Start()
    {
        edgeXscale = transform.localScale.x;
        leftSphere = transform.GetChild(0).gameObject;
        rightSphere = transform.GetChild(1).gameObject;
    }
    
    void Update()
    {
        // Update edge visualization based on connected nodes
    }
    
    public void SetEventLength(int length)
    {
        eventLength = length;
        UpdateEdgeAppearance();
    }
    
    public int GetEventLength()
    {
        return eventLength;
    }
    
    public void CreateEdge(GameObject leftNode, GameObject rightNode)
    {
        ConnectNodes(leftNode, rightNode);
    }
    
    public void SetEdgeEventLength(int length, float xScale)
    {
        eventLength = length;
        
        // Apply the xScale to the edge width/thickness
        var currentScale = transform.localScale;
        transform.localScale = new Vector3(currentScale.x * xScale, currentScale.y, currentScale.z);
        
        UpdateEdgeAppearance();
    }
    
    public void ConnectNodes(GameObject leftNode, GameObject rightNode)
    {
        if (leftSphere != null)
        {
            leftSphere.transform.position = leftNode.transform.position;
        }
        
        if (rightSphere != null)
        {
            rightSphere.transform.position = rightNode.transform.position;
        }
        
        UpdateEdgePosition();
    }
    
    private void UpdateEdgePosition()
    {
        if (leftSphere != null && rightSphere != null)
        {
            // Calculate midpoint
            Vector3 midpoint = (leftSphere.transform.position + rightSphere.transform.position) / 2;
            transform.position = midpoint;
            
            // Calculate distance and update scale
            float distance = Vector3.Distance(leftSphere.transform.position, rightSphere.transform.position);
            transform.localScale = new Vector3(distance, transform.localScale.y, transform.localScale.z);
            
            // Rotate to face the connection
            Vector3 direction = rightSphere.transform.position - leftSphere.transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    private void UpdateEdgeAppearance()
    {
        // Update visual appearance based on event length
        // Could change color, thickness, etc. based on eventLength
    }
} 