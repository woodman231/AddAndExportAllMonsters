using System;
using System.Collections.Generic;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static List<object> GetMonsterAppearances(Monster monster, IEnumerable<MapData> mapDatas)
        {
            var results = new List<object>();

            foreach (var mapData in mapDatas)
            {
                var mapDataResults = new Dictionary<string, object>();

                mapDataResults["SceneName"] = mapData.SceneName;

                var mapArea = mapData.MapArea.GetComponent<MapArea>();

                if (mapArea is not null)
                {
                    mapDataResults["MapAreaName"] = mapArea.GetName();
                }

                foreach (var areaMonster in mapArea.Monsters)
                {
                    var areaMonsterAsMonster = (Monster)areaMonster.gameObject.GetComponent<Monster>();
                    if (areaMonsterAsMonster.ID == monster.ID)
                    {
                        results.Add(mapDataResults);
                    }
                }

            }

            return results;
        }

        public static Dictionary<string, object> GetMonsterProperties(Monster monster, IEnumerable<Type> targetSkillTypes, IEnumerable<Type> targetItemTypes, IEnumerable<MapData> mapDatas)
        {
            var results = new Dictionary<string, object>();

            results["InstanceID"] = monster.GetInstanceID();
            results["ID"] = monster.ID;
            results["JournalNumber"] = GameController.Instance.GetMonsterJournalIndex(monster);
            results["Name"] = monster.OriginalMonsterName;
            results["TypesName"] = monster.GetMonsterTypeString();
            results["TypesArray"] = monster.MonsterTypes;
            results["IsSpectralFamiliar"] = monster.IsSpectralFamiliar;

            results["ExploreAbilityName"] = monster.ExploreActionExploreAbility.GetName();
            results["ExploreAbilityDescription"] = StripColorCodes(StripNewLine(monster.ExploreActionExploreAbility.GetDescription()));
            results["HasSonarAbility"] = monster.HasSonarAbility();

            results["BaseStats"] = monster.baseStats;
            results["LightShiftStats"] = monster.lightShift;
            results["DarkShiftStats"] = monster.darkShift;

            var lightPassive = monster.SkillManager.GetLightPassive();
            results["LightShiftPassive"] = ExportUtilities.GetSkillProperties(lightPassive.gameObject, monster, targetSkillTypes);

            var darkPassive = monster.SkillManager.GetDarkPassive();
            results["DarkShiftPassive"] = ExportUtilities.GetSkillProperties(darkPassive.gameObject, monster, targetSkillTypes);

            results["BaseSkills"] = ExportUtilities.GetBaseSkillsProperty(monster.SkillManager.BaseSkills, monster, targetSkillTypes);
            results["SkillTrees"] = ExportUtilities.GetSkillTreesProperty(monster.SkillManager.SkillTrees, monster, targetSkillTypes);
            results["Ultimates"] = ExportUtilities.GetBaseSkillsProperty(monster.SkillManager.Ultimates, monster, targetSkillTypes);

            results["GoldReward"] = monster.GetGoldReward();
            results["RewardsCommon"] = ExportUtilities.GetItemsProperty(monster.RewardsCommon, targetItemTypes);
            results["RewardsRare"] = ExportUtilities.GetItemsProperty(monster.RewardsRare, targetItemTypes);
            results["NoRareRewards"] = monster.NoRareRewards;

            results["Appearances"] = ExportUtilities.GetMonsterAppearances(monster, mapDatas);

            return results;
        }
    }
}
