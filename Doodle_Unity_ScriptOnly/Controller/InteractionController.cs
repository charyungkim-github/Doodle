using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class InteractionController : MonoBehaviour
{
    ObjectManipulator objectManipulator;
    BoundingBox boundingBox;

    void Start()
    {
        objectManipulator = GetComponent<ObjectManipulator>();
        boundingBox = GetComponent<BoundingBox>();
    }

    public void TurnOn(bool on)
    {
        objectManipulator.enabled = on;
        boundingBox.enabled = on;
    }

    public void OnSelected()
    {
        DeleteObjectParams.isSelectedOnDeleteObject = true;
        DeleteObjectParams.selectedDeleteObject = this.gameObject;
    }

    public void OffSelected()
    {
        DeleteObjectParams.isSelectedOnDeleteObject = false;
        DeleteObjectParams.selectedDeleteObject = null;
    }
}
