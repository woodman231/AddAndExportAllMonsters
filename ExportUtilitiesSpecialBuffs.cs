using System;
using System.Collections.Generic;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static Dictionary<string, object> GetSpecialBuffProperties(BuffManager.ESpecialBuff specialBuffType, List<Monster> monsters)
        {
            // I tried using an instance of BuffManager to get these properties but it didn't seem to work. It looks like the only way I could really get them is to get the
            // OverlaySpecialBuff properties from a PassiveSkill from a built monster that has that property

            string targetEnumName = Enum.GetName(typeof(BuffManager.ESpecialBuff), specialBuffType);

            var results = new Dictionary<string, object>();

            for (var monsterIndex = 0; monsterIndex < monsters.Count - 1; monsterIndex++)
            {
                var thisMonster = monsters[monsterIndex];
                var thisMonstersSkillTrees = thisMonster.SkillManager.SkillTrees;

                for (var skillTreeIndex = 0; skillTreeIndex < thisMonstersSkillTrees.Length; skillTreeIndex++)
                {
                    var skillTree = thisMonstersSkillTrees[skillTreeIndex];

                    for (var tierIndex = 0; tierIndex < SkillTree.TierCount; tierIndex++)
                    {
                        var skillsInTreeTier = skillTree.GetSkillGOsByTier(tierIndex);

                        for (var skillIndex = 0; skillIndex < skillsInTreeTier.Count - 1; skillIndex++)
                        {
                            var skill = skillsInTreeTier[skillIndex];
                            var skillAsPassiveSkill = skill.gameObject.GetComponent<PassiveSkill>();

                            if (skillAsPassiveSkill is not null)
                            {
                                var overlaySpecialBuff = skillAsPassiveSkill.GetOverlaySpecialBuff(thisMonster.BuffManager);
                                if (overlaySpecialBuff is not null)
                                {
                                    if (overlaySpecialBuff.Name == targetEnumName)
                                    {
                                        results["Name"] = overlaySpecialBuff.Name;
                                        results["Value"] = (int)specialBuffType;
                                        results["Icon"] = overlaySpecialBuff.Icon;
                                        results["Description"] = StripCurlyBraces(overlaySpecialBuff.Description);
                                        results["IsNegative"] = overlaySpecialBuff.IsNegative;

                                        return results;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }
    }
}
