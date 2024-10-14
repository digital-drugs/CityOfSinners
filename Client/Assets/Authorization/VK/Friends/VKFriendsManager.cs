using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

public class VKFriendsManager : MonoBehaviour
{
    private VKAuthorization vksdk;
    internal void Setup(VKAuthorization vksdk)
    {
        this.vksdk = vksdk;

        vksdk.OnVKLoginSucces += OnVKLoginSucces;
    }

    private void OnVKLoginSucces(object sender, EventArgs e)
    {
        GetVkFriends();
    }

    [DllImport("__Internal")]
    private static extern void VKGetFriends(string objectName, string methodName,  int userId);

    public void GetVkFriends()
    {
        //VKGetFriends(gameObject.name, "SetVKFriends", vksdk.userId);
    }

    [HideInInspector]
    [SerializeField] private VKFriends vkFriends;
    public void SetVKFriends(string response)
    {
        vkFriends = JsonUtility.FromJson<VKFriends>(response);

        AddVkFriends(vkFriends);
    }

    [SerializeField] private Transform friendsContainer;
    [SerializeField] private VkFriendUi vkFriendUiPrefab;
    private void AddVkFriends(VKFriends vkFriends)
    {
        foreach (var f in vkFriends.items)
        {
            if (f.first_name == "DELETED") continue;

            var friendUi = Instantiate(vkFriendUiPrefab, friendsContainer);

            friendUi.Assign(f);
        }
    }

}
