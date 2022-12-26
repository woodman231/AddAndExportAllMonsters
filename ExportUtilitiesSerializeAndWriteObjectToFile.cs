using Newtonsoft.Json;
using System.IO;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static void SerializeAndWriteObjectToFile(string fileName, object objectDetails)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, objectDetails);
                }
            }
        }
    }
}
