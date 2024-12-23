using UnityEditor;
using UnityEngine;

namespace Eran
{
    internal static class ProjectViewGui
    {
        private static GUIStyle projectView = new GUIStyle()
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = Color.blue },
        };

        internal static void OnProjectWindowItemOnGui(string guid, Rect rect)
        {
            if (!Globals<Config>.Value.MarkRed) return;

            var store = Globals<BacklinkStore>.Value;
            if (!store.Initialized) return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            ShowRowQuantity(rect, path, store);

            long size = 0;
            var _ = store.UnusedFiles.TryGetValue(path, out size) || store.UnusedScenes.TryGetValue(path, out size);

            if (SearchUtils.IsUnused(path))
            {
                var buf = GUI.color;
                {
                    GUI.color = new Color(1, 0, 0, 1f);
                    GUI.Box(rect, string.Empty);
                }
                GUI.color = buf;
                GUI.Label(rect, CommonUtils.BytesToString(size),projectView);
            }
        }


        internal static void ShowRowQuantity(Rect rect, string path, BacklinkStore backlinkStore)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return;

            backlinkStore.FoldersWithQty.TryGetValue(path, out var folderWithQty);

            var cntFiles = folderWithQty?.UnusedFilesQty ?? 0;
            var cntScenes = folderWithQty?.UnusedScenesQty ?? 0;
            long size = folderWithQty?.UnusedSize ?? 0;

            if (cntFiles == 0 && cntScenes == 0) return;
            var countStr = cntFiles + cntScenes > 0
                ? $"{cntFiles} | {cntScenes} ({CommonUtils.BytesToString(size)})"
                : "";
            GUI.Label(rect, countStr, projectView);
        }
    }
}