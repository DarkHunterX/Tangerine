using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Patchers;
using UnityEngine;


namespace Tangerine.Manager.Loaders
{
    internal static class DebutEggLoader
    {
        private const string JsonFile = "DebutEggConfig.json";

        private struct DebutEggData
        {
            public int Model;
            public int Trigger;
            public string EggStart;
            public string EggLoop;
        }

        public static bool Load(string modPath, TangerineDebutEgg loader)
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(Path.Combine(modPath, JsonFile)));
                var list = node["ListDebutEgg"]?.AsArray();

                if (list == null)
                {
                    Plugin.Log.LogError($"Failed to read {JsonFile} for mod \"{modPath}\"");
                    return false;
                }

                foreach (var debutEgg in list.Select(DeserializeDebutEgg))
                    loader.AddDebutEggData(debutEgg.Model, debutEgg.Trigger, debutEgg.EggStart, debutEgg.EggLoop);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to read {JsonFile} for mod \"{modPath}\": {e}");
                return false;
            }

            return true;
        }

        public static void Unload(string modId)
        {
            TangerineLoader.FilePaths.OnModDisabled(modId);
        }

        public static bool HasContentToLoad(string modPath)
        {
            return File.Exists(Path.Combine(modPath, JsonFile));
        }

        private static DebutEggData DeserializeDebutEgg(JsonNode node)
        {
            var debutEggData = new DebutEggData()
            {
                Model = node["Model"].Deserialize<int>(),
                Trigger = node["Trigger"].Deserialize<int>(),
                EggStart = node["EggStart"].Deserialize<string>(),
                EggLoop = node["EggLoop"].Deserialize<string>(),
            };

            return debutEggData;
        }
    }
}