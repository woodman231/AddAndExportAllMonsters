using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static List<Type> GetSkillTargetTypes()
        {
            Plugin.Log.LogInfo("GetSkillTargetTypes called");

            var results = new List<Type>();

            results.Add(typeof(BaseSkill));
            results.Add(typeof(BaseAction));
            results.Add(typeof(ActionModifier));

            results.Add(typeof(ActionBuff));
            results.Add(typeof(ActionConditionalDamage));
            results.Add(typeof(ActionDamage));
            results.Add(typeof(ActionDebuff));
            results.Add(typeof(ActionDebuffCataclyst));
            results.Add(typeof(ActionDebuffOnHit));
            results.Add(typeof(ActionHeal));
            results.Add(typeof(ActionRedirect));
            results.Add(typeof(ActionRemoveBuff));
            results.Add(typeof(ActionRemoveDebuff));
            results.Add(typeof(ActionRemoveStacks));
            results.Add(typeof(ActionRevive));
            results.Add(typeof(ActionShield));
            results.Add(typeof(ActionShieldBurst));
            results.Add(typeof(ActionSpecialBuff));
            results.Add(typeof(ActionTypeRestriction));

            results.Add(typeof(PassiveSkill));

            var passiveSkillAssembly = typeof(PassiveSkill).Assembly;
            var passiveSkillAssemblyTypes = passiveSkillAssembly.GetTypes();

            var passiveSkillTypes = passiveSkillAssemblyTypes.Where(w => w.BaseType == typeof(PassiveSkill)).ToList();

            foreach (var passiveSkillType in passiveSkillTypes)
            {
                results.Add(passiveSkillType);
            }

            return results;
        }

        public static Dictionary<string, object> GetSkillProperties(GameObject skill, Monster monster, IEnumerable<Type> targetTypes)
        {
            Plugin.Log.LogInfo($"exporting skill {skill.name} for {monster.Name}");
            var results = new Dictionary<string, object>();

            foreach (var targetType in targetTypes)
            {
                if (targetType == typeof(BaseSkill))
                {
                    var skillAsTargetType = (BaseSkill)skill?.gameObject?.GetComponent(targetType);

                    if (skillAsTargetType is not null)
                    {
                        results["ID"] = skillAsTargetType.ID;
                        results["Name"] = skillAsTargetType.GetFullName();
                        results["Description"] = StripColorCodes(StripNewLine(skillAsTargetType.GetTooltip(monster)));
                    }
                }
                else
                {
                    var skillAsTargetType = skill?.gameObject?.GetComponent(targetType);

                    if (skillAsTargetType is not null)
                    {
                        results[$"Is{targetType.Name}"] = true;

                        var targetTypeResults = new Dictionary<string, object>();

                        if (targetType == typeof(PassiveSkill))
                        {
                            var typeAsPassiveSkill = (PassiveSkill)skillAsTargetType;

                            var overlayBuffs = typeAsPassiveSkill.GetOverlayBuffs();
                            if (overlayBuffs is not null)
                            {
                                targetTypeResults["OverlayBuffs"] = overlayBuffs;
                            }

                            var overlayDebuffs = typeAsPassiveSkill.GetOverlayDebuffs();
                            if (overlayDebuffs is not null)
                            {
                                targetTypeResults["OverlayDebuffs"] = overlayDebuffs;
                            }

                            if (monster.BuffManager is not null)
                            {
                                Plugin.Log.LogInfo("monster.BuffManager is not null");

                                var overlaySpecialBuff = typeAsPassiveSkill.GetOverlaySpecialBuff(monster.BuffManager);
                                if (overlaySpecialBuff is not null)
                                {
                                    // overlaySpecialBuff.BuffType always returns 0, but the other properties like Name and Icon seem to be ok.
                                    // either a bug in the game or just a wierd effect of how this export is being made.
                                    int realTypeValue = -1;

                                    var values = Enum.GetValues(typeof(BuffManager.ESpecialBuff));

                                    foreach (var enumValue in values)
                                    {
                                        var enumName = Enum.GetName(typeof(BuffManager.ESpecialBuff), enumValue);
                                        if (enumName.ToLower() == overlaySpecialBuff.Name.ToLower())
                                        {
                                            realTypeValue = (int)enumValue;
                                        }
                                    }

                                    var overlaySpecialBuffProperties = new Dictionary<string, object>();

                                    overlaySpecialBuffProperties["BuffType"] = realTypeValue;
                                    overlaySpecialBuffProperties["Name"] = overlaySpecialBuff.Name;
                                    overlaySpecialBuffProperties["Icon"] = overlaySpecialBuff.Icon;
                                    overlaySpecialBuffProperties["Description"] = StripCurlyBraces(overlaySpecialBuff.Description);
                                    overlaySpecialBuffProperties["IsNegative"] = overlaySpecialBuff.IsNegative;

                                    targetTypeResults["OverlaySpecialBuff"] = overlaySpecialBuffProperties;
                                }
                            }
                            else
                            {
                                Plugin.Log.LogInfo("monster.BuffManager is null");
                            }
                        }

                        if (targetType.BaseType == typeof(PassiveSkill))
                        {
                            foreach (var method in targetType.GetMethods())
                            {
                                if (method.DeclaringType == targetType)
                                {
                                    if (method.Name.StartsWith("On"))
                                    {
                                        targetTypeResults[$"Is{method.Name}"] = true;
                                    }
                                }
                            }
                        }

                        var fields = targetType.GetFields();

                        foreach (var field in fields)
                        {
                            if (field.DeclaringType == targetType)
                            {
                                object valueToSet = null;

                                if (field.FieldType == typeof(GameObject) || field.FieldType.BaseType == typeof(GameObject))
                                {
                                    valueToSet = GetReferenceableIDFromGameObject((GameObject)field.GetValue(skillAsTargetType));
                                }
                                else if (field.FieldType == typeof(List<GameObject>) || field.FieldType.BaseType == typeof(List<GameObject>))
                                {
                                    valueToSet = GetReferenceableIDsFromGameObjects((List<GameObject>)field.GetValue(skillAsTargetType));
                                }
                                else if (field.FieldType == typeof(string))
                                {
                                    valueToSet = StripColorCodes(StripNewLine((string)field.GetValue(skillAsTargetType)));
                                }
                                else if (field.FieldType == typeof(UnityEngine.Vector3))
                                {
                                    valueToSet = null;
                                }
                                else if (field.FieldType == typeof(BaseAction))
                                {
                                    var fieldValueAsBaseAction = (BaseAction)field.GetValue(skillAsTargetType);

                                    if (fieldValueAsBaseAction is not null)
                                    {
                                        valueToSet = fieldValueAsBaseAction.ID;
                                    }
                                }
                                else
                                {
                                    valueToSet = field.GetValue(skillAsTargetType);
                                }

                                targetTypeResults[field.Name] = valueToSet;
                            }
                        }

                        results[$"{targetType.Name}Properties"] = targetTypeResults;
                    }
                }
            }

            Plugin.Log.LogInfo($"done exporting skill {skill.name} for {monster.Name}");

            return results;
        }

        public static Dictionary<string, object> GetSkillTreeProperties(SkillTree skillTree, Monster monster, IEnumerable<Type> targetTypes)
        {
            var results = new Dictionary<string, object>();

            Plugin.Log.LogInfo($"Exporting Skill Tree {skillTree.GetInstanceID()} for {monster.Name}");

            for (var tier = 0; tier < SkillTree.TierCount; tier++)
            {
                var skillsInTier = skillTree.GetSkillGOsByTier(tier);
                var skillsToExport = new List<object>();

                Plugin.Log.LogInfo($"Exprting Skill Tree {skillTree.GetInstanceID()} tier {tier + 1} for {monster.Name}");

                foreach (var skill in skillsInTier)
                {
                    var thisSkillDetails = GetSkillProperties(skill, monster, targetTypes);

                    skillsToExport.Add(thisSkillDetails);
                }

                results[$"Tier{tier + 1}Skills"] = skillsToExport;
            }

            Plugin.Log.LogInfo($"Done Exporting SkillTreeProperties for {monster.Name}");

            return results;
        }

        public static List<object> GetSkillTreesProperty(IEnumerable<SkillTree> skilltrees, Monster monster, IEnumerable<Type> targetTypes)
        {
            var results = new List<object>();

            Plugin.Log.LogInfo($"Exporting skill trees for {monster.Name}");

            foreach (var skillTree in skilltrees)
            {
                results.Add(GetSkillTreeProperties(skillTree, monster, targetTypes));
            }

            Plugin.Log.LogInfo($"Done setting skilltrees property for {monster.Name}");

            return results;
        }

        public static List<object> GetBaseSkillsProperty(IEnumerable<GameObject> baseSkills, Monster monster, IEnumerable<Type> targetTypes)
        {
            var results = new List<object>();

            foreach (var baseSkill in baseSkills)
            {
                var skillProperties = GetSkillProperties(baseSkill, monster, targetTypes);

                results.Add(skillProperties);
            }

            return results;
        }
    }
}
