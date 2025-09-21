using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataObjects;
using UnityEngine.UIElements;

public class Edge : MonoBehaviour
{
    private GameObject cylinderOrRectangle;
    private GameObject otherConnectionPoint;
    private int eventLength = 0; // 0 indicates no length
    private float edgeXscale;
    
    void Start()
    {
        // The prefab this is attached to has two children, the first is the cylinder or rectangle, the second is the other connection point

        edgeXscale = transform.localScale.x;
        cylinderOrRectangle = transform.GetChild(0).gameObject;
        otherConnectionPoint = transform.GetChild(1).gameObject;
    }
    
    void Update()
    {
        // Update edge visualization based on connected nodes
    }
    
    
    public void CreateEdge(GameObject leftNode, GameObject rightNode)
    {
        // This Gizmo is used to visualize the edge in the scene view
        // two endpoints and a line between them
        var edgeVisualizationGizmosComponent = GetComponent<EdgeVisualizationGizmos>();
        if (edgeVisualizationGizmosComponent != null)
        {
            // Pass the world positions of the start and end points of the edge
            Debug.Log($"CreateEdge called - LeftNode: {leftNode.name} at {leftNode.transform.position}, RightNode: {rightNode.name} at {rightNode.transform.position}");
            edgeVisualizationGizmosComponent.SetGizmoPoints(leftNode.transform.position, rightNode.transform.position);
        }
        // Position and orient the edge to connect the two nodes
        PositionAndOrientEdge(leftNode, rightNode);
    }
    
    /// <summary>
    /// Positions, scales, and rotates the edge to connect two nodes
    /// </summary>
    private void PositionAndOrientEdge(GameObject leftNode, GameObject rightNode)
    {
        if (cylinderOrRectangle == null || otherConnectionPoint == null)
        {
            Debug.LogError("Edge components not properly initialized");
            return;
        }
        
        // Get world positions of the nodes
        Vector3 leftPosition = leftNode.transform.position;
        Vector3 rightPosition = rightNode.transform.position;
        
        // Calculate the midpoint between the two nodes
        Vector3 midpoint = (leftPosition + rightPosition) / 2f;
        
        // Calculate the distance between the nodes
        float distance = Vector3.Distance(leftPosition, rightPosition);
        
        // Calculate the direction from left to right node
        Vector3 direction = (rightPosition - leftPosition).normalized;
        
        // Position the edge at the midpoint
        transform.position = midpoint;
        
        // Scale the cylinder/rectangle to match the distance between nodes
        // Keep the original Y and Z scale, but set X scale to the distance
        Vector3 currentScale = cylinderOrRectangle.transform.localScale;
        cylinderOrRectangle.transform.localScale = new Vector3( currentScale.x, distance, currentScale.z);
        
        // Rotate the cylinder/rectangle to face the direction between nodes
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Position the other connection point at the endpoint
        otherConnectionPoint.transform.position = rightPosition;    
        
        Debug.Log($"Edge positioned at midpoint: {midpoint}, Distance: {distance}, Direction: {direction}");
    }
    
    public void SetEdgeEventLength(int length)
    {
        eventLength = length + 5;  // Starting at the beginning of the year, need to extend to the end of the last year
        
        // Check if Y rotation is positive (90 degrees)
        float yRotation = transform.rotation.eulerAngles.y;
        bool isPositiveYRotation = (yRotation > 0 && yRotation < 180);
        
        // Use negative eventLength for positive Y rotation, positive otherwise
        float scaledLength = isPositiveYRotation ? -eventLength : eventLength;
        
        // Apply the scaling to the parent transform
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(scaledLength, currentScale.y, currentScale.z);
    }
    

} 