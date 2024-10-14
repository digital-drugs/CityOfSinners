using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrizeRowScript : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public void SetText(string txt)
    {
        text.text = txt;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
