using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickToInteract : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The layers that are interactable")]
    private LayerMask layerMask;
    [SerializeField]
    [Tooltip("The maximum distance for interactable items")]
    private float maxDistance = 10.0f;
    [SerializeField]
    private string nameOfInteractableDescriptionfield;

    private IInteractablePanel itemToInteractWith;
    private IInteractablePanel previousItemToInteractWith;
    private Camera theMainCamera;

    private StarterAssetsInputs _input;

    private void Awake()
    {
        previousItemToInteractWith = null;
    }

    private void OnEnable()
    {
    }

    private void DoPrevious()
    {
        if (_input.previous && itemToInteractWith != null)
            itemToInteractWith.PreviousEventInPanel();
        _input.previous = false;
    }

    private void DoNext()
    {
        if (_input.next && itemToInteractWith != null)
            itemToInteractWith.NextEventInPanel();
        _input.next = false;
    }
    
    private void DoInteract()
    {
        if (_input.interact && itemToInteractWith != null)
            itemToInteractWith.InteractWithPanel();
        _input.interact = false;
    }

    private void Start()
    {
        theMainCamera = Camera.main;
        _input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        itemToInteractWith = detectItemHitByRay();
        doPanelSelectDeselect();
        DoPrevious();
        DoNext();
        DoInteract();
    }

    private void doPanelSelectDeselect()
    {
        // Clear Previous if applicable
        if (previousItemToInteractWith != null && previousItemToInteractWith != itemToInteractWith)
        {
            Debug.Log($"[ClickToInteract] Clearing previous item: {previousItemToInteractWith.GetType().Name}");
            previousItemToInteractWith.ClearEventDetailsPanel();
            previousItemToInteractWith = null;
        }
        // Select Current if applicable
        if (itemToInteractWith != null)
        {
            Debug.Log($"[ClickToInteract] Selecting new item: {itemToInteractWith.GetType().Name}");
            previousItemToInteractWith = itemToInteractWith;
            itemToInteractWith.DisplayDetailsInEventDetailsPanel();
        }
    }

    private IInteractablePanel detectItemHitByRay()
    {
        Ray ray = theMainCamera.ViewportPointToRay(Vector3.one / 2.0f);

        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, maxDistance, layerMask))
        {
            Debug.Log($"[ClickToInteract] Raycast hit: {hitInfo.collider.name} on layer {hitInfo.collider.gameObject.layer}");
            
            // Try to get a component that implements IInteractablePanel
            var panel = hitInfo.collider.GetComponent<TopEventHallPanel>();
            if (panel != null) 
            {
                Debug.Log($"[ClickToInteract] Found TopEventHallPanel: {hitInfo.collider.name}");
                return panel;
            }
            
            var photoPanel = hitInfo.collider.GetComponent<FamilyPhotoHallPanel>();
            if (photoPanel != null) 
            {
                Debug.Log($"[ClickToInteract] Found FamilyPhotoHallPanel: {hitInfo.collider.name}");
                return photoPanel;
            }
            
            Debug.LogWarning($"[ClickToInteract] Hit object {hitInfo.collider.name} but no IInteractablePanel component found");
        }
        return null;
    }
} 