using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

public class NicknameChecker : MonoBehaviour
{
    //private string connectionString = "server=yourserver;user=youruser;database=yourdatabase;port=yourport;password=yourpassword";

    //private HashSet<string> badWords;

    //void Start()
    //{
    //    LoadBadWords();

    //    string nickname = "����������"; // ������ ��������, ������� ����� ���������.

    //    if (IsNicknameValid(nickname) && !IsNicknameInDB(nickname))
    //    {
    //        Debug.Log("������� �������� � �� ���������� � ���� ������.");
    //    }
    //    else
    //    {
    //        Debug.Log("������� ���������� ��� ��� ���������� � ���� ������.");
    //    }
    //}

    //private void LoadBadWords()
    //{
    //    badWords = new HashSet<string>();

    //    // ���� � ����� � ��������� ������� � ����� Resources
    //    TextAsset badWordsAsset = Resources.Load<TextAsset>("badwords");

    //    if (badWordsAsset != null)
    //    {
    //        string[] words = badWordsAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
    //        foreach (string word in words)
    //        {
    //            badWords.Add(word.Trim().ToLower());
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("�� ������� ����� ���� badwords.txt � ����� Resources.");
    //    }
    //}

    //private bool IsNicknameValid(string nickname)
    //{
    //    // ��������: ������ ������������� � ��������� �����.
    //    if (!Regex.IsMatch(nickname, @"^[a-zA-Z�-��-�]+$"))
    //        return false;

    //    // �������� �� ������������� �������.
    //    foreach (string badWord in badWords)
    //    {
    //        if (nickname.ToLower().Contains(badWord))
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}

    //private bool IsNicknameInDB(string nickname)
    //{
    //    try
    //    {
    //        using (MySqlConnection conn = new MySqlConnection(connectionString))
    //        {
    //            conn.Open();

    //            string sql = "SELECT COUNT(*) FROM users WHERE nickname = @nickname";
    //            MySqlCommand cmd = new MySqlCommand(sql, conn);
    //            cmd.Parameters.AddWithValue("@nickname", nickname);

    //            long count = (long)cmd.ExecuteScalar();
    //            return count > 0;
    //        }
    //    }
    //    catch (MySqlException ex)
    //    {
    //        Debug.LogError("������ ����������� � ���� ������: " + ex.Message);
    //        return true;
    //    }
    //}
}