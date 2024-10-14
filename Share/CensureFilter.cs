using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share
{
    public class CensureFilter
    {

        public static List<string> BaseFilterWords = new List<string>()
        {
            "хуй", "хуе", "хули", "хуя", "хер",
            "пизд", "пезд", "пзд",
            "ебал", "ебан", "ебут", "ебок", "еблив", "еблан", "ебаб", "ебак",
            "бляд", "блят", "сук", "шлюх", "шлёндр", "гондон", "гандон", "пидор", "пидр", "педик", "педр", "пидар", "мраз",
            "1488", "хохол", "хохло", "кацап", "русня", "москал", "маскал"
        };

        public static List<string> ExceptionWords = new List<string>()
        {
            "страхуй", "страхуе", "хулиган", "херсон", "парикмахер",
            "хлебал", "дебаланс", "перебал", "перебал", "погребал", "икебан", "дебаланс", "колебан", "сгребан", "хлебан", "гребут", "скребут", "шебут", "скребок", "чебок", "погребок", "кебаб",
            "люблят", "злоблят", "слаблят", "сабля", "углублят", "истреблят", "оскорблят", "подоблят", "усугублят", "дроблят", "потреблят", "пособлят", "подоблят",
            "сук", "барсук", "сукно", "сабля", "педро",
            "хохлома"
        };

        public static Dictionary<string, string[]> Similars = new Dictionary<string, string[]>()
        {
            { "а", new string[] { "а", "a", "4" } },
            { "б", new string[] { "б", "b", "6", "ь", "ъ" } },
            { "в", new string[] { "в", "v", "w", "8", "&" } },
            { "г", new string[] { "г", "g", "r" } },
            { "д", new string[] { "д", "d", "g" } },
            { "е", new string[] { "е", "e", "ё", "э", "йо", "з", "3", "yo" } },
            { "ё", new string[] { "ё", "е", "e", "э", "йо", "з", "3", "yo" } },
            { "ж", new string[] { "ж", "j", "zh", "*"} },
            { "з", new string[] { "з", "z", "3" } },
            { "и", new string[] { "и", "i", "й", "u" } },
            { "й", new string[] { "й", "и", "i", "u" } },
            { "к", new string[] { "к", "k" } },
            { "л", new string[] { "л", "l", "ji", "jl" } },
            { "м", new string[] { "м", "m" } },
            { "н", new string[] { "н", "n", "h" } },
            { "о", new string[] { "о", "o", "0" } },
            { "п", new string[] { "п", "p", "n" } },
            { "р", new string[] { "р", "r", "p" } },
            { "с", new string[] { "с", "c", "s" } },
            { "т", new string[] { "т", "t", "m" } },
            { "у", new string[] { "у", "y", "u" } },
            { "ф", new string[] { "ф", "f", "ph" } },
            { "х", new string[] { "х", "x", "h"  } },
            { "ц", new string[] { "ц", "ts", "u", "c", "4" } },
            { "ч", new string[] { "ч", "ch", "4", "u" } },
            { "ш", new string[] { "ш", "щ", "sh", "w", "v" } },
            { "щ", new string[] { "щ", "ш", "sch", "w", "v" } },
            { "ъ", new string[] { "ъ", "б", "b", "6", "ь" } },
            { "ы", new string[] { "ы", "ьi", "бi", "6i", "bi" } },
            { "ь", new string[] { "ь", "б", "b", "6", "ъ" } },
            { "э", new string[] { "э", "ё", "е", "e" , "3", "з" } },
            { "ю", new string[] { "ю", "io", "u", "y", "yo" } },
            { "я", new string[] { "я", "9", "ya", "еа" } },
            { "1", new string[] { "1", "i", "l"  } },
            { "4", new string[] { "4", "а", "a" } },
            { "8", new string[] { "8", "в" } }
        };

        public static List<string> FilterWords = new List<string>();

        static char[] replacers = new char[]
        {
            '*', '#', '@', '&'
        };

        public CensureFilter()
        {
            foreach (var word in BaseFilterWords)
            {
                Dictionary<string, int> temp = new Dictionary<string, int>();

                temp.Add(word, 0);

                for (int i = 0; i < word.Length; i++)
                {
                    if (!Similars.ContainsKey(word[i].ToString()))
                        continue;

                    int limit = temp.Count;

                    foreach (var tWord in temp.ToList())
                    {
                        if (tWord.Value > tWord.Key.Length - 1)
                            continue;

                        string letStr = tWord.Key[tWord.Value].ToString();

                        if (!Similars.ContainsKey(letStr))
                            continue;

                        foreach (var ch in Similars[letStr])
                        {
                            var tWordNew = tWord.Key.Remove(tWord.Value, 1).Insert(tWord.Value, ch);

                            if (!temp.ContainsKey(tWordNew))
                                temp.Add(tWordNew, tWord.Value + ch.Length);
                            else
                                temp[tWordNew] = tWord.Value + ch.Length;

                            if (!FilterWords.Contains(tWordNew))
                                FilterWords.Add(tWordNew);
                        }
                    }
                }

                temp.Clear();
            }
        }

        public static string Beatify(string str, bool trim = true, bool removeExtraSpaces = true, bool removeExtraSymbols = false)
        {
            if (trim)
                str = str.Trim(' ');

            if (removeExtraSpaces)
                for (int i = 0; i < str.Length - 1; i++)
                    if (str[i] == ' ' && str[i + 1] == ' ')
                        str = str.Remove(i--, 1);

            if (removeExtraSymbols)
                for (int i = 0; i < str.Length - 1; i++)
                {
                    var lChar = char.ToLower(str[i]);

                    if (i + 3 < str.Length && lChar == char.ToLower(str[i + 1]) && lChar == char.ToLower(str[i + 2]) && lChar == char.ToLower(str[i + 3]))
                        str = str.Remove(i--, 1);
                }

            return str;
        }

        public static string Process(string str, bool checkExceptions = true, bool replaceAllWord = false, char replaceChar = '*')
        {
            char?[] removedChars = new char?[str.Length];
            bool[] upperChars = new bool[str.Length];

            for (int i = 0, k = 0; i < str.Length; i++, k++)
            {
                if (char.IsUpper(str[i]))
                    upperChars[k] = true;

                if (char.IsWhiteSpace(str[i]) || char.IsPunctuation(str[i]) || (i + 1 < str.Length && char.ToLower(str[i]) == char.ToLower(str[i + 1])))
                {
                    removedChars[k] = str[i];

                    str = str.Remove(i--, 1);
                }
            }

            str = str.ToLower();

            foreach (var word in FilterWords)
            {
                int idx = -1;

                while ((idx = str.IndexOf(word, idx + 1)) != -1)
                {
                    if (checkExceptions)
                    {
                        bool isExcept = false;

                        foreach (var exception in ExceptionWords)
                        {
                            if (!exception.Contains(word))
                                continue;

                            int idxEx = str.IndexOf(exception);

                            if (idxEx == -1)
                                continue;

                            if (idxEx + exception.IndexOf(word) == idx)
                            {
                                isExcept = true;

                                break;
                            }
                        }

                        if (isExcept)
                            continue;
                    }

                    str = str.Remove(idx, word.Length);
                    str = str.Insert(idx, new string(replaceChar, word.Length));
                }
            }

            StringBuilder strB = new StringBuilder();

            int j = 0;

            for (int i = 0; i < removedChars.Length; i++)
            {
                if (removedChars[i] != null)
                    strB.Append(removedChars[i]);
                else if (j < str.Length)
                    strB.Append(str[j++]);

                if (upperChars[i])
                    strB[i] = char.ToUpper(strB[i]);
            }

            if (replaceAllWord)
            {
                int nextPos = -1;

                for (int i = 0; i < strB.Length; i++)
                {
                    if (nextPos == -1 && ((!char.IsWhiteSpace(strB[i]) && !char.IsPunctuation(strB[i])) || strB[i] == replaceChar))
                    {
                        bool containsFilter = false;

                        for (int k = i; k < strB.Length; k++)
                        {
                            if ((char.IsWhiteSpace(strB[k]) || char.IsPunctuation(strB[k])) && strB[k] != replaceChar)
                                break;

                            if (strB[k] == replaceChar)
                                containsFilter = true;

                            nextPos = k;
                        }

                        if (!containsFilter)
                            nextPos = -1;
                    }

                    if (i <= nextPos)
                    {
                        strB[i] = replaceChar;
                    }
                    else
                        nextPos = -1;
                }
            }

            return strB.ToString();
        }

        public static bool BadWord(string str, bool checkExceptions = true, bool replaceAllWord = false, char replaceChar = '*')
        {
            bool isBad = false;

            char?[] removedChars = new char?[str.Length];
            bool[] upperChars = new bool[str.Length];

            for (int i = 0, k = 0; i < str.Length; i++, k++)
            {
                if (char.IsUpper(str[i]))
                    upperChars[k] = true;

                if (char.IsWhiteSpace(str[i]) || char.IsPunctuation(str[i]) || (i + 1 < str.Length && char.ToLower(str[i]) == char.ToLower(str[i + 1])))
                {
                    removedChars[k] = str[i];

                    str = str.Remove(i--, 1);
                }
            }

            str = str.ToLower();

            foreach (var word in FilterWords)
            {
                int idx = -1;

                while ((idx = str.IndexOf(word, idx + 1)) != -1)
                {
                    if (checkExceptions)
                    {
                        bool isExcept = false;

                        foreach (var exception in ExceptionWords)
                        {
                            if (!exception.Contains(word))
                                continue;

                            int idxEx = str.IndexOf(exception);

                            if (idxEx == -1)
                                continue;

                            if (idxEx + exception.IndexOf(word) == idx)
                            {
                                isExcept = true;

                                break;
                            }
                        }

                        if (isExcept)
                            continue;
                    }

                    str = str.Remove(idx, word.Length);
                    str = str.Insert(idx, new string(replaceChar, word.Length));
                }
            }

            StringBuilder strB = new StringBuilder();

            int j = 0;

            for (int i = 0; i < removedChars.Length; i++)
            {
                if (removedChars[i] != null)
                    strB.Append(removedChars[i]);
                else if (j < str.Length)
                    strB.Append(str[j++]);

                if (upperChars[i])
                    strB[i] = char.ToUpper(strB[i]);
            }

            if (replaceAllWord)
            {
                int nextPos = -1;

                for (int i = 0; i < strB.Length; i++)
                {
                    if (nextPos == -1 && ((!char.IsWhiteSpace(strB[i]) && !char.IsPunctuation(strB[i])) || strB[i] == replaceChar))
                    {
                        bool containsFilter = false;

                        for (int k = i; k < strB.Length; k++)
                        {
                            if ((char.IsWhiteSpace(strB[k]) || char.IsPunctuation(strB[k])) && strB[k] != replaceChar)
                                break;

                            if (strB[k] == replaceChar)
                                containsFilter = true;

                            nextPos = k;
                        }

                        if (!containsFilter)
                            nextPos = -1;
                    }

                    if (i <= nextPos)
                    {
                        strB[i] = replaceChar;
                    }
                    else
                        nextPos = -1;
                }
            }

            return isBad;
        }
    }

}

[Serializable]
public class CensuredWords
{
    public string userName;
    public string beforeCensuredUserName;
    public string afterCensuredUserName;

    public CensuredWords(string userName, string beforeCensuredUserName, string afterCensuredUserName)
    {
        this.userName = userName;
        this.beforeCensuredUserName = beforeCensuredUserName;
        this.afterCensuredUserName = afterCensuredUserName;
    }
}
