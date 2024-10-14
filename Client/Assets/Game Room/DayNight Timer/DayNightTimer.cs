using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightTimer : MonoBehaviour
{
    [SerializeField] private RectTransform nightLeft;
    [SerializeField] private RectTransform nightRight;
    [SerializeField] private RectTransform dayLeft;
    [SerializeField] private RectTransform dayRight;  

    [SerializeField] private float fullSizeX = 137;
    [SerializeField] private float FullSizeY = 274;

    private TimerPhase currentPhase;

    public void ResetTimer()
    {
        dayRight.transform.SetAsLastSibling();
        SetFullSize(dayRight);

        dayLeft.transform.SetAsLastSibling();
        SetFullSize(dayLeft);

        nightRight.transform.SetAsLastSibling();
        SetFullSize(nightRight);       

        nightLeft.transform.SetAsLastSibling();
        SetFullSize(nightLeft);

        currentPhase = TimerPhase.nightLeft;
    }

    [SerializeField] private float speed;
    public void SetPhaseTime(int time)
    {
        //Debug.Log($"time {0}");

        if (time == 0) return;

        speed = fullSizeX * 2 / time;
    }

    private bool canResize = false;
    public void SetDay(ParameterDictionary parameters)
    {
        canResize = true;

        var currentPhaseTime = (int)parameters[(byte)Params.Timer];
        SetPhaseTime(currentPhaseTime);

        dayRight.transform.SetAsLastSibling();
        SetFullSize(dayRight);      

        nightRight.transform.SetAsLastSibling();
        SetFullSize(nightRight);

        nightLeft.transform.SetAsLastSibling();
        SetFullSize(nightLeft);

        dayLeft.transform.SetAsLastSibling();
        SetZeroSize(dayLeft);

        currentPhase = TimerPhase.DayLeft;

        //StartCoroutine(DayAnimation());
    }

    public void SetNight(ParameterDictionary parameters)
    {
        canResize = true;

        var currentPhaseTime = (int)parameters[(byte)Params.Timer];
        SetPhaseTime(currentPhaseTime);

        nightRight.transform.SetAsLastSibling();
        SetFullSize(nightRight);

        dayRight.transform.SetAsLastSibling();
        SetFullSize(dayRight);

        dayLeft.transform.SetAsLastSibling();
        SetFullSize(dayLeft);

        nightLeft.transform.SetAsLastSibling();
        SetZeroSize(nightLeft);

        currentPhase = TimerPhase.nightLeft;

        //StartCoroutine(NightAnimation());
    }

    private void SetFullSize(RectTransform rect)
    {
        rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
    }

    private void SetZeroSize(RectTransform rect)
    {
        rect.sizeDelta = new Vector2(0, FullSizeY);
    }

    //[SerializeField] private float animationSpeed = 1f;
    //[SerializeField] private float animationPeriod = 0.01f;
    //IEnumerator DayAnimation()
    //{
    //    var rect = nightRight;

    //    while (rect.sizeDelta.x >0)
    //    {
    //        rect.sizeDelta = new Vector2(rect.sizeDelta.x - speed * Time.deltaTime, FullSizeY);

    //        if (rect.sizeDelta.x >= fullSizeX)
    //        {
    //            rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
    //        }

    //        yield return new WaitForSeconds(animationPeriod);
    //    }

    //    rect = dayLeft;

    //    while (rect.sizeDelta.x < fullSizeX)
    //    {
    //        rect.sizeDelta = new Vector2(rect.sizeDelta.x + speed * Time.deltaTime, FullSizeY);

    //        if (rect.sizeDelta.x >= fullSizeX)
    //        {
    //            rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
    //        }

    //        yield return new WaitForSeconds(animationPeriod);
    //    }       
    //}

    //IEnumerator NightAnimation()
    //{
    //    var rect = dayRight;

    //    while (rect.sizeDelta.x > 0)
    //    {
    //        rect.sizeDelta = new Vector2(rect.sizeDelta.x - speed * Time.deltaTime, FullSizeY);

    //        if (rect.sizeDelta.x >= fullSizeX)
    //        {
    //            rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
    //        }

    //        yield return new WaitForSeconds(animationPeriod);
    //    }

    //    rect = nightLeft;

    //    while (rect.sizeDelta.x < fullSizeX)
    //    {
    //        rect.sizeDelta = new Vector2(rect.sizeDelta.x + speed * Time.deltaTime, FullSizeY);

    //        if (rect.sizeDelta.x >= fullSizeX)
    //        {
    //            rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
    //        }

    //        yield return new WaitForSeconds(animationPeriod);
    //    }
    //}


    private void Update()
    {
        if (!canResize) return;

        switch (currentPhase)
        {
            case TimerPhase.nightLeft: DecreaseSize(dayRight);  break;

            case TimerPhase.nightRight: IcreaseSize(nightLeft); ; break;

            case TimerPhase.DayLeft: DecreaseSize(nightRight);  break;

            case TimerPhase.dayRight: IcreaseSize(dayLeft);  break;
        }
    }



    private void IcreaseSize(RectTransform rect)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x + speed * Time.deltaTime, FullSizeY);

        if (rect.sizeDelta.x >= fullSizeX)
        {
            //Debug.Log($" {rect.name} is full size x");

            rect.sizeDelta = new Vector2(fullSizeX, FullSizeY);
            //canResize = false;

            SwitchPhase();
        }
    }

    private void DecreaseSize(RectTransform rect)
    {
        rect.sizeDelta = new Vector2(rect.sizeDelta.x - speed * Time.deltaTime, FullSizeY);

        if (rect.sizeDelta.x <= 0)
        {
            rect.sizeDelta = new Vector2(0, FullSizeY);
            //canResize = false;

            SwitchPhase();
        }
    }

    private void SwitchPhase()
    {
        switch (currentPhase)
        {
            case TimerPhase.nightLeft: currentPhase = TimerPhase.nightRight; break;

            //case TimerPhase.nightRight: currentPhase = TimerPhase.DayLeft; break;

            case TimerPhase.DayLeft: currentPhase = TimerPhase.dayRight; break;

                //case TimerPhase.dayRight: currentPhase = TimerPhase.nightLeft; break;                           
        }
    }
}

    public enum TimerPhase
{
    nightRight,
    nightLeft,
    dayRight,
    DayLeft,
}
