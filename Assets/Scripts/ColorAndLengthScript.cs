using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAndLengthScript : MonoBehaviour
{
    [SerializeField] [Tooltip("Length")] private int length = 10;
    [SerializeField] [Tooltip("Color")] private Color color = Color.white;

    // Start is called before the first frame update
    void Start()
    {           
            var currentPosition = this.transform.position;
            var currentLocalPosition = this.transform.localPosition;
            this.transform.localPosition = new Vector3(0, 0, length);      
    }

    public void SetColor(Color color)
    {
        var children = GetComponentsInChildren<Renderer>();
        children[0].material.color = color;
    }

    public void SetVisibility(bool isVisible)
    {
        var children = GetComponentsInChildren<Renderer>();
        children[0].forceRenderingOff = !isVisible;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
