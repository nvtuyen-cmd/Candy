using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace EranServer
{
    public class EranAssetBundlesEditor
    {
        public static EranAssetBundlesEditor Ins;
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            Ins = new EranAssetBundlesEditor();
            if (!Directory.Exists(assetBundlePath))
            {
                Directory.CreateDirectory(assetBundlePath);
            }
        }
        public byte[] AssetBundleToBytes(EranAssetBundles _target)
        {
            if (string.IsNullOrEmpty(_target.AssetId))
            {
                _target.Reset();
            }
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>()
            {
                new AssetBundleBuild
                {
                    assetBundleName = _target.AssetId,
                    assetNames = new[] { AssetDatabase.GetAssetPath(_target) }
                }
            };
            BuildHandler(assetBundleBuilds.ToArray());
            string path = Path.Combine(assetBundlePath, _target.AssetId);
            return File.ReadAllBytes(path);
        }
        public void Builds(EranAssetBundles[] _targets)
        {
            List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
            foreach (EranAssetBundles target in _targets)
            {
                if (string.IsNullOrEmpty(target.AssetId))
                {
                    target.Reset();
                }
                if (assetBundleBuilds.Exists(x => x.assetBundleName == target.AssetId))
                {
                    continue;
                }
                assetBundleBuilds.Add(new AssetBundleBuild
                {
                    assetBundleName = target.AssetId,
                    assetNames = new[] { AssetDatabase.GetAssetPath(target) }
                });
            }

            BuildHandler(assetBundleBuilds.ToArray());
        }
        private void BuildHandler(AssetBundleBuild[] _assetBundleBuilds)
        {
            BuildPipeline.BuildAssetBundles(assetBundlePath, _assetBundleBuilds, BuildAssetBundleOptions.None, buildTargetGroup);
        }
        private static readonly string assetBundlePath = Path.Combine(Application.dataPath, "../Asset_Bundles");
        private static readonly BuildTarget buildTargetGroup = EditorUserBuildSettings.activeBuildTarget;
    }
}
