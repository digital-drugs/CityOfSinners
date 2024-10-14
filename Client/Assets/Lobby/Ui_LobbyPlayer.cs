using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ui_LobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_PlayerName;
   public void Assign(string playerName)
    {
        Text_PlayerName.text = playerName;
    }
}
