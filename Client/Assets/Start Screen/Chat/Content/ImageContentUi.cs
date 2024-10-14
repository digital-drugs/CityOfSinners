using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageContentUi : MonoBehaviour
{
    [SerializeField] private Image contentImage;
    public void Assign(Sprite sprite)
    {
        contentImage.sprite = sprite;
    }  
}
