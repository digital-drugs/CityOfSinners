using UnityEngine;
using System.Collections;

public class OpenUrl : MonoBehaviour
{
    [SerializeField] private string _uri;
    public void OpenURL()
    {
        Application.OpenURL(_uri);
        Debug.Log("is this working?");
    }

}