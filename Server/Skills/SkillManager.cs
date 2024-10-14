using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class SkillManager
    {
        public SkillManager()
        {
            LoadSkills();
            //CreateSkills();
        }

        public static Dictionary<string, object> rawSkillData;
        //public static Dictionary<string, Skill> skills { get; private set; } = new Dictionary<string, Skill>();
        
        private void LoadSkills()
        {
            DBManager.Inst.OnSkillsChange += OnSkillsChange;
            
            rawSkillData = DBManager.Inst.LoadSkills();   
        }

        public static string GetSkillNameById(SkillEffect skillId)
        {
            return GetSkillNameById(skillId.ToString());
        }

        public static string GetSkillNameById(string skillId)
        {
            var result = "";

            if(rawSkillData.ContainsKey(skillId))
            {
                var skillData = (Dictionary<byte,object>) rawSkillData[skillId];

                result = (string)skillData[(byte)Params.SkillName];
            }

            return result;
        }

        public static Dictionary<string, object> LoadSkillsData()
        {
            return DBManager.Inst.LoadSkills();
        }

        private void OnSkillsChange(object sender, EventArgs e)
        {
            rawSkillData = DBManager.Inst.LoadSkills();

            Logger.Log.Debug($"skills changed");
        }

        //private void CreateSkills()
        //{           
        //    CreateCitizenSkills();
        //    CreateMafiaSkills();
        //    CreateMafiaBossSkills();
        //    CreateSinnerSkills();
        //    CreateSaintSkills();
        //    CreateWerewolfSkills();
        //    CreateManiacSkills();
        //    CreateWitnessSkills();
        //    CreateDoctorSkills();
        //    CreateCommissarSkills();
        //    CreateGuerillaSkills();
        //    CreateShareSkills();
        //}

        //public static Dictionary<int, object> GetSkillsData()
        //{
        //    var result =    new Dictionary<int, object>();

        //    var skillCount = 0;

        //    foreach(var s in skills)
        //    {
        //        var skillData = new Dictionary<byte, object>();

        //        skillData.Add((byte)Params.SkillType, s.Key);
        //        skillData.Add((byte)Params.SkillLevel, 0);

        //        result.Add(skillCount++, skillData);
        //    }

        //    return result;
        //} 

        private void AddSkill(EffectType effectType)
        {
            //var skill = new Skill(effectType);
            //skills.Add(effectType, skill);
        }

        //private void CreateCitizenSkills()
        //{
        //    AddSkill(EffectType.CitizenVitality);
        //    AddSkill(EffectType.CitizenSecret);
        //    AddSkill(EffectType.CitizenOneMore);
        //    AddSkill(EffectType.CitizenWerewolf);
        //    AddSkill(EffectType.CitizenManiac);
        //}

        //private void CreateMafiaSkills()
        //{
        //    AddSkill(EffectType.MafiaBold);
        //    AddSkill(EffectType.MafiaKillX2);
        //    AddSkill(EffectType.MafiaToFreedom);
        //    AddSkill(EffectType.MafiaChameleon);
        //    AddSkill(EffectType.MafiaManiac);
        //}


        //private void CreateMafiaBossSkills()
        //{
        //    AddSkill(EffectType.MafiaBossUps);
        //    AddSkill(EffectType.MafiaBossBodyBuilder);
        //    AddSkill(EffectType.MafiaBossInterception);
        //    AddSkill(EffectType.MafiaBossOneMore);
        //    AddSkill(EffectType.MafiaBossExperienced);
        //}

        //private void CreateSinnerSkills()
        //{
        //    AddSkill(EffectType.SinnerStudy);
        //    AddSkill(EffectType.SinnerNotGiveUp);
        //    AddSkill(EffectType.SinnerChange);
        //    AddSkill(EffectType.SinnerMist);
        //    AddSkill(EffectType.SinnerOneMore);
        //}

        //private void CreateSaintSkills()
        //{
        //    AddSkill(EffectType.SaintResistant);
        //    AddSkill(EffectType.SaintBadMood);
        //    AddSkill(EffectType.SaintLucky);
        //    AddSkill(EffectType.SaintGuard);
        //    AddSkill(EffectType.SaintPower);
        //}

        //private void CreateWerewolfSkills()
        //{
        //    AddSkill(EffectType.WerewolfUnlimit);
        //    AddSkill(EffectType.WerewolfThunder);
        //    AddSkill(EffectType.WerewolfOneMore);
        //    AddSkill(EffectType.WerewolfChange);
        //    AddSkill(EffectType.WerewolfRage);
        //}

        //private void CreateManiacSkills()
        //{
        //    AddSkill(EffectType.ManiacIdeal);
        //    AddSkill(EffectType.ManiacOneMore);
        //    AddSkill(EffectType.ManiacWorse);
        //    AddSkill(EffectType.ManiacSong);
        //    AddSkill(EffectType.ManiacToFreedom);
        //}

        //private void CreateWitnessSkills()
        //{
        //    AddSkill(EffectType.WitnessNoInterference);
        //    AddSkill(EffectType.WitnessCarma);
        //    AddSkill(EffectType.WitnessFriends);
        //    AddSkill(EffectType.WitnessWillPower);
        //    AddSkill(EffectType.WitnessOneMore);
        //}

        //private void CreateDoctorSkills()
        //{
        //    AddSkill(EffectType.DoctorClizma);
        //    AddSkill(EffectType.DoctorHealX2);
        //    AddSkill(EffectType.DoctorCrazyDoc);
        //    AddSkill(EffectType.DoctorEvilScalpel);
        //    AddSkill(EffectType.DoctorWhereAreYou);
        //}

        //private void CreateCommissarSkills()
        //{
        //    AddSkill(EffectType.CommissarRage);
        //    AddSkill(EffectType.CommissarOneMore);
        //    AddSkill(EffectType.CommissarThroughLie);
        //    AddSkill(EffectType.CommissarMafiaColleagues);
        //    AddSkill(EffectType.CommissarGift);
        //}

        //private void CreateGuerillaSkills()
        //{
        //    AddSkill(EffectType.GuerillaIce);
        //    AddSkill(EffectType.GuerillaOwn);
        //    AddSkill(EffectType.GuerillaLongEffect);
        //    AddSkill(EffectType.GuerillaThatsAll);
        //    AddSkill(EffectType.GuerillaReward);
        //}

        //private void CreateShareSkills()
        //{
        //    AddSkill(EffectType.ShareSkillNiceAndLong);
        //    AddSkill(EffectType.ShareSkillVoteX2);
        //    AddSkill(EffectType.ShareSkillHide);
        //    AddSkill(EffectType.ShareSkillFree);
        //    AddSkill(EffectType.ShareSkillRating);
        //}
    }
}
