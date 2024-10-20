using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class FileUploaderHelper
{
    static FileUploader fileUploaderObject;
    static Action<string> pathCallback;

    static FileUploaderHelper()
    {
        string methodName = "FileRequestCallback"; // We will not use reflection, so as not to complicate things, hardcode :)
        string objectName = typeof(FileUploaderHelper).Name; // But not here

        // Create a helper object for the FileUploader system
        var wrapperGameObject = new GameObject(objectName, typeof(FileUploader));
        fileUploaderObject = wrapperGameObject.GetComponent<FileUploader>();

        // Initializing the JS part of the FileUploader system
        InitFileLoader(objectName, methodName);

    }

    /// <summary>
    /// Requests a file from the user.
    /// Should be called when the user clicks!
    /// </summary>
    /// <param name="callback">Will be called after the user selects a file, the Http path to the file is passed as a parameter</param>
    /// <param name="extensions">File extensions that can be selected, example: ".jpg, .jpeg, .png"</param>
    public static void RequestFile(Action<string> callback, string extensions = ".jpg, .jpeg, .png")
    {
        RequestUserFile(extensions);
        pathCallback = callback;
    }

    /// <summary>
    /// For internal use
    /// </summary>
    /// <param name="path">The path to the file</param>
    public static void SetResult(string path)
    {
        pathCallback.Invoke(path);
        Dispose();
    }

    private static void Dispose()
    {
        ResetFileLoader();
        pathCallback = null;
    }

    // Below we declare external functions from our .jslib file
    [DllImport("__Internal")]
    private static extern void InitFileLoader(string objectName, string methodName);

    [DllImport("__Internal")]
    private static extern void RequestUserFile(string extensions);

    [DllImport("__Internal")]
    private static extern void ResetFileLoader();
}