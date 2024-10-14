using ExitGames.Client.Photon.LoadBalancing;
using Mafia_Server.Extras;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public static class SkillHelper
    {
        public static void AddSkillEffectToPlayer(BasePlayer player, Skill skill, DurationType endPhase, int endDay=0)
        {
            player.playerRole.roleEffects.AddSkillEffect(skill);
           
            var newSkillHandler = new SkillHandler(player, skill, player.GetRoom());

            var dayCount = 0;

            if (endDay > 0)
            {
                dayCount = player.GetRoom().roomPhases.dayCount + endDay;
            }

            newSkillHandler.Subscribe(endPhase, dayCount);

            player.playerRole.roleEffects.AddSkillHandler(newSkillHandler);
        }

        //private BasePlayer GetExtraTarget(BasePlayer player, Dictionary<byte, object> extraData)
        //{
        //    var extraTargetId = (long)extraData[(byte)Params.UserId];

        //    return GetExtraTarget(player, extraTargetId);
        //}

        //private BasePlayer GetExtraTarget(BasePlayer player, long id)
        //{
        //    var extraTargetPlayer = room.roomLogic.activePlayers[id];

        //    var mirrorExtra = extraTargetPlayer.FindExtraEffect(ExtraEffect.Mirror);
        //    if (mirrorExtra != null)
        //    {
        //        return player;
        //    }

        //    return extraTargetPlayer;
        //}

    }
}
