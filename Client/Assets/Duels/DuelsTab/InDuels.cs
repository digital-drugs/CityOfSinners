using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InDuels : MonoBehaviour
{
    [SerializeField] private TMP_Text caption;

    [SerializeField] private Transform topContent;
    [SerializeField] private Transform tableContent;
    [SerializeField] private Transform awardsLevelsContent;
    [SerializeField] private TopRowScript topRowUi;
    [SerializeField] private TableRowScript tableRowUi;
    [SerializeField] private AwardLevelScript awardLevelui;

    [SerializeField] private TextMeshProUGUI wheelPoints;
    [SerializeField] private TextMeshProUGUI prize;
    [SerializeField] private Button rotateButton;


    public void ShowTab(ParameterDictionary parameters)
    {
        this.gameObject.SetActive(true);

        caption.text = "Дуэль заканчивается " + parameters[(byte)Params.End];

        UnityEngine.Debug.Log("Show duels tab");

        var rows = (Dictionary<int, object>)parameters[(byte)Params.users];

        ShowDuelPersonalTop(rows);
        BuildTable(parameters);
        BuildWheelPart(parameters);
        BuildAwardsLevels(parameters);
    }

    public void BuildAwardsLevels(ParameterDictionary parameters)
    {
        var levels = (Dictionary<int, object>)parameters[(byte)Params.Levels];

        //UnityEngine.Debug.Log("LEVELS COUNT " + levels.Count);

        int winsInDuel = (int)parameters[(byte)Params.clanDuelWins];
        
        UiHelper.ClearContainer(awardsLevelsContent);
        int c = 0;
        foreach (var l in levels)
        {
            var data = (Dictionary<byte, object>)l.Value;
            var amount = (int)data[(byte)Params.Amount];
            string s = "";

            if (winsInDuel == 0)
            {
                s = amount.ToString();
            }
            else
            {
                if (winsInDuel >= (int)data[(byte)Params.Amount])
                {
                    s = amount.ToString() + "/" + amount.ToString();
                    winsInDuel = winsInDuel - amount;
                }
                else
                {
                    s = winsInDuel.ToString() + "/" + amount.ToString();
                    winsInDuel = 0;
                }
            }

            UnityEngine.Debug.Log(s);
            c++;
            //Добавление элементов со строкой на страницу
            AddAwardLevelUi(s, c);
        }
    }

    private void AddAwardLevelUi(string s, int c)
    {
        var newEleemntUi = Instantiate(awardLevelui);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, awardsLevelsContent);

        newEleemntUi.Assign(s,c);
    }

    public void ShowDuelPersonalTop(Dictionary<int, object> rows)
    {
        //UnityEngine.Debug.Log(rows.Count);

        UiHelper.ClearContainer(topContent);
        foreach (var row in rows)
        {
            var data = (Dictionary<byte, object>)row.Value;

            //UnityEngine.Debug.Log("Name " + (string)data[(byte)Params.Name]);
            AddElement(data);
        }
    }

    public void AddElement(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(topRowUi);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, topContent);

        newEleemntUi.Assign(data);
    }

    public void BuildWheelPart(ParameterDictionary parameters)
    {
        int wheelCost = (int)parameters[(byte)Params.wheelCost];
        int wheelPoints = (int)parameters[(byte)Params.wheelPoints];

        this.wheelPoints.text = wheelPoints + "/" + wheelCost;
        if (wheelPoints >= wheelCost)
        {
            rotateButton.interactable = true;
        }
        else
        {
            rotateButton.interactable = false;
        }

        if (parameters.ContainsKey((byte)Params.prize))
        {
            var prize = (Dictionary<byte, object>)parameters[(byte)Params.prize];
            int amount = (int)prize[(byte)Params.Amount];
            ResourseType resourse = (ResourseType)prize[(byte)Params.Resourse];
            this.prize.text = amount + " " + resourse;
        }
    }

    public void RotateWheel()
    {
        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.RotateWheel,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    /// <summary>
    /// Построить попарную таблицу
    /// </summary>
    public void BuildTable(ParameterDictionary parameters)
    {
        var c1missionsRows = new List<Dictionary<byte, object>>();
        var c2missionsRows = new List<Dictionary<byte, object>>();

        var clans = (Dictionary<int, object>)parameters[(byte)Params.clans];
        int c1Id = (int)((Dictionary<byte, object>)clans[0])[(byte)Params.Id];
        int c2Id = (int)((Dictionary<byte, object>)clans[1])[(byte)Params.Id];

        //UnityEngine.Debug.Log("clans count " + clans.Count);
        //UnityEngine.Debug.Log("c1Id " + c1Id);
        //UnityEngine.Debug.Log("c2Id " + c2Id);

        var rows = (Dictionary<int, object>)parameters[(byte)Params.users];

        //System.Comparison<Dictionary<byte, object>> comparison = CompareRows();

        RowsComparer rComparer = new RowsComparer();

        foreach(var r in rows)
        {
            var data = (Dictionary<byte, object>)r.Value;

            UnityEngine.Debug.Log(data[(byte)Params.missionNumber]);

            if ((int)data[(byte)Params.clanId] == c1Id)
            {
                c1missionsRows.Add(data);
            }
            else
            {
                c2missionsRows.Add(data);
            }
        }

        //UnityEngine.Debug.Log("c1missions count " + c1missionsRows.Count);
        //UnityEngine.Debug.Log("c2missions count " + c2missionsRows.Count);

        c1missionsRows.Sort(rComparer);
        c2missionsRows.Sort(rComparer);

        foreach (var e in c2missionsRows)
        {
            //UnityEngine.Debug.Log(e[(byte)Params.missionNumber]);
        }

        var missions = (Dictionary<int, object>)parameters[(byte)Params.missions];

        string name1, name2, points1, points2;

        UiHelper.ClearContainer(tableContent);
        for (int i = 0; i < missions.Count; i++)
        {
            var m = (Dictionary<byte, object>)missions[i];

            //UnityEngine.Debug.Log("mission Id " + (DuelsAchiveId)m[(byte)Params.Id]);
            string missionId = ((DuelsAchiveId)m[(byte)Params.Id]).ToString();

            if (i < c1missionsRows.Count)
            {
                name1 = (string)(c1missionsRows[i])[(byte)Params.Name];
                points1 = ((int)(c1missionsRows[i])[(byte)Params.Amount]).ToString();
            }
            else
            {
                name1 = "";
                points1 = "";
            }

            if (i < c2missionsRows.Count)
            {
                name2 = (string)(c2missionsRows[i])[(byte)Params.Name];
                points2 = ((int)(c2missionsRows[i])[(byte)Params.Amount]).ToString();
            }
            else
            {
                name2 = "";
                points2 = "";
            }

            //UnityEngine.Debug.Log("row1 " + name1 + " " + points1);
            //UnityEngine.Debug.Log("row2 " + name2 + " " + points2);
            
            AddTableRow(missionId, name1, points1, points2, name2);

        }
    }

    public void AddTableRow(string missionId, string name1, string points1, string points2, string name2)
    {
        var newEleemntUi = Instantiate(tableRowUi);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, tableContent);

        newEleemntUi.Assign(missionId, name1, points1, points2, name2);
    }

    public class RowsComparer : IComparer<Dictionary<byte, object>>
    {
        public int Compare(Dictionary<byte, object> x, Dictionary<byte, object> y)
        {
            int m1 = (int)x[(byte)Params.missionNumber];
            int m2 = (int)y[(byte)Params.missionNumber];


            if (m1 > m2) return 1;
            if (m2 > m1) return -1;
            return 0;
        }
    }
}


