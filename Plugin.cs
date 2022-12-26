using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AddAndExportAllMonsters
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal static List<Type> TargetSkillTypes;
        internal static List<Type> TargetItemTypes;

        private void Awake()
        {
            Log = base.Logger;

            TargetSkillTypes = ExportUtilities.GetSkillTargetTypes();
            TargetItemTypes = ExportUtilities.GetBaseItemTypes();

            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            Log.LogInfo("Patches applied!");
        }

        [HarmonyPatch(typeof(MonsterManager), nameof(MonsterManager.LoadGame), new Type[] { typeof(SaveGameData), typeof(bool) })]
        private class MonsterManagerLoadGamePatch
        {
            [UsedImplicitly]
            private static void Postfix(MonsterManager __instance, ref SaveGameData saveGameData, ref bool newGamePlusSetup)
            {
                // This will add all of the monters, in all shifts, but only if the save game file only has one spectral wolf in it.
                Plugin.Log.LogInfo("MonsterManager.LoadGame detected");

                int monstersCount = __instance.TotalCount;
                if (monstersCount == 1)
                {
                    bool firstMonsterIsSpectralWolf = false;

                    var firstMonster = __instance.AllMonster[0];
                    if (firstMonster.IsSpectralFamiliar && firstMonster.GetName() == "Wolf")
                    {
                        firstMonsterIsSpectralWolf = true;
                    }

                    if (firstMonsterIsSpectralWolf)
                    {
                        firstMonster.NameMonster("Spectral Wolf Normal");
                        firstMonster.Equipment.UnequipAll();
                        firstMonster.SetLevel(42);
                        firstMonster.SkillManager.ResetSkills();

                        try
                        {
                            // First add the light and dark shifted versions of the spectral wolf.
                            var spectralWolfMonster = GameController.Instance.WorldData.Referenceables.Where(w => w?.ID == 228).Select(s => s.GetComponent<Monster>()).FirstOrDefault();

                            if (spectralWolfMonster is not null)
                            {
                                var addedSpectralWolfLightMonster = __instance.AddMonsterByPrefab(spectralWolfMonster.gameObject, EShift.Light);
                                addedSpectralWolfLightMonster.NameMonster("Spectral Wolf Light");
                                addedSpectralWolfLightMonster.Equipment.UnequipAll();
                                addedSpectralWolfLightMonster.SetLevel(42);
                                addedSpectralWolfLightMonster.SkillManager.ResetSkills();

                                var addedSpectralWolfDarkMonster = __instance.AddMonsterByPrefab(spectralWolfMonster.gameObject, EShift.Dark);
                                addedSpectralWolfDarkMonster.NameMonster("Spectral Wolf Dark");
                                addedSpectralWolfDarkMonster.Equipment.UnequipAll();
                                addedSpectralWolfDarkMonster.SetLevel(42);
                                addedSpectralWolfDarkMonster.SkillManager.ResetSkills();
                            }

                            // Now add all of the other monsters with a light or dark shift
                            var monsters = GameController.Instance.MonsterJournalList.Where(w => w?.GetComponent<Monster>() != null && w?.GetComponent<Monster>().ID != 228).Select(s => s.GetComponent<Monster>());
                            foreach (var monster in monsters)
                            {
                                foreach (EShift monsterShift in Enum.GetValues(typeof(EShift)))
                                {
                                    try
                                    {
                                        var addedMonster = __instance.AddMonsterByPrefab(monster.gameObject, monsterShift);
                                        addedMonster.NameMonster($"{monster.Name} {monsterShift}");
                                        addedMonster.Equipment.UnequipAll();
                                        addedMonster.SetLevel(42);
                                        addedMonster.SkillManager.ResetSkills();
                                    }
                                    catch (Exception ex)
                                    {
                                        Plugin.Log.LogError($"Unable to add monster {monster.Name} ({monster.ID}): {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogError($"Something went wrong: {ex.Message}");
                        }
                    }
                }

                // Now that all of the monsters have been added, begin to export them                
                var allLoadedMonsters = __instance.AllMonster.Where(w => w.Shift == EShift.Normal).ToList();

                var targetSkillTypes = Plugin.TargetSkillTypes;
                var targetItemTypes = Plugin.TargetItemTypes;
                var mapDatas = GameController.Instance.WorldData.Maps;

                foreach (var loadedMonster in allLoadedMonsters)
                {
                    try
                    {
                        Plugin.Log.LogInfo($"Exporting monster: {loadedMonster.OriginalMonsterName}");

                        string thisMonsterFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\Monsters\\{loadedMonster.OriginalMonsterName}.json";
                        var monsterProperties = ExportUtilities.GetMonsterProperties(loadedMonster, targetSkillTypes, targetItemTypes, mapDatas);

                        Plugin.Log.LogInfo($"Saving monster: {loadedMonster.OriginalMonsterName}");

                        ExportUtilities.SerializeAndWriteObjectToFile(thisMonsterFileName, monsterProperties);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError($"Unable to export monster: {ex.Message}");
                    }
                }

                // Make exports of items
                var items = GameController.Instance.WorldData.Referenceables.Where(x => x?.gameObject?.GetComponent<BaseItem>() != null).Select(x => x.gameObject.GetComponent<BaseItem>()).ToList();
                foreach (var item in items)
                {
                    try
                    {
                        var itemName = item.GetName();

                        if (itemName == "??? Egg")
                        {
                            itemName = "Unknown Egg";
                        }

                        Plugin.Log.LogInfo($"Exporting Item {itemName}");

                        var itemFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\Items\\{itemName}.json";
                        var itemObject = ExportUtilities.GetItemProperties(item.gameObject, targetItemTypes);

                        ExportUtilities.SerializeAndWriteObjectToFile(itemFileName, itemObject);
                    }
                    catch (Exception EX)
                    {
                        Plugin.Log.LogError($"Something went wrong: {EX.Message}");
                    }
                }

                // Make exports of some important enums as well
                try
                {
                    Plugin.Log.LogInfo("Gathering Enum Data...");
                    var targetEnums = new List<Type>();
                    targetEnums.Add(typeof(EElement));
                    targetEnums.Add(typeof(EDamageType));
                    targetEnums.Add(typeof(ETargetType));
                    targetEnums.Add(typeof(EShift));
                    targetEnums.Add(typeof(EMonsterType));
                    targetEnums.Add(typeof(EStat));
                    targetEnums.Add(typeof(Equipment.EquipmentType));
                    targetEnums.Add(typeof(BuffManager.ESpecialBuff));
                    targetEnums.Add(typeof(BuffManager.BuffType));
                    targetEnums.Add(typeof(BuffManager.DebuffType));

                    foreach (var targetEnum in targetEnums)
                    {
                        var thisEnumsResults = new Dictionary<string, object>();

                        thisEnumsResults["KeyValueObjects"] = ExportUtilities.GetEnumAsKeyValueObjects(targetEnum);
                        thisEnumsResults["KeyValues"] = ExportUtilities.GetEnumAsKeyValuePairs(targetEnum);
                        thisEnumsResults["InvertedKeyValues"] = ExportUtilities.GetEnumAsInvertedKeyValuePairs(targetEnum);

                        string thisEnumsFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\Enums\\{targetEnum.Name}.json";

                        Plugin.Log.LogInfo($"Writing Enums Data for {targetEnum.Name}");
                        ExportUtilities.SerializeAndWriteObjectToFile(thisEnumsFileName, thisEnumsResults);
                        Plugin.Log.LogInfo($"Done Writing Enums Data for {targetEnum.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Something went wrong: {ex.Message}");
                }

                // Get Details of Buffs
                try
                {
                    Plugin.Log.LogInfo("Gathering Buff Data...");

                    var targetEnum = typeof(BuffManager.BuffType);
                    var enumValues = targetEnum.GetEnumValues();

                    foreach (var enumValue in enumValues)
                    {
                        var buffProperties = ExportUtilities.GetBuffProperties((BuffManager.BuffType)enumValue);

                        if ((string)buffProperties["Name"] != string.Empty)
                        {
                            var buffFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\Buffs\\{buffProperties["Name"]}.json";

                            Plugin.Log.LogInfo($"Writing Buff Data for {buffProperties["Name"]}");
                            ExportUtilities.SerializeAndWriteObjectToFile(buffFileName, buffProperties);
                            Plugin.Log.LogInfo($"Done Writing Buff Data for {buffProperties["Name"]}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Something went wrong: {ex.Message}");
                }

                // Get Details of Debuffs
                try
                {
                    Plugin.Log.LogInfo("Gathering Debuff Data...");

                    var targetEnum = typeof(BuffManager.DebuffType);
                    var enumValues = targetEnum.GetEnumValues();

                    foreach (var enumValue in enumValues)
                    {
                        var debuffProperties = ExportUtilities.GetDebuffProperties((BuffManager.DebuffType)enumValue);

                        if ((string)debuffProperties["Name"] != string.Empty)
                        {
                            var debuffFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\Debuffs\\{debuffProperties["Name"]}.json";

                            Plugin.Log.LogInfo($"Writing Buff Data for {debuffProperties["Name"]}");
                            ExportUtilities.SerializeAndWriteObjectToFile(debuffFileName, debuffProperties);
                            Plugin.Log.LogInfo($"Done Writing Buff Data for {debuffProperties["Name"]}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Something went wrong: {ex.Message}");
                }

                // Get Details of Special Buffs
                try
                {
                    Plugin.Log.LogInfo("Gathering Special Buff Data...");

                    var targetEnum = typeof(BuffManager.ESpecialBuff);
                    var enumValues = targetEnum.GetEnumValues();

                    foreach (var enumValue in enumValues)
                    {
                        try
                        {
                            var specialBuffProperties = ExportUtilities.GetSpecialBuffProperties((BuffManager.ESpecialBuff)enumValue, __instance.AllMonster);

                            if ((string)specialBuffProperties["Name"] != string.Empty)
                            {
                                var specialBuffFileName = $"{Directory.GetCurrentDirectory()}\\BepInEx\\plugins\\AddAndExportAllMonsters\\SpecialBuffs\\{specialBuffProperties["Name"]}.json";

                                Plugin.Log.LogInfo($"Writing SpecialBuff Data for {specialBuffProperties["Name"]}");
                                ExportUtilities.SerializeAndWriteObjectToFile(specialBuffFileName, specialBuffProperties);
                                Plugin.Log.LogInfo($"Done Writing SpecialBuff Data for {specialBuffProperties["Name"]}");
                            }

                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogError($"Something went wrong while gathering special buff data for {enumValue}: {ex.Message}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Something went wrong: {ex.Message}");
                }
            }
        }

    }
}
