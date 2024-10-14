using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Canvas mainCanvas;

    public float systemFontSize = 16;
    public float dialogFontSize = 16;
    public CensureFilter censureFilter { get; private set; }

    public Color Color_CardInSlot;
    public long userId { get; private set; }
    public void SetUserId(ParameterDictionary parameters)
    {
        userId = (long)parameters[(byte)Params.UserId];

        ServerLogger.ins.AddLog($"set id on login => {DateTime.Now.TimeOfDay} => {userId}");
    }

    public string userName { get; private set; }
    public void SetUserName(ParameterDictionary parameters)
    {
        userName=(string)parameters[(byte)Params.UserName];
    }

    public PlayerQueueStatus playerStatus { get; private set; }
    public event EventHandler OnPlayerStatusChange;
    public void SetPlayerStatus(ParameterDictionary parameters)
    {
        playerStatus = (PlayerQueueStatus)parameters[(byte)Params.PlayerStatus];

        OnPlayerStatusChange?.Invoke(this, EventArgs.Empty);
    }

    private void Awake()
    {
        if (instance == null) 
        {
            instance = this;
        }
        else
        { 
            Destroy(gameObject);
            return; 
        }

        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;

        //censureFilter = new CensureFilter();
    }

    private void Update()
    {
        CheckCursorTarget();
    }

    private IMouseAction currentMouseAction;
    private IExtraAction currentExtraAction;
    private void CheckCursorTarget()
    {
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        PointerEventData eventData = new PointerEventData(EventSystem.current);

        eventData.position = Input.mousePosition;

        EventSystem.current.RaycastAll(eventData, raycastResults);

        var findMouseAction = false;

        var findExtraAction = false;

        foreach (var r in raycastResults)
        {
            var iMouseAction = r.gameObject.GetComponent<IMouseAction>();

            if (iMouseAction != null)
            {
                if (currentMouseAction != iMouseAction )
                {
                    if (currentMouseAction != null) currentMouseAction.EndAction();

                    currentMouseAction = iMouseAction;

                    currentMouseAction.StartAction();
                }

                findMouseAction = true;
            }

            var iExtraAction = r.gameObject.GetComponent<IExtraAction>();

            if (iExtraAction != null)
            {
                if (currentExtraAction != iExtraAction)
                {
                    if (currentExtraAction != null) currentExtraAction.EndAction();

                    currentExtraAction = iExtraAction;

                    currentExtraAction.StartAction();
                }

                findExtraAction = true;
            }
        }

        if (!findMouseAction)
        {
            if (currentMouseAction != null) currentMouseAction.EndAction();
            currentMouseAction = null;
        }

        if (!findExtraAction)
        {
            if (currentExtraAction != null) currentExtraAction.EndAction();
            currentExtraAction = null;
        }
    }
}
