using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtraEffectUi : MonoBehaviour, IMouseAction
{
    [SerializeField] private Image extraIco;
    public void Assign(Sprite sprite)
    {
        extraIco.sprite = sprite;
    }

    [SerializeField] private float scaleFactor=5;
    private Vector3 originalScale;
    public void StartAction()
    {
        var rect = extraIco.GetComponent<RectTransform>();

        originalScale = rect.localScale;

        rect.localScale = originalScale * scaleFactor;

        //Debug.Log($"{originalSize} {rect.size}");
    }

    public void EndAction()
    {
        var rect = extraIco.GetComponent<RectTransform>();

        rect.localScale = originalScale;
    }  
}
