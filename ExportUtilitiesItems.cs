using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AddAndExportAllMonsters
{
    public partial class ExportUtilities
    {
        public static List<Type> GetBaseItemTypes()
        {
            var results = new List<Type>();

            var baseItemAssembly = typeof(BaseItem).Assembly;
            var typesInBaseItemAssembly = baseItemAssembly.GetTypes();

            var baseItemTypes = typesInBaseItemAssembly.Where(w => w.BaseType == typeof(BaseItem)).ToList();

            foreach (var baseItemType in baseItemTypes)
            {
                results.Add(baseItemType);

                var additionalTypes = typesInBaseItemAssembly.Where(w => w.BaseType == baseItemType).ToList();
                if (additionalTypes is not null)
                {
                    foreach (var additionalType in additionalTypes)
                    {
                        results.Add(additionalType);
                    }
                }
            }

            return results;
        }

        public static Dictionary<string, object> GetItemProperties(GameObject itemGameObject, IEnumerable<Type> targetItemTypes)
        {
            var results = new Dictionary<string, object>();

            var gameObjectAsBaseItem = (BaseItem)itemGameObject.GetComponent<BaseItem>();

            if (gameObjectAsBaseItem is not null)
            {
                results["ID"] = gameObjectAsBaseItem.ID;
                results["Name"] = gameObjectAsBaseItem.GetName();
                results["Description"] = StripColorCodes(StripNewLine(gameObjectAsBaseItem.GetTooltip(0)));
                results["Icon"] = gameObjectAsBaseItem.GetIconInfo();
                results["Price"] = gameObjectAsBaseItem.Price;

                foreach (var itemType in targetItemTypes)
                {
                    var itemAsTargetType = itemGameObject?.gameObject?.GetComponent(itemType);
                    if (itemAsTargetType is not null)
                    {
                        results[$"Is{itemType.Name}"] = true;

                        var targetTypeResults = new Dictionary<string, object>();

                        var fields = itemType.GetFields();

                        foreach (var field in fields)
                        {
                            if (field.DeclaringType == itemType)
                            {
                                object valueToSet = null;

                                if (field.FieldType == typeof(GameObject) || field.FieldType.BaseType == typeof(GameObject))
                                {
                                    valueToSet = GetReferenceableIDFromGameObject((GameObject)field.GetValue(itemAsTargetType));
                                }
                                else if (field.FieldType == typeof(List<GameObject>) || field.FieldType.BaseType == typeof(List<GameObject>))
                                {
                                    valueToSet = GetReferenceableIDsFromGameObjects((List<GameObject>)field.GetValue(itemAsTargetType));
                                }
                                else if (field.FieldType == typeof(string))
                                {
                                    valueToSet = StripColorCodes(StripNewLine((string)field.GetValue(itemAsTargetType)));
                                }
                                else if (field.FieldType == typeof(UnityEngine.Vector3))
                                {
                                    valueToSet = null;
                                }
                                else if (field.FieldType == typeof(BaseItem))
                                {
                                    var fieldValueAsBaseItem = (BaseItem)field.GetValue(itemAsTargetType);

                                    if (fieldValueAsBaseItem is not null)
                                    {
                                        valueToSet = fieldValueAsBaseItem.ID;
                                    }
                                }
                                else if (field.FieldType == typeof(List<ItemQuantity>))
                                {
                                    var valueAsListOfItemQuantity = (List<ItemQuantity>)field.GetValue(itemAsTargetType);

                                    var itemQuantityResults = new List<object>();

                                    if (valueAsListOfItemQuantity is not null)
                                    {
                                        foreach (var valueAsItemQuanity in valueAsListOfItemQuantity)
                                        {
                                            var itemQuantityDetails = new Dictionary<string, object>();

                                            var itemDetails = GetItemProperties(valueAsItemQuanity.Item, targetItemTypes);

                                            itemQuantityDetails["Item"] = itemDetails;
                                            itemQuantityDetails["Quantity"] = valueAsItemQuanity.Quantity;

                                            itemQuantityResults.Add(itemQuantityDetails);
                                        }
                                    }

                                    valueToSet = itemQuantityResults;
                                }
                                else if (field.FieldType == typeof(List<CraftBox.RewardDefine>))
                                {
                                    var valueAsRewardsList = (List<CraftBox.RewardDefine>)field.GetValue(itemAsTargetType);

                                    var rewardsListResult = new List<object>();

                                    foreach (CraftBox.RewardDefine rewardDefine in valueAsRewardsList)
                                    {
                                        var rewardsObject = new Dictionary<string, object>();

                                        rewardsObject["CommonRewards"] = ExportUtilities.GetItemsProperty(rewardDefine.CommonRewards, targetItemTypes);
                                        rewardsObject["RareRewards"] = ExportUtilities.GetItemsProperty(rewardDefine.RareRewards, targetItemTypes);
                                        rewardsObject["Rank"] = rewardDefine.Rank;

                                        rewardsListResult.Add(rewardsObject);
                                    }

                                    valueToSet = rewardsListResult;
                                }
                                else
                                {
                                    valueToSet = field.GetValue(itemAsTargetType);
                                }


                                targetTypeResults[field.Name] = valueToSet;
                            }
                        }

                        if (itemType.BaseType == typeof(Equipment))
                        {
                            var methods = itemType.GetMethods();
                            foreach (var method in methods)
                            {
                                if (method.DeclaringType == itemType)
                                {
                                    if (method.Name.StartsWith("On"))
                                    {
                                        targetTypeResults[$"Is{method.Name}"] = true;
                                    }
                                }
                            }
                        }

                        results[$"{itemType.Name}Properties"] = targetTypeResults;
                    }
                }
            }

            return results;
        }

        public static List<object> GetItemsProperty(IEnumerable<GameObject> itemsAsGameObjects, IEnumerable<Type> targetItemTypes)
        {
            var results = new List<object>();

            foreach (var itemAsGameObject in itemsAsGameObjects)
            {
                results.Add(GetItemProperties(itemAsGameObject, targetItemTypes));
            }

            return results;
        }
    }
}
