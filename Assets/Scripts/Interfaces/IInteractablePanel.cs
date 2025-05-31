using UnityEngine;

/// <summary>
/// Interface for panels that can be interacted with using the ClickToInteract system
/// </summary>
public interface IInteractablePanel
{
    /// <summary>
    /// Called when the panel is selected (looked at)
    /// </summary>
    void DisplayDetailsInEventDetailsPanel();
    
    /// <summary>
    /// Called when the panel is deselected
    /// </summary>
    void ClearEventDetailsPanel();
    
    /// <summary>
    /// Called when the next input is pressed
    /// </summary>
    void NextEventInPanel();
    
    /// <summary>
    /// Called when the previous input is pressed
    /// </summary>
    void PreviousEventInPanel();
    
    /// <summary>
    /// Called when the interact input is pressed
    /// </summary>
    void InteractWithPanel();
} 