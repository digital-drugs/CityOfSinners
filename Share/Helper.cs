using System;
using System.Collections.Generic;

namespace Share
{       
    public class Helper
    {
        public static List<T> GetListClone<T>(List<T> list)
        {
            var result = new List<T>();

            foreach (var element in list)
            {
                result.Add(element);
            }

            return result;
        }

        public static Dictionary<K,V> GetDictionaryClone<K,V>(Dictionary<K, V> dictionary)
        {
            var result = new Dictionary<K, V>();

            foreach (var element in dictionary)
            {
                result.Add(element.Key, element.Value);
            }

            return result;
        }

     

        public static object GetEnumElement<T>(string name)
        {
            var result = Enum.Parse(typeof(T), name);

            return result;
        }

      

        public static string GetMessageFromReason(ChangeTeamReason reason)
        {
            var result = "";
            switch(reason)
            {
                case ChangeTeamReason.Saint: result = "Вас посетил Святой, Вы теперь играете за команду мирных"; break;
                case ChangeTeamReason.Sinner: result = "Вас посетил Грешник, Вы теперь играете за команду злых"; break;
                case ChangeTeamReason.VisitGood: result = "Вы посетили мирного жителя, Вы теперь играете за команду мирных"; break;
                case ChangeTeamReason.VisitBad: result = "Вы посетили мафию, Вы теперь играете за команду злых"; break;
            }

            return result;
        }

        public static string LeagueIdToRusName(LeagueId id)
        {
            var result = "";

            switch (id)
            {
                case LeagueId.League_1: { result = "Новички"; } break;
                case LeagueId.League_2: { result = "Золотая"; } break;
                case LeagueId.League_3: { result = "Бриллиантовая"; } break;
                case LeagueId.League_4: { result = "Сапфировая"; } break;
                case LeagueId.League_5: { result = "Рубиновая"; } break;
                case LeagueId.League_6: { result = "Олимп Святых"; } break;
            }

            return result;
        }

        public static string RoomTypeToRus_Name(RoomType id)
        {
            var result = "";

            switch (id)
            {
                case RoomType._8: { result = "8"; } break;
                case RoomType._12: { result = "12"; } break;
                case RoomType._16: { result = "16"; } break;
                case RoomType._20: { result = "20"; } break;
            }

            return result;
        }

        public static string CostIdToRusName(CostId id)
        {
            var result = "";

            switch (id)
            {
                case CostId._20: { result = "20"; } break;
                case CostId._100: { result = "100"; } break;
                case CostId._200: { result = "200"; } break;
                case CostId._5000: { result = "5000"; } break;
            }

            return result;
        }

        public static Role_Effect GetEffectByRoleType(RoleType roleType)
        {
            Role_Effect result = new Role_Effect();
            //Effect result = new Effect();

            switch (roleType)
            {
                case RoleType.Doctor: 
                    { 
                        result .effect = RoleEffect.Doctor;
                    } 
                    break;
                case RoleType.Guerilla:
                    {
                        result.effect = RoleEffect.Guerilla;
                    }
                    break;
                case RoleType.Saint:
                    {
                        result.effect = RoleEffect.Saint;
                    }
                    break;
                case RoleType.Witness:
                    {
                        result.effect = RoleEffect.Witness;
                    }
                    break;
                case RoleType.Commissar:
                    {
                        result.effect = RoleEffect.Commissar;
                    }
                    break;
                case RoleType.Mafia:
                    {
                        result.effect = RoleEffect.Mafia;
                    }
                    break;
                case RoleType.MafiaBoss:
                    {
                        result.effect = RoleEffect.MafiaBoss;
                    }
                    break;
                case RoleType.Maniac:
                    {
                        result.effect = RoleEffect.Maniac;
                    }
                    break;
                case RoleType.Sinner:
                    {
                        result.effect = RoleEffect.Sinner;
                    }
                    break;
                case RoleType.Werewolf:
                    {
                        result.effect = RoleEffect.Werewolf;
                    }
                    break;
            }

            return result;
        }

        public static DurationType GetExtraDurationById(string extraId)
        {
            var enumExtraId = (ExtraEffect)GetEnumElement<ExtraEffect>(extraId);
            return GetExtraDurationById(enumExtraId);
        }

        public static DurationType GetExtraDurationById(ExtraEffect extraId)
        {
            var result = DurationType.Instant;

            switch(extraId)
            {
                case ExtraEffect.Smoke: { result = DurationType.NightEnd; } break;
                case ExtraEffect.Kalash: { result = DurationType.Instant; } break;
                case ExtraEffect.Armor: { result = DurationType.Instant; } break;
                case ExtraEffect.SleepElexir: { result = DurationType.NightEnd; } break;
                case ExtraEffect.Bit: { result = DurationType.DayEnd; } break;
                case ExtraEffect.Сonfession: { result = DurationType.Instant; } break;
                case ExtraEffect.Eye: { result = DurationType.Instant; } break;
                case ExtraEffect.HotWater: { result = DurationType.Instant; } break;
                case ExtraEffect.GoldenFleece: { result = DurationType.Instant; } break;
                case ExtraEffect.WireTape: { result = DurationType.NightEnd; } break;
                case ExtraEffect.RadioSet: { result = result = DurationType.Instant; } break;
                case ExtraEffect.Iron: { result = DurationType.Instant; } break;
                case ExtraEffect.ActiveRole: { result = DurationType.Instant; } break;
                case ExtraEffect.Cuff: { result = DurationType.DayEnd; } break;
                case ExtraEffect.AirPlane: { result = DurationType.Instant; } break;
                case ExtraEffect.MineDetector: { result = DurationType.Instant; } break;
                case ExtraEffect.Grenade: { result = DurationType.Instant; } break;
                case ExtraEffect.Lightning: { result = DurationType.Instant; } break;
                case ExtraEffect.Explosion: { result = DurationType.Instant; } break;
                case ExtraEffect.Mine: { result = DurationType.Instant; } break;
                case ExtraEffect.Cape: { result = DurationType.Instant; } break;
                case ExtraEffect.Dublicat: { result = DurationType.Instant; } break;
                case ExtraEffect.Gun: { result = DurationType.Instant; } break;
                case ExtraEffect.BraceKnukles: { result = DurationType.Instant; } break;
                case ExtraEffect.MagicCube: { result = DurationType.DayEnd; } break;
                case ExtraEffect.FoolsCap: { result = DurationType.Instant; } break;
                case ExtraEffect.PopCorn: { result = DurationType.Instant; } break;
                case ExtraEffect.Keys: { result = DurationType.Instant; } break;
                case ExtraEffect.Cola: { result = DurationType.DayEnd; } break;
                case ExtraEffect.Mirror: { result = DurationType.EndPhase; } break;
                case ExtraEffect.Dart: { result = DurationType.NightStart; } break;
                case ExtraEffect.Kinder: { result = DurationType.Instant; } break;
                case ExtraEffect.Fizz: { result = DurationType.DayEnd; } break;
                case ExtraEffect.CosmoRait: { result = DurationType.Instant; } break;
                case ExtraEffect.Hidrogen: { result = DurationType.Game; } break;
                case ExtraEffect.Star: { result = DurationType.Instant; } break;
                case ExtraEffect.Asteroid: { result = DurationType.Instant; } break;
                case ExtraEffect.Comet: { result = DurationType.Instant; } break;

                case ExtraEffect.Orbit: { result = DurationType.Instant; } break;
                case ExtraEffect.Spacesuit: { result = DurationType.DayEnd; } break;
                case ExtraEffect.SpiderPaw: { result = DurationType.Instant; } break;
                case ExtraEffect.Trap: { result = DurationType.Instant; } break;
                case ExtraEffect.Web: { result = DurationType.NightEnd; } break;
                case ExtraEffect.Cocoon: { result = DurationType.NightEnd; } break;
                case ExtraEffect.Scarab: { result = DurationType.DayStart; } break;
                case ExtraEffect.Toxin: { result = DurationType.Instant; } break;
                case ExtraEffect.Hook: { result = DurationType.Instant; } break;
                case ExtraEffect.Skull: { result = DurationType.Instant; } break;
                case ExtraEffect.PirateBomb: { result = DurationType.Instant; } break;
                case ExtraEffect.Saber: { result = DurationType.Instant; } break;
                case ExtraEffect.Tracker: { result = DurationType.NightEnd; } break;
                case ExtraEffect.Botle: { result = DurationType.Instant; } break;
                case ExtraEffect.PirateBox: { result = DurationType.Instant; } break;
                case ExtraEffect.NewYearToy: { result = DurationType.Instant; } break;
                case ExtraEffect.SnowBall: { result = DurationType.Instant; } break;
                case ExtraEffect.Pumpkin: { result = DurationType.Instant; } break;
                case ExtraEffect.Mask: { result = DurationType.Instant; } break;
                case ExtraEffect.Lili: { result = DurationType.Instant; } break;
                case ExtraEffect.Shovel: { result = DurationType.Instant; } break;
                default: { } break;
            }

            return result;
        }

        public static string GetRoleNameById_Rus(string roleIdString)
        {
            var roleId = (RoleType)GetEnumElement<RoleType>(roleIdString);

            return GetRoleNameById_Rus(roleId);
        }
        public static string GetRoleNameById_Rus(RoleType roleId)
        {
            var result = "";

            switch (roleId)
            {
                case RoleType.Citizen: { result = $"Гражданин"; } break;
                case RoleType.Doctor: { result = $"Доктор"; } break;               
                case RoleType.Guerilla: { result = $"Партизан"; } break;
                case RoleType.Saint: { result = $"Святой"; } break;
                case RoleType.Witness: { result = $"Свидетель"; } break;
                case RoleType.Commissar: { result = $"Комиссар"; } break;

                case RoleType.Mafia: { result = $"Мафиози"; } break;
                case RoleType.MafiaBoss: { result = $"Босс мафии"; } break;
                case RoleType.Maniac: { result = $"Маньяк"; } break;                
                case RoleType.Sinner: { result = $"Грешник"; } break;
                case RoleType.Werewolf: { result = $"Оборотень"; } break;             

                case RoleType.Jane: { result = $"Джейн"; } break;
                case RoleType.Loki: { result = $"Локи"; } break;
                case RoleType.Garry: { result = $"Гарри"; } break;

                case RoleType.Alien: { result = $"Пришелец"; } break;
                case RoleType.Astronaut: { result = $"Космонавт"; } break;

                case RoleType.GoodClown: { result = $"Добрый Клоун"; } break;
                case RoleType.BadClown: { result = $"Злой Клоун"; } break;

                case RoleType.Scientist: { result = $"Ученый"; } break;

                case RoleType.NULL: { result = $"Общие"; } break;

                default: { result = $"роль {roleId}"; } break;
            }

            return result;
        }

        public static RoleType GetRoleTypeByName_Rus(string name)
        {
            var result = RoleType.Citizen;

            switch (name)
            {
                case "Гражданин": { result = RoleType.Citizen; } break;
                case "Доктор": { result = RoleType.Doctor; } break;
                case "Партизан": { result = RoleType.Guerilla; } break;
                case "Святой": { result = RoleType.Saint; } break;
                case "Свидетель": { result = RoleType.Witness; } break;
                case "Комиссар": { result = RoleType.Commissar; } break;

                case "Мафиози": { result = RoleType.Mafia; } break;
                case "Босс мафии": { result = RoleType.MafiaBoss; } break;
                case "Маньяк": { result = RoleType.Maniac; } break;
                case "Грешник": { result = RoleType.Sinner; } break;
                case "Оборотень": { result = RoleType.Werewolf; } break;

                case "Джейн": { result = RoleType.Jane; } break;
                case "Локи": { result = RoleType.Loki; } break;
                case "Гарри": { result = RoleType.Garry; } break;

                //злой клоун
                //добрый клоун

                //космонавт
                //пришелец

                //ученый

                default: { result = RoleType.Citizen; } break;
            }

            return result;
        }
    }

    
}
