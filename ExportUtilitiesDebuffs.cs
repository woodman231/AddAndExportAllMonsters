using System.Collections.Generic;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static Dictionary<string, object> GetDebuffProperties(BuffManager.DebuffType debuffType)
        {
            var results = new Dictionary<string, object>();

            results["Name"] = BuffManager.GetDebuffName(debuffType);
            results["Value"] = (int)debuffType;
            results["Icon"] = BuffManager.GetDebuffIcon(debuffType);
            results["Description"] = StripCurlyBraces(BuffManager.GetDebuffDescription(debuffType));

            return results;
        }
    }
}
