using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageUnderCursorUi : MonoBehaviour, IMouseAction
{
    public void StartAction()
    {
        var image = GetComponent<Image>();

        if (image == null) return;

        var position = image.rectTransform.position;

        ImageManager.instance.ShowImageUnderCursor(image, position);
    }

    public void EndAction()
    {
        ImageManager.instance.HideImageUnderCursor();
    }
}
