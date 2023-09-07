using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using ToS;

namespace ToS {

    [CreateAssetMenu(fileName = "Item Overlay", menuName = "ToS/AvatarSystem", order = 1)]
    public class ItemOverlay : ScriptableObject
    {
        public ItemAvatarType Type;
        public string Namespace;
        public string PrefabMale;
        public string PrefabFemale;
        public string PrefabDwarfMale;
        public string PrefabDwarfFemale;
        public string PrefabOrcMale;
        public string PrefabOrcFemale;
        public bool RemoveUnderwear;
        public bool RemoveHair;
        public bool RemoveBeard;
        public bool RemoveHands;
        public bool RemoveLegs;
        public bool RemoveChest;
        public bool ExtendBust;
        public bool RemoveChestArmor;
        public bool RemoveHelmArmor;
        public bool RemoveBootArmor;
        public bool RemovePantsArmor;
        public bool RemoveGlovesArmor;
        public ViewBody HandsView;
        public ViewBody LegsView;
        public bool OnlyFemale;
        public bool OnlyMale;

#if UNITY_EDITOR
        public void LoadPrefab(string overlayFilename = "", string objectName = "")
        {
            string path = (overlayFilename != "") ? overlayFilename : AssetDatabase.GetAssetPath(Selection.activeObject);
            string filename = (overlayFilename != "") ? objectName : Selection.activeObject.name;

            Namespace = filename;

            if (path != "" && Path.GetExtension(path) != "")            
                path = path.Replace(objectName + ".asset", "");
            
            path = path.Replace("Overlays", "Prefabs");
            path = path.Replace("\\", "/"); 

            PrefabMale = path + "H_M_" + filename + ".prefab";
            PrefabFemale = path + "H_F_" + filename + ".prefab";
            PrefabDwarfMale = path + "D_M_" + filename + ".prefab";
            PrefabDwarfFemale = path + "D_F_" + filename + ".prefab";
            PrefabOrcMale = path + "O_M_" + filename + ".prefab";
            PrefabOrcFemale = path + "O_F_" + filename + ".prefab";

            string[] nameSplit = filename.Split("_");

            if(nameSplit.Length > 1)
            {
                switch (nameSplit[1])
                {
                    case "Boots": Type = ItemAvatarType.Boots; break;
                    case "Chest": Type = ItemAvatarType.Chest; break;
                    case "Pants": Type = ItemAvatarType.Pants; break;
                    case "Gloves": Type = ItemAvatarType.Gloves; break;
                    case "Helm": Type = ItemAvatarType.Helmet; break;
                    case "Cloak": Type = ItemAvatarType.Cloak; break;
                    case "Robe": Type = ItemAvatarType.Robe; break;
                }
            }

            EditorUtility.SetDirty(this);
        }
#endif
    }
}


