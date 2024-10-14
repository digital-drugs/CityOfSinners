using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDeskUi : MonoBehaviour
{
    [SerializeField] private GameObject marker;
    public void DisableMarker()
    {
        marker.SetActive(false);
    }

    public void EnableMarker()
    {
        marker.SetActive(true);
    }

   
}
