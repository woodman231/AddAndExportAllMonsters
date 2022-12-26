using System;
using System.Collections.Generic;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static List<object> GetEnumAsKeyValueObjects(Type type)
        {
            var results = new List<object>();

            if (type.IsEnum == false)
            {
                Plugin.Log.LogWarning($"A non enum was passed in to GetEnumAsKeyValueObjects ({type.FullName})");
            }
            else
            {
                var values = Enum.GetValues(type);

                foreach (var value in values)
                {
                    string enumName = Enum.GetName(type, value);
                    int enumValue = (int)value;

                    var props = new Dictionary<string, object>();

                    props["Key"] = enumName;
                    props["Value"] = enumValue;

                    results.Add(props);
                }
            }

            return results;
        }

        public static Dictionary<string, int> GetEnumAsKeyValuePairs(Type type)
        {
            var results = new Dictionary<string, int>();

            if (type.IsEnum == false)
            {
                Plugin.Log.LogWarning($"A non enum was passed in to GetEnumAsKeyValuePairs ({type.FullName})");
            }
            else
            {
                var values = Enum.GetValues(type);
                foreach (var value in values)
                {
                    int enumValue = (int)value;
                    string enumName = Enum.GetName(type, value);

                    results.Add(enumName, enumValue);
                }
            }

            return results;
        }

        public static Dictionary<int, string> GetEnumAsInvertedKeyValuePairs(Type type)
        {
            var results = new Dictionary<int, string>();

            if (type.IsEnum == false)
            {
                Plugin.Log.LogWarning($"A non enum was passed in to GetEnumAsInvertedKeyValuePairs ({type.FullName})");
            }
            else
            {
                var values = Enum.GetValues(type);
                foreach (var value in values)
                {
                    int enumValue = (int)value;
                    string enumName = Enum.GetName(type, value);

                    results.Add(enumValue, enumName);
                }
            }

            return results;
        }
    }
}
