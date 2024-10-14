using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmileManager : MonoBehaviour
{
    public static SmileManager instance;

    private void Start()
    {
        if(instance==null) { instance = this; }
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
    }

    public List<ChatSprite> sprites = new List<ChatSprite>();

    public Sprite unknownSprite;

    //public Sprite GetSprite(int id)
    //{
    //    var result = sprites.Find(x => x.id == id).sprite;

    //    if (result == null)
    //    {
    //        result = unknownSprite;
    //    }

    //    return result;
    //}

    public Sprite GetSmileSprite(char id)
    {
        var result = sprites.Find(x => x.id == id).sprite;

        if (result == null)
        {
            result = unknownSprite;
        }

        return result;
    }
}

[Serializable]
public class ChatSprite
{
    public char id;
    public Sprite sprite;
}