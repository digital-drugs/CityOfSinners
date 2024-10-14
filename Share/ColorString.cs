using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share
{
    public static class ColorString
    {
        public static string GetColoredFromRole(RoleType role)
        {
            var result = "";

            switch (role)
            {
                case RoleType.Citizen: { result = "Гражданина"; } break;
                case RoleType.Doctor: { result = "Доктора"; } break;
                case RoleType.Guerilla: { result = "Партизана"; } break;
                case RoleType.Saint: { result = "Святого"; } break;
                case RoleType.Commissar: { result = "Комиссара"; } break;
                case RoleType.Witness: { result = "Свидетеля"; } break;

                case RoleType.Mafia: { result = "Мафиози"; } break;
                case RoleType.MafiaBoss: { result = "Босса мафии"; } break;
                case RoleType.Sinner: { result = "Грешника"; } break;

                case RoleType.Maniac: { result = "Маньяка"; } break;
                case RoleType.Werewolf: { result = "Оборотня"; } break;
            }

            //красим строку тегом
            result = GetColoredRole(result);

            return result;
        }

        public static string GetColoredRole(RoleType role)
        {
            var roleRus = Helper.GetRoleNameById_Rus(role);

            var result = GetColoredString(roleRus, ColorId.Role);

            return result;
        }     

        public static string GetColoredRole(string role)
        {
            var result = GetColoredString(role, ColorId.Role);

            return result;
        }

        public static string GetColoredExtra(string extra)
        {
            var result = GetColoredString(extra, ColorId.Extra);

            return result;
        }

        public static string GetColoredSkill(string skill)
        {
            var result = GetColoredString(skill, ColorId.Skill);

            return result;
        }

        public static string GetColoredString(string text, ColorId colorId)
        {
            var result = $"<color=#";

            switch (colorId)
            {
                case ColorId.Player: { result += "2aaed4"; } break;
                case ColorId.Judging: { result += "a7be7f"; } break;
                case ColorId.Extra: { result += "cf1de9"; } break;
                case ColorId.Role: { result += "ff725f"; } break;
                case ColorId.Skill: { result += "d99a3a"; } break;

                case ColorId.Gold: { result += "D99000"; } break;
                case ColorId.Silver: { result += "7B7B7B"; } break;
            }

            result += $">{text}</color>"; 

            return result ;
        }
    }

    public enum ColorId
    {
        Player,
        Judging,
        Extra,
        Role,
        Skill,

        Gold,
        Silver,
    }
}
