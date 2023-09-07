#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using ToS;
using UnityEditor;

public class ReloadOverlay : MonoBehaviour
{
    [MenuItem("ToS/Avatar System/Reload Avatar Overlays")]
    public static void Reload()
    {
        string[] Subdirs = { "Plates", "Cloths", "Leathers", "Others", "Robe", "NPC" };

        foreach (string subdir in Subdirs)
        {
            string fullDirItems = Application.dataPath + "/_ToS/Player/" + subdir;
            string[] sDirectoriesItems = Directory.GetDirectories(fullDirItems);

            foreach (var directory in sDirectoriesItems)
            {
                if (Directory.Exists(directory + "/Overlays"))
                {
                    string[] aFilePaths = Directory.GetFiles(directory + "/Overlays", "*.asset");

                    foreach (string sFilePath in aFilePaths)
                    {
                        if (Path.GetExtension(sFilePath) == ".asset" || Path.GetExtension(sFilePath) == ".asset")
                        {
                         
                            Object overlayObject = AssetDatabase.LoadAssetAtPath("Assets" + sFilePath.Replace(Application.dataPath, ""), typeof(ItemOverlay));
                            ItemOverlay overlay = overlayObject as ItemOverlay;
                            overlay.LoadPrefab("Assets" + sFilePath.Replace(Application.dataPath, ""), overlayObject.name);
                            EditorUtility.SetDirty(overlay);

                            Debug.Log("Reload overlay: Assets" + sFilePath.Replace(Application.dataPath, ""));
                        }
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
    }
}
#endif