using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CenterAndScale(this.transform.parent.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        CenterAndScale(this.transform.parent.localPosition.z);
    }

    void CenterAndScale(float parentSize)
    {        
        this.transform.localScale = new Vector3(0.5f, 0.5f, parentSize);
        this.transform.localPosition = new Vector3(0, 0, parentSize / -2);
    }
}
