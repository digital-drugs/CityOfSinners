using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mafia_Server
{
    public class Skill
    {
        public int level;
        public SkillEffect effect { get; private set; }

        public Skill(SkillEffect effect) 
        {
            this.effect = effect;
        }

        public Skill(SkillEffect effect, int value)
        {
            this.effect = effect;

            SetSkillValue(value);
        }

        public int skillValue { get; private set; }
        public void SetSkillValue(int skillValue)
        {
            this.skillValue = skillValue;
        }

        public string GetColoredName()
        {
            var skillName = SkillManager.GetSkillNameById(effect);
            //var skillString = ColorString.GetColoredString(skillName, ColorId.Skill);

            //var nameRus = Helper.GetSkillNameById_Rus(effect);

            var result = ColorString.GetColoredString(skillName, ColorId.Skill);

            return result;
        }
    }
}
