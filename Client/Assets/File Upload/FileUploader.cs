using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileUploader : MonoBehaviour
{
    private void Start()
    {
        // We don't need to delete it on the new scene, because system is singletone
        DontDestroyOnLoad(gameObject);
    }

    // This method is called from JS via SendMessage
    void FileRequestCallback(string path)
    {
        // Sending the received link back to the FileUploaderHelper
        FileUploaderHelper.SetResult(path);
    }
}