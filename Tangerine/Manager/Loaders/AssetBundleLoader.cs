using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Patchers;

namespace Tangerine.Manager.Loaders
{
    internal static class AssetBundleLoader
    {
        private const string JsonFile = "AssetBundleConfig.json";
        private const string AssetBundleFolder = "AssetBundles";
        private static readonly string DownloadFolder = Path.Combine("StreamingAssets", "DownloadData");

        public static bool Load(string modPath, TangerineLoader loader)
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(Path.Combine(modPath, JsonFile)));
                var list = node["ListAssetbundleId"]?.AsArray();

                if (list == null)
                {
                    Plugin.Log.LogError($"Failed to read {JsonFile} for mod \"{modPath}\"");
                    return false;
                }

                var assetBundleFolder = Path.Combine(modPath, AssetBundleFolder);

                foreach (var bundle in list.Select(DeserializeAssetBundleId))
                {
                    var id = bundle.bundleInfo;
                    var dependencies = bundle.dependencies;
                    loader.AddAssetBundleDependencies(id.name, dependencies);

                    var bundleNamePath = Path.Combine(assetBundleFolder, id.name.Replace('/', Path.DirectorySeparatorChar));
                    var bundleHashPath = Path.Combine(assetBundleFolder, id.hash);
                    var bundleGamePath = Path.Combine(BepInEx.Paths.GameDataPath, DownloadFolder, id.hash);

                    if (File.Exists(bundleNamePath))
                    {
                        // Prioritize loading from real file name
                        id.size = new FileInfo(bundleNamePath).Length;
                        loader.AddAssetBundleId(id, bundleNamePath);
                    }
                    else if (File.Exists(bundleHashPath))
                    {
                        // Next option, load from hash name in mod folder
                        id.size = new FileInfo(bundleHashPath).Length;
                        loader.AddAssetBundleId(id, bundleHashPath);
                    }
                    else if (File.Exists(bundleGamePath))
                    {
                        // Fall back to game folder if neither of the above exist (this can be used to modify hash, crc, and size of existing vanilla bundles)
                        Plugin.Log.LogWarning($"Custom bundle {id.name} does not exist in \"{Path.Combine(Path.GetFileName(modPath), AssetBundleFolder)}\". Falling back to game's DownloadData folder");
                        id.size = new FileInfo(bundleGamePath).Length;
                        loader.AddAssetBundleId(id, bundleGamePath);
                    }
                    else
                    {
                        Plugin.Log.LogWarning($"Failed to add bundle {id.name} for mod \"{modPath}\": No bundle exists on disk");
                    }
                }
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
            TangerineLoader.AssetBundleIds.OnModDisabled(modId);
            TangerineLoader.AssetBundlePaths.OnModDisabled(modId);
        }

        public static bool HasContentToLoad(string modPath)
        {
            return File.Exists(Path.Combine(modPath, JsonFile));
        }

        private static (AssetbundleId bundleInfo, HashSet<string> dependencies) DeserializeAssetBundleId(JsonNode node)
        {
            var dependList = new HashSet<string>();
            var dList = node["dependencies"]?.AsArray();
            if (dList != null)
            {
                foreach (var dependency in dList)
                    dependList.Add(dependency.ToString());
            }

            var name = node["name"].Deserialize<string>();
            var hash = node["hash"]?.Deserialize<string>();
            if (string.IsNullOrEmpty(hash))
                hash = AssetsBundleManager.Instance.FileName2MD5(name);
            
            var crc = node["crc"]?.Deserialize<uint>();
            if (crc == null) crc = 0;

            return (new AssetbundleId(name, hash, (uint)crc, 0), dependList);
        }
    }
}
