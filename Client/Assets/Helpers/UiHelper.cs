using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UiHelper 
{
    public static void ClearContainer(Transform container)
    {
        for (int i = container.childCount-1; i >= 0; i--)
        {
            MoveUiObjectToTrash(container.GetChild(i).gameObject);
        }
    }

    public static void AssignObjectToContainer(GameObject gameObject, Transform container)
    {
        gameObject.transform.SetParent(container.transform);

        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
    }

    public static void AssignObjectToContainer(GameObject gameObject, Transform container, Vector3 localPosition)
    {
        gameObject.transform.SetParent(container.transform);

        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localScale = Vector3.one;
    }

    private static Transform trash;
    
    public static void MoveUiObjectToTrash(GameObject gameObject)
    {
        CheckTrash();
        gameObject.transform.SetParent(trash);
        Object.Destroy(gameObject);
    }
    private static void CheckTrash()
    {
        if (trash == null) trash = new GameObject().transform;
        trash.gameObject.SetActive(false);
    }

    public static void SetupTMPDropMenu(TMP_Dropdown dropDown, System.Type type)
    {
        dropDown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var element in Enum.GetValues(type))
        {
            options.Add(new TMP_Dropdown.OptionData(element.ToString()));
        }

        dropDown.AddOptions(options);
    }

  
}
