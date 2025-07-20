using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.DataObjects;

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
    private IInteractablePanel lastFrameItemToInteractWith; // Track previous frame's value for comparison
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
        // Don't process input if modal is open
        if (FixOrFlagPhotoHandler.IsModalOpen) return;
        
        if (_input.previous && itemToInteractWith != null)
            itemToInteractWith.PreviousEventInPanel();
        _input.previous = false;
    }

    private void DoNext()
    {
        // Don't process input if modal is open
        if (FixOrFlagPhotoHandler.IsModalOpen) return;
        
        if (_input.next && itemToInteractWith != null)
            itemToInteractWith.NextEventInPanel();
        _input.next = false;
    }
    
    private void DoInteract()
    {
        // Don't process input if modal is open
        if (FixOrFlagPhotoHandler.IsModalOpen) return;
        
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
        // Store the previous frame's value for comparison
        lastFrameItemToInteractWith = itemToInteractWith;
        
        // Detect the current item to interact with
        itemToInteractWith = detectItemHitByRay();
        
        // Only call doPanelSelectDeselect if itemToInteractWith has changed
        if (lastFrameItemToInteractWith != itemToInteractWith)
        {            
            doPanelSelectDeselect();
        }
        
        DoPrevious();
        DoNext();
        DoInteract();
    }

    private void doPanelSelectDeselect()
    {
        // Clear Previous if applicable
        if (previousItemToInteractWith != null && previousItemToInteractWith != itemToInteractWith)
        {
            previousItemToInteractWith.ClearEventDetailsPanel();
            previousItemToInteractWith = null;
        }
        // Select Current if applicable
        if (itemToInteractWith != null)
        {
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
            // Try to get a component that implements IInteractablePanel
            var panel = hitInfo.collider.GetComponent<TopEventHallPanel>();
            if (panel != null) return panel;
            
            var photoPanel = hitInfo.collider.GetComponent<FamilyPhotoHallPanel>();
            if (photoPanel != null) return photoPanel;
        }
        return null;
    }
} 