using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VkFriendUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI firstNameText;
    [SerializeField] private TextMeshProUGUI lastNameText;    
    [SerializeField] private RawImage ava;

    public void Assign(VKFriend f)
    {
        firstNameText.text = f.first_name;
        lastNameText.text = f.last_name;

        StartCoroutine(LoadAva(ava, f.photo_100));
    }

    IEnumerator LoadAva(RawImage ava, string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            ava.texture = myTexture;
        }
    }

}
