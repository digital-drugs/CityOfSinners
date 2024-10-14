using System;

namespace Share
{
    public class Effect
    {
        public long effectOwner;
        //public EffectType effectType;

        //public Effect(RoleEffect roleEffect)
        //{
        //    effectType = EffectType.Role;
        //}

        //public Effect(SkillEffect skillEffect)
        //{
        //    effectType = EffectType.Skill;
        //}
    }

    public class Extra_Effect : Effect
    {
        public ExtraEffect effect;

        public Extra_Effect(ExtraEffect effect)
        {
            //effectType = EffectType.Extra;

            this.effect = effect;
        }
    }

    public class Role_Effect : Effect
    {
        public RoleEffect effect;

        public Role_Effect()
        {

        }

        public Role_Effect(RoleEffect effect)
        {
            //effectType = EffectType.Extra;

            this.effect = effect;
        }
    }

    public class Skill_Effect : Effect
    {
        public SkillEffect effect;

        public Skill_Effect(SkillEffect effect)
        {
            //effectType = EffectType.Extra;

            this.effect = effect;
        }
    }

    public enum EffectType
    {
        Role,
        Skill,
        Extra,
    }

    public enum RoleEffect
    {
        Mafia,
        Commissar,
        Doctor,
        Witness,
        Guerilla,
        MafiaBoss,
        Maniac,
        Saint,
        Sinner,
        Werewolf,
        JaneCharm,
        JaneKill,
    }

    public enum SkillEffect
    {
        //гражданин
        /// <summary>
        /// живучесть
        /// </summary>
        CitizenVitality,
        /// <summary>
        /// раскрыть тайну
        /// </summary>
        CitizenSecret,
        /// <summary>
        /// +1
        /// </summary>
        CitizenOneMore,
        /// <summary>
        /// оборотень уходи
        /// </summary>
        CitizenWerewolf,
        /// <summary>
        /// маньяк стоять
        /// </summary>
        CitizenManiac,

        //мафиози
        /// <summary>
        /// резкий дерзкий
        /// </summary>
        MafiaBold,
        /// <summary>
        /// x2 убийство
        /// </summary>
        MafiaKillX2,
        /// <summary>
        /// бегу на волю
        /// </summary>
        MafiaToFreedom,
        /// <summary>
        /// хамелеон
        /// </summary>
        MafiaChameleon,
        /// <summary>
        /// месть маньяку
        /// </summary>
        MafiaManiac,


        //босс мафии
        /// <summary>
        /// упс
        /// </summary>
        MafiaBossUps,
        /// <summary>
        /// качок
        /// </summary>
        MafiaBossBodyBuilder,
        /// <summary>
        /// перехват
        /// </summary>
        MafiaBossInterception,
        /// <summary>
        /// +1
        /// </summary>
        MafiaBossOneMore,
        /// <summary>
        /// опытный босс
        /// </summary>
        MafiaBossExperienced,

        //грешник
        /// <summary>
        /// учусь
        /// </summary>
        SinnerStudy,
        /// <summary>
        /// я не сдаюсь
        /// </summary>
        SinnerNotGiveUp,
        /// <summary>
        /// замена
        /// </summary>
        SinnerChange,
        /// <summary>
        /// туман
        /// </summary>
        SinnerMist,
        /// <summary>
        /// +1
        /// </summary>
        SinnerOneMore,

        //святой
        /// <summary>
        /// стойкий
        /// </summary>
        SaintResistant,
        /// <summary>
        /// не в духе
        /// </summary>
        SaintBadMood,
        /// <summary>
        /// повезло
        /// </summary>
        SaintLucky,
        /// <summary>
        /// оберег
        /// </summary>
        SaintGuard,
        /// <summary>
        /// власть
        /// </summary>
        SaintPower,

        //оборотень
        /// <summary>
        /// безлимит
        /// </summary>
        WerewolfUnlimit,
        /// <summary>
        /// гром
        /// </summary>
        WerewolfThunder,
        /// <summary>
        /// +1
        /// </summary>
        WerewolfOneMore,
        /// <summary>
        /// меняюсь
        /// </summary>
        WerewolfChange,
        /// <summary>
        /// ярость
        /// </summary>
        WerewolfRage,

        //маньяк
        /// <summary>
        /// я идеальный
        /// </summary>
        ManiacIdeal,
        /// <summary>
        /// +1
        /// </summary>
        ManiacOneMore,
        /// <summary>
        /// чем я хуже
        /// </summary>
        ManiacWorse,
        /// <summary>
        /// песня маньяка
        /// </summary>
        ManiacSong,
        /// <summary>
        /// бегу на волю
        /// </summary>
        ManiacToFreedom,

        //свидетель
        /// <summary>
        /// без помех
        /// </summary>
        WitnessNoInterference,
        /// <summary>
        /// карма
        /// </summary>
        WitnessCarma,
        /// <summary>
        /// друзья
        /// </summary>
        WitnessFriends,
        /// <summary>
        /// сила воли
        /// </summary>
        WitnessWillPower,
        /// <summary>
        /// +1
        /// </summary>
        WitnessOneMore,

        //доктор
        /// <summary>
        /// клизма на убой
        /// </summary>
        DoctorClizma,
        /// <summary>
        /// х2
        /// </summary>
        DoctorHealX2,
        /// <summary>
        /// бешеный док
        /// </summary>
        DoctorCrazyDoc,
        /// <summary>
        /// злой скальпель
        /// </summary>
        DoctorEvilScalpel,
        /// <summary>
        /// где ты
        /// </summary>
        DoctorWhereAreYou,

        //коммиссар
        /// <summary>
        /// злость
        /// </summary>
        CommissarRage,
        /// <summary>
        /// +1
        /// </summary>
        CommissarOneMore,
        /// <summary>
        /// сквозь обман
        /// </summary>
        CommissarThroughLie,
        /// <summary>
        /// сдай колег
        /// </summary>
        CommissarMafiaColleagues,
        /// <summary>
        /// дар
        /// </summary>
        CommissarGift,

        //партизан
        /// <summary>
        /// лед
        /// </summary>
        GuerillaIce,
        /// <summary>
        /// я свой
        /// </summary>
        GuerillaOwn,
        /// <summary>
        /// долгий эффект
        /// </summary>
        GuerillaLongEffect,
        /// <summary>
        /// ну все
        /// </summary>
        GuerillaThatsAll,
        /// <summary>
        /// награда за разговоры
        /// </summary>
        GuerillaReward,

        //общие скиллы
        /// <summary>
        /// красиво надолго
        /// </summary>
        ShareSkillNiceAndLong,
        /// <summary>
        /// х2
        /// </summary>
        ShareSkillVoteX2,
        /// <summary>
        /// антиболтун
        /// </summary>
        ShareSkillAntiTalk,
        /// <summary>
        /// бесплатно
        /// </summary>
        ShareSkillFree,
        /// <summary>
        /// +1000
        /// </summary>
        ShareSkillRating,
    }
}
