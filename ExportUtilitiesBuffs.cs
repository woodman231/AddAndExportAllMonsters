using System.Collections.Generic;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static Dictionary<string, object> GetBuffProperties(BuffManager.BuffType buffType)
        {
            var results = new Dictionary<string, object>();

            results["Name"] = BuffManager.GetBuffName(buffType);
            results["Value"] = (int)buffType;
            results["Icon"] = BuffManager.GetBuffIcon(buffType);
            results["Description"] = StripCurlyBraces(BuffManager.GetBuffDescription(buffType));

            return results;
        }
    }
}
