using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AddAndExportAllMonsters
{
    public static partial class ExportUtilities
    {
        public static List<Type> GetAssemblyTypes()
        {
            var monsterType = typeof(Monster);
            var assembly = monsterType.Assembly;

            return assembly.GetTypes().ToList();
        }

        public static string StripColorCodes(string input)
        {
            return Regex.Replace(input, @"\^C[a-z0-9]{8}", "");
        }

        public static string StripNewLine(string input)
        {
            return Regex.Replace(input, @"[\r\n]+", " ");
        }

        public static string StripCurlyBraces(string input)
        {
            string withoutOpenCurlyBrace = input.Replace("{", "");
            string withoutClosedCurlyBrace = withoutOpenCurlyBrace.Replace("}", "");

            return withoutClosedCurlyBrace;
        }

        public static int? GetReferenceableIDFromGameObject(GameObject gameObjectInQuestion)
        {
            var gameObjectAsReferenceable = gameObjectInQuestion?.gameObject?.GetComponent<Referenceable>();
            if (gameObjectAsReferenceable is not null)
            {
                return gameObjectAsReferenceable.ID;
            }

            return null;
        }

        public static int[] GetReferenceableIDsFromGameObjects(List<GameObject> gameObjects)
        {
            var results = new List<int>();

            foreach (var gameObject in gameObjects)
            {
                var gameObjectAsReferenceable = gameObject?.gameObject?.GetComponent<Referenceable>();
                if (gameObjectAsReferenceable is not null)
                {
                    results.Add(gameObjectAsReferenceable.ID);
                }
            }

            if (results.Count == 0)
            {
                return null;
            }
            else
            {
                return results.ToArray();
            }
        }
    }
}
