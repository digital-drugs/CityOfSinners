using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleImageUi : MonoBehaviour
{
    public Image maleImage;
    public Image femaleImage;
    public Image unicImage;
    public void StartLoadImages(RoleData roleData)
    {
        gameObject.name = roleData.roleId;

        GameRoomUi.instance.StartLoadTexture(maleImage, roleData.maleUrl);
        //GameRoomUi.instance.StartLoadTexture(femaleImage, roleData.maleUrl);
        //GameRoomUi.instance.StartLoadTexture(unicImage, roleData.maleUrl);
        //throw new NotImplementedException();
    }
}
