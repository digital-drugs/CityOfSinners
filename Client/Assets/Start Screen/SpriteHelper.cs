using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteHelper : MonoBehaviour
{
    public static SpriteHelper ins;

    private void Awake()
    {
        ins = this;
    }

    [SerializeField] private Sprite Sprite_Coin;
    public Sprite GetSprite_Coin { get => Sprite_Coin; }

    [SerializeField] private Sprite Sprite_Diamond;
    public Sprite GetSprite_Diamond { get => Sprite_Diamond; }
}
