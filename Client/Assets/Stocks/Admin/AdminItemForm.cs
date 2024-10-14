using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminItemForm : MonoBehaviour
{
    [SerializeField] private TMP_InputField number;

    [SerializeField] private TMP_Dropdown resourse;

    [SerializeField] private TMP_Dropdown extraId;
    [SerializeField] private TMP_Dropdown roleType;

    [SerializeField] private TMP_InputField amount;


    public void Start()
    {
        UnityEngine.Debug.Log("ItemForm Start");

        UiHelper.SetupTMPDropMenu(resourse, typeof(ResourseType));
        UiHelper.SetupTMPDropMenu(extraId, typeof(ExtraEffect));
        UiHelper.SetupTMPDropMenu(roleType, typeof(RoleType));

        OnChangeResourseDrop();
    }

    int id = 0;
    int setId = 0;
    public void ShowItem(int newNumber, int setId)
    {
        this.gameObject.SetActive(true);

        //UiHelper.SetupTMPDropMenu(resourse, typeof(ResourseType));
        id = 0;
        this.setId = setId;

        number.text = (newNumber + 1).ToString();

        OnChangeResourseDrop();
    }

    public void ShowItem(ParameterDictionary data)
    {
        this.gameObject.SetActive(true);

        //UiHelper.SetupTMPDropMenu(resourse, typeof(ResourseType));

        id = (int)data[(byte)Params.Id];
        setId = (int)data[(byte)Params.SetId];

        number.text = ((int)data[(byte)Params.Number]).ToString();
        amount.text = ((int)data[(byte)Params.Amount]).ToString();

        var resourseText = (string)data[(byte)Params.Resourse];
        int optionIndex = this.resourse.options.FindIndex(x => x.text == resourseText);
        this.resourse.value = optionIndex;

        var text = (string)data[(byte)Params.ResourseId];
        switch (resourse.value)
        {
            case (int)ResourseType.extra:

                optionIndex = this.extraId.options.FindIndex(x => x.text == text);
                this.extraId.value = optionIndex;

                break;

            case (int)ResourseType.energy:

                optionIndex = this.roleType.options.FindIndex(x => x.text == text);
                this.roleType.value = optionIndex;

                break;
        }

        OnChangeResourseDrop();

    }

    public void OnChangeResourseDrop()
    {
        UnityEngine.Debug.Log("OnChangeResourseDrop " + resourse.value);

        extraId.gameObject.SetActive(false);
        roleType.gameObject.SetActive(false);

        switch (resourse.value)
        {
            case (int)ResourseType.extra:

                extraId.gameObject.SetActive(true);

                break;

            case (int)ResourseType.energy:

                roleType.gameObject.SetActive(true);

                break;
        }


    }

    public void SaveItem()
    {
        UnityEngine.Debug.Log("Save Item");

        var data = new Dictionary<byte, object>();

        string resourseId = "";

        switch (resourse.value)
        {
            case (int)ResourseType.extra:

                resourseId = ((ExtraEffect)extraId.value).ToString();

                break;

            case (int)ResourseType.energy:

                resourseId = ((RoleType)roleType.value).ToString();

                break;
        }

        data.Add((byte)Params.Id, id);
        data.Add((byte)Params.SetId, setId); ;
        data.Add((byte)Params.Number, int.Parse(number.text));
        data.Add((byte)Params.Amount, int.Parse(amount.text));
        data.Add((byte)Params.Resourse, ((ResourseType)resourse.value).ToString());
        data.Add((byte)Params.ResourseId, resourseId);

        //UnityEngine.Debug.Log("id " + id);
        //UnityEngine.Debug.Log("setId " + setId);
        //UnityEngine.Debug.Log("number " + int.Parse(number.text));
        //UnityEngine.Debug.Log("amount " + int.Parse(amount.text));
        //UnityEngine.Debug.Log("resourse " + ((ResourseType)resourse.value).ToString());
        //UnityEngine.Debug.Log("resourseId " + resourseId);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.SaveItem,
            data,
            PhotonManager.Inst.sendOptions);

        this.gameObject.SetActive(false);
    }
}
