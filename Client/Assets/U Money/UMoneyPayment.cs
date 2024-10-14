using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UMoneyPayment : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void UMoneyOpenConfirmURL(string URL);

    public static void OpenConfirmURL(string URL)
    {
        UMoneyOpenConfirmURL(URL);
    }
}
