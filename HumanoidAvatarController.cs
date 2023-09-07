using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition;
using UniRx;
using Shared; 
using Cysharp.Threading.Tasks;
using ToS.Model;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToS
{
    [Serializable]
    public enum ItemAvatarType
    {
        None,
        Chest,
        Pants,
        Gloves,
        Boots,
        Helmet,
        Cloak,
        Acessorie,
        ShortSkirt,
        LongDress,
        Robe
    }

    [Serializable]
    public struct MaterialVariant
    {
        public string Name;
        public Material Arm;
        public Material Body;
        public Material Head;
        public Material Leg;
        public Material Nails;
    }

    [Serializable]
    public struct RaceElement
    {
        public string Namespace;
        public RaceType Race;
        public GenderType Gender;
        public Avatar AnimatorAvatar;
        public string FullBody;
        public string FullHands;
        public string FullLegs;
        public string Hands;
        public string Head;
        public string Legs;
        public string MiddleHands;
        public string MiddleLegs;
        public string Underwear;
        public string Bust;
        public List<MaterialVariant> Materials;
        public Vector3 EyesOffset;
        public Vector3 CameraBodyPosition;
        public Quaternion CameraBodyRotation;
        public Vector3 CameraHeadPosition;
        public Quaternion CameraHeadRotation;
    }

    [Serializable]
    public struct RecourceAvatar
    {
        public string Namespace;
        public RaceType Race;
        public GenderType Gender;
        public string Prefab;
        public Sprite Icon;
    }

    [Serializable]
    public enum ViewBody
    {
        Full,
        Middle,
        Min
    }

    [Serializable]
    public class ItemAvatar
    {
        public string Namespace;
        public bool Active;
        public ItemOverlay Overlay;
    }

    [Serializable]
    public enum MaterialType
    {
        Base,
        African,
        Asian,
        Athletic,
        Old,
        Thin,
        Heavy
    }

    [Serializable]
    public struct BoneMapper
    {
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [Serializable]
    public struct BoneMapperByRace
    {
        public string Namespace;
        public RaceType Race;
        public GenderType Gender;
        public List<BoneMapper> BoneMappers;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HumanoidAvatarController))]
    public class HumanoidAvatarControllerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            HumanoidAvatarController humanoidAvatarController = (HumanoidAvatarController)target;

            if (GUILayout.Button("Refresh Avatar"))
            {
                humanoidAvatarController.Refresh();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                humanoidAvatarController.CreatePrefab();
            }

            if (GUILayout.Button("Create Pooling"))
            {
                humanoidAvatarController.CreatePooling();
            }

            if (GUILayout.Button("Recalculate Bones"))
            {
                humanoidAvatarController.RecalculateBones();
            }

            if (GUILayout.Button("Mapper Resources"))
            {
                humanoidAvatarController.MapperResources();
            }

            if (GUILayout.Button("Save data to File"))
            {
                humanoidAvatarController.Save();
            }

            if (GUILayout.Button("Load data to File"))
            {
                humanoidAvatarController.Load();
            }
        }
    }
#endif

    public class ItemSettings
    {
        public bool PersistentHand;
    }

    [ExecuteInEditMode]
    public class HumanoidAvatarController : MonoBehaviour
    {
        public AvatarModel model;

        private int RaceIndex = 0;

        private bool inRefreshProcess = false;
        
        private string[] IgnoreElements = { 
            "CC_Base_TearLine", "CC_Base_EyeOcclusion", "CC_Base_Eye",
            "CC_Base_Teeth", "CC_Base_Tongue"
        };

        private string[] TrashTextures =
        {
            "Std_Cornea_L_Diffuse", "Std_Cornea_R_Diffuse", "Std_Eye_L_Diffuse",
            "Std_Eye_L_Normal", "Std_Eye_Occlusion_L_Diffuse", "Std_Eye_Occlusion_R_Diffuse",
            "Std_Eye_R_Diffuse", "Std_Eye_R_Normal", "Std_Eyelash_Diffuse",
            "Std_Eyelash_Normal", "Std_Lower_Teeth_Diffuse", "Std_Lower_Teeth_Normal",
            "Std_Nails_Diffuse", "Std_Nails_Normal", "Std_Skin_Arm_Diffuse",
            "Std_Skin_Arm_Normal", "Std_Skin_Body_Diffuse", "Std_Skin_Body_Normal",
            "Std_Skin_Head_Diffuse", "Std_Skin_Head_Normal", "Std_Skin_Leg_Diffuse",
            "Std_Skin_Leg_Normal", "Std_Tearline_L_Diffuse", "Std_Tearline_R_Diffuse",
            "Std_Tongue_Diffuse", "Std_Tongue_Normal", "Std_Upper_Teeth_Diffuse",
            "Std_Upper_Teeth_Normal", "Std_Eye_Occlusion_L_Opacity", "Std_Eye_Occlusion_R_Opacity",
            "Female_Brow_Base_Transparency_Diffuse", "Female_Brow_Transparency_Diffuse",
            "Male_Bushy_Base_Transparency_Diffuse", "Male_Bushy_Transparency_Diffuse"
        };

        private string[] TrashDiretories =
        {
            "CC_Base_Eye", "CC_Base_EyeOcclusion", "CC_Base_TearLine",
            "CC_Base_Teeth", "Male_Bushy", "Camila_Brow", "Female_Angled"
        };
        
        public GameObject Visual;      
        public Animator Animator;
        public GameObject Controller;        

        [Header("Bones")]
        [SerializeField] private string BonesName = "CC_Base_BoneRoot";
        [SerializeField] private GameObject Bones;
        public Transform HeadTransform;
        public Transform LeftHandTransform;
        public Transform RightHandTransform;
        public Transform BackSpineTransform;
        private List<BoneMapperByRace> BoneMappers = new List<BoneMapperByRace>();

        [Header("Diretories")]
        public string RacesDiretory;
        public string HairsDiretory;
        public string BeardDiretory;

        [Header("Visual")]
        public GameObject EyesPrefab;
        private GameObject EyesLeft;
        private GameObject EyesRight;
        private MaterialType _MaterialType = MaterialType.Base;
        public MaterialType MaterialType = MaterialType.Base;
        private List<GameObject> InUsePrefabs = new List<GameObject>();

        [Header("Colors")]
        private Color _HairColor;
        public Color HairColor;
        private Color _BeardColor;
        public Color BeardColor;
        private Color _BodyColor;
        public Color BodyColor;
        private Color _EyesColor;
        public Color EyesColor;
        public string HairColorsFile;
        public string BeardColorsFile;

        [Header("CameraOrientation")]
        private Vector3 CameraBodyPosition;
        private Quaternion CameraBodyRotation;
        private Vector3 CameraHeadPosition;
        private Quaternion CameraHeadRotation;

        [Header("Elements")]
        public List<RaceElement> RacesElements = new List<RaceElement>();
        public List<RecourceAvatar> HairElements = new List<RecourceAvatar>();
        public List<RecourceAvatar> BeardElements = new List<RecourceAvatar>();
        public List<ItemAvatar> Items = new List<ItemAvatar>();

        //Pooling
        [Header("Pooling")]
        public GameObject Pooling;
        public List<GameObject> ElementsPooling = new List<GameObject>();
        private Dictionary<string, int> PoolingIndex = new Dictionary<string, int>();

        //Events
        [Header("Events")]
        public UnityEvent OnRefresh;

        void Start()
        {
            model = new AvatarModel();
            //model.gender.Subscribe(_ => Refresh());
            //model.race.Subscribe(_ => Refresh());
            //model.hair.Subscribe(_ => Refresh());
            //model.beard.Subscribe(_ => Refresh());
            //model.hairColor.Subscribe(_ => Refresh());
            //model.beardColor.Subscribe(_ => Refresh());
            //Refresh();
        }

        public void EventBreakpoint(string eventName, long startTime)
        {
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Debug.Log(eventName + ": " + (now - startTime).ToString("0.00"));
        }

        public async Task Refresh()
        {
            if (!inRefreshProcess)
            {
                long startTimer = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                inRefreshProcess = true;

                //gameObject.SetActive(false);

                if (Controller != null)
                {
                    /*SkinnedMeshCombiner smc = Controller.GetComponent<SkinnedMeshCombiner>();

                    if (smc)
                    {
                        if (smc.isActiveAndEnabled)
                            smc.UndoCombineMeshes(false, true);
                    }*/
                }

                bool RemoveHead = false;
                bool RemoveUnderwear = false;
                bool RemoveHair = false;
                bool RemoveBeard = false;
                bool RemoveHands = false;
                bool RemoveLegs = false;
                bool RemoveChest = false;
                bool ExtendBust = false;
                bool RemoveArmor = false;
                bool RemoveHelmet = false;
                bool RemoveGloves = false;
                bool RemovePants = false;
                bool RemoveBoots = false;
                bool UseChest = false;
                bool UsePants = false;

                ViewBody Hands = ViewBody.Full;
                ViewBody Legs = ViewBody.Full;

                RaceIndex = GetRace(model.race.Value, model.gender.Value);
                RaceElement RaceElement = (RacesElements.Count > RaceIndex) ? RacesElements[RaceIndex] : RacesElements[0];

                CameraBodyPosition = RaceElement.CameraBodyPosition;
                CameraBodyRotation = RaceElement.CameraBodyRotation;
                CameraHeadPosition = RaceElement.CameraHeadPosition;
                CameraHeadRotation = RaceElement.CameraHeadRotation;

                ClearVisual();

                foreach (var item in Items)
                {
                    if (item.Active)
                    {
                        if (item.Overlay.RemoveUnderwear)
                            RemoveUnderwear = true;

                        if (item.Overlay.RemoveChest)
                            RemoveChest = true;

                        if (item.Overlay.RemoveHands)
                            RemoveHands = true;

                        if (item.Overlay.RemoveHair)
                            RemoveHair = true;

                        if (item.Overlay.RemoveBeard)
                            RemoveBeard = true;

                        if (item.Overlay.RemoveLegs)
                            RemoveLegs = true;

                        if (item.Overlay.ExtendBust)
                            ExtendBust = true;

                        if (item.Overlay.RemoveChestArmor)
                            RemoveArmor = true;

                        if (item.Overlay.RemoveGlovesArmor)
                            RemoveGloves = true;

                        if (item.Overlay.RemoveBootArmor)
                            RemoveBoots = true;

                        if (item.Overlay.RemoveHelmArmor)
                            RemoveHelmet = true;

                        if (item.Overlay.RemovePantsArmor)
                            RemovePants = true;

                        if (item.Overlay.Type == ItemAvatarType.Chest)
                            UseChest = true;

                        if (item.Overlay.Type == ItemAvatarType.Pants)
                            UsePants = true;

                        if (Hands == ViewBody.Full && item.Overlay.HandsView != ViewBody.Full)
                            Hands = item.Overlay.HandsView;
                        else if (Hands == ViewBody.Middle && item.Overlay.HandsView == ViewBody.Min)
                            Hands = ViewBody.Min;

                        if (Legs == ViewBody.Full && item.Overlay.LegsView != ViewBody.Full)
                            Legs = item.Overlay.HandsView;
                        else if (Legs == ViewBody.Middle && item.Overlay.LegsView == ViewBody.Min)
                            Legs = ViewBody.Min;
                    }
                }

                if(model.gender.Value == GenderType.Female)
                {
                    if (UsePants && UseChest)
                        RemoveUnderwear = true;
                }
                else
                {
                    if (UsePants)
                        RemoveUnderwear = true;
                }

                //Body
                if (!RemoveChest)
                {
                    CreateObject(RaceElement.FullBody, false, RaceElement, BodyColor);
                }
                else
                {
                    if (!RemoveHands)
                    {
                        switch (Hands)
                        {
                            case ViewBody.Full: CreateObject(RaceElement.FullHands, false, RaceElement, BodyColor); break;
                            case ViewBody.Middle: CreateObject(RaceElement.MiddleHands, false, RaceElement, BodyColor); break;
                            case ViewBody.Min: CreateObject(RaceElement.Hands, false, RaceElement, BodyColor); break;
                        }
                    }

                    if (!RemoveLegs)
                    {
                        switch (Legs)
                        {
                            case ViewBody.Full: CreateObject(RaceElement.FullLegs, false, RaceElement, BodyColor); break;
                            case ViewBody.Middle: CreateObject(RaceElement.MiddleLegs, false, RaceElement, BodyColor); break;
                            case ViewBody.Min: CreateObject(RaceElement.Legs, false, RaceElement, BodyColor); break;
                        }
                    }

                    if (!RemoveHead)
                        CreateObject(RaceElement.Head, false, RaceElement, BodyColor);

                    if (ExtendBust)
                        CreateObject(RaceElement.Bust, false, RaceElement, BodyColor);
                }

                //Underware
                if (RaceElement.Underwear != "" && !RemoveUnderwear)
                    CreateObject(RaceElement.Underwear, false, RaceElement, Color.clear);


                //Beard
                if (BeardElements.Count > 0 && !RemoveBeard)
                {
                    int pointer = -1;

                    foreach (var beard in BeardElements)
                    {
                        if (beard.Gender == RaceElement.Gender && beard.Race == RaceElement.Race)
                        {
                            pointer++;

                            if (pointer == model.beard.Value)
                            {
                                CreateObject(beard.Prefab, false, RaceElement, model.beardColor.Value);
                                break;
                            }
                        }
                    }
                }

                //Items
                foreach (var item in Items)
                {
                    if (item.Active &&
                        ((RaceElement.Gender == GenderType.Male && !item.Overlay.OnlyFemale) ||
                        (RaceElement.Gender == GenderType.Female && !item.Overlay.OnlyMale))
                    )
                    {
                        bool showItem = true;

                        switch (item.Overlay.Type)
                        {
                            case ItemAvatarType.Boots: showItem = (!RemoveBoots); break;
                            case ItemAvatarType.Helmet: showItem = (!RemoveHelmet); break;
                            case ItemAvatarType.Pants: showItem = (!RemovePants); break;
                            case ItemAvatarType.Chest: showItem = (!RemoveArmor); break;
                            case ItemAvatarType.Gloves: showItem = (!RemoveGloves); break;
                        }

                        if (showItem)
                        {
                            string prefabMale;
                            string prefabFemale;

                            switch (model.race.Value)
                            {
                                case RaceType.Dwarf:
                                    prefabMale = item.Overlay.PrefabDwarfMale;
                                    prefabFemale = item.Overlay.PrefabDwarfFemale;
                                    break;
                                case RaceType.Orc:
                                    prefabMale = item.Overlay.PrefabOrcMale;
                                    prefabFemale = item.Overlay.PrefabOrcFemale;
                                    break;
                                default:
                                    prefabMale = item.Overlay.PrefabMale;
                                    prefabFemale = item.Overlay.PrefabFemale;
                                    break;
                            }

                            if (RaceElement.Gender == GenderType.Female)
                                CreateObject(prefabFemale, false, RaceElement, Color.clear, false, item.Overlay.Type, true);
                            if (RaceElement.Gender == GenderType.Male)
                                CreateObject(prefabMale, false, RaceElement, Color.clear, false, item.Overlay.Type, true);
                        }
                    }
                }

                //GetComponent<FixLightingPlayer>().Fix();
                FixBones(RaceElement);
                ShowAvatar();

                //Hair
                if (HairElements.Count > 0 && !RemoveHair)
                {
                    int pointer = -1;

                    foreach (var hair in HairElements)
                    {
                        if (hair.Gender == RaceElement.Gender && hair.Race == RaceElement.Race)
                        {
                            pointer++;

                            if (pointer == model.hair.Value)
                            {
                                CreateObject(hair.Prefab, false, RaceElement, model.hairColor.Value);
                                break;
                            }
                        }
                    }
                }

                OnRefresh.Invoke();
                //gameObject.SetActive(true);
                inRefreshProcess = false;
            }            
        }

        public async Task CreateObject(string prefabOriginal, bool removeElement, RaceElement raceElement, 
            Color changeColor, bool Base = false, ItemAvatarType Type = ItemAvatarType.None, bool IsItem = false)
        {
            PrefabManager.Load(prefabOriginal, (GameObject prefab) =>
            {
                if (prefab != null)            
                    CreateObject(prefab, removeElement, raceElement, changeColor, Base, Type, IsItem);
            });                
        }

        public async Task CreateObject(GameObject prefabOriginal, bool removeElement, RaceElement raceElement, 
            Color changeColor, bool Base = false, ItemAvatarType Type = ItemAvatarType.None, bool IsItem = false)
        {
            if (prefabOriginal != null && !removeElement)
            {
                GameObject prefab = await GetInPooling(prefabOriginal.name, Visual.transform, false, prefabOriginal);
                InUsePrefabs.Add(prefab);

                if (prefab != null)
                {
                    prefab.SetActive(true);

                    if(Type != ItemAvatarType.Helmet)                    
                        prefab.transform.SetParent(Visual.transform);                    
                    else                    
                        prefab.transform.SetParent(HeadTransform);

                    Animator animator = prefab.GetComponent<Animator>();

                    if (animator != null)                    
                        animator.enabled = false;
                    
                    Component[] elements = prefab.transform.GetComponentsInChildren(typeof(Transform));

                    foreach (var element in elements)
                    {
                        if (prefab && prefab.transform.childCount > 0 && element != null)
                        {
                            if (Bones != null && !IgnoreElements.Contains(element.gameObject.name) || Base)
                            {
                                SkinnedMeshRenderer smr = element.gameObject.GetComponent<SkinnedMeshRenderer>();

                                if (smr != null)
                                    FixColorAndLighting(smr, changeColor, raceElement, IsItem);
                            }
                        }
                    }
                }
            }
        }

        public void ShowAvatar()
        {
            if (Visual.transform.childCount > 0)
            {
                int totalChilds = Visual.transform.childCount;

                for (int i = 0; i < totalChilds; i++)
                {
                    var element = Visual.transform.GetChild(i).gameObject;

                    if (!element.activeInHierarchy)
                    {
                        element.SetActive(true);
                    }
                }
            }

            SkinnedMeshRenderer[] meshs = Visual.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach(var mesh in meshs)
            {
                if (!mesh.enabled)
                    mesh.enabled = true;
            }

            if(Controller != null)
            {
                /*SkinnedMeshCombiner smc = Controller.GetComponent<SkinnedMeshCombiner>();

                if (smc)
                {
                    if (smc.isActiveAndEnabled)
                        smc.UndoCombineMeshes(false, true);

                    Transform[] elementsRoot = Visual.GetComponentsInParent<Transform>();

                    foreach (var elementRoot in elementsRoot)
                    {
                        if (elementRoot.name == "Combined Meshes")
                            DestroyImmediate(elementRoot.gameObject);
                    }
                }*/
            }                           
        }

        public MaterialVariant GetMaterialType(List<MaterialVariant> materials, MaterialType type)
        {
            for(int i = 0; i< materials.Count; i++)
            {
                if(materials[i].Name == type.ToString())
                {
                    return materials[i];
                }
            }

            return materials[0];
        }

        public void FixBones(RaceElement raceElement)
        {
            Component[] bones = Bones.transform.GetComponentsInChildren(typeof(Transform));
            BoneMapperByRace boneMapperByRace = new BoneMapperByRace();
            Dictionary<string, BoneMapper> boneMapperIndex = new Dictionary<string, BoneMapper>();

            foreach (var boneMapper in BoneMappers)
            {
                if (boneMapper.Race == raceElement.Race && boneMapper.Gender == raceElement.Gender)
                {
                    boneMapperByRace = boneMapper;

                    foreach (var mapper in boneMapperByRace.BoneMappers)
                    {
                        try
                        {
                            boneMapperIndex.Add(mapper.Name, mapper);
                        }
                        catch (Exception e) { }                        
                    }

                    break;
                }
            }

            //EyeAndHeadAnimator eyeAndHeadAnimator = Animator.gameObject.GetComponent<EyeAndHeadAnimator>();

            foreach (var bone in bones)
            {
                BoneMapper mapper;

                if (boneMapperIndex.TryGetValue(bone.name, out mapper))
                {
                    bone.transform.position = mapper.Position;
                    bone.transform.rotation = mapper.Rotation;
                }

                if (
                    raceElement.Gender == GenderType.Female &&
                    (bone.name == "CC_Base_L_RibsTwist" || bone.name == "CC_Base_R_RibsTwist")
                )
                {
                    if (bone.gameObject.GetComponent<DynamicBone>() == null)
                    {
                        DynamicBone dynamicBone = bone.gameObject.AddComponent<DynamicBone>();
                        dynamicBone.m_Root = bone.transform;
                        dynamicBone.m_Damping = 1;
                        dynamicBone.m_Elasticity = 0.1f;
                        dynamicBone.m_Stiffness = 0.9f;
                    }
                    else
                    {
                        DynamicBone dynamicBone = bone.gameObject.GetComponent<DynamicBone>();
                        dynamicBone.enabled = true;
                        dynamicBone.m_Root = bone.transform;
                        dynamicBone.m_Damping = 1;
                        dynamicBone.m_Elasticity = 0.1f;
                        dynamicBone.m_Stiffness = 0.9f;
                    }
                }
                else if(
                    raceElement.Gender == GenderType.Male &&
                    (bone.name == "CC_Base_L_RibsTwist" || bone.name == "CC_Base_R_RibsTwist")
                )
                {
                    if (bone.gameObject.GetComponent<DynamicBone>() != null)
                    {
                        DynamicBone dynamicBone = bone.gameObject.GetComponent<DynamicBone>();
                        dynamicBone.enabled = false;
                    }
                }

                if (bone.name == "CC_Base_L_Eye")
                {
                    if(bone.transform.childCount == 0)
                    {
                        EyesLeft = Instantiate(EyesPrefab, bone.transform, false);
                        EyesLeft.transform.localPosition = new Vector3(0, -0.005f, 0);
                        EyesLeft.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    }
                }

                if (bone.name == "CC_Base_R_Eye")
                {
                    if (bone.transform.childCount == 0)
                    {
                        EyesRight = Instantiate(EyesPrefab, bone.transform, false);
                        EyesRight.transform.localPosition = new Vector3(0, -0.005f, 0);
                        EyesRight.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    }
                }

                switch (bone.name)
                {
                    case "Nose_Ring": bone.transform.position = new Vector3(0, 0.731f, -0.168f); break;
                    case "Ear_Rings": bone.transform.position = new Vector3(0, 0, 0); break;
                    case "CC_Base_L_RibsTwist":
                        switch (raceElement.Race)
                        {
                            case RaceType.Human:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(-0.09564231f, 0.06646942f, 0.106811f) :
                                    new Vector3(-0.1119941f, 0.05629589f, 0.1035706f);
                            break;
                            case RaceType.Elve:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(-0.09633024f, 0.05969938f, 0.1140531f) :
                                    new Vector3(-0.1119941f, 0.05629583f, 0.1035706f);
                            break;
                            case RaceType.Dwarf:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(-0.1068501f, 0.04650855f, 0.1396115f) :
                                    new Vector3(-0.1320673f, 0.03406002f, 0.1337188f);
                            break;
                            case RaceType.Orc:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(-0.1129063f, 0.04244344f, 0.1206402f) :
                                    new Vector3(-0.1425965f, 0.07795986f, 0.1302927f);
                            break;
                        }
                    break;
                    case "CC_Base_R_RibsTwist":
                        switch (raceElement.Race)
                        {
                            case RaceType.Human:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(0.09882656f, 0.06808054f, 0.09929445f) :
                                    new Vector3(0.1099795f, 0.05668342f, 0.1036507f);
                                break;
                            case RaceType.Elve:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(0.09934572f, 0.06143671f, 0.1063554f) :
                                    new Vector3(0.1099795f, 0.05668336f, 0.1036507f);
                                break;
                            case RaceType.Dwarf:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(0.1099871f, 0.04863784f, 0.1313414f) :
                                    new Vector3(0.1293508f, 0.03478146f, 0.1335697f);
                                break;
                            case RaceType.Orc:
                                bone.transform.localPosition =
                                    (raceElement.Gender == GenderType.Female) ?
                                    new Vector3(0.1129827f, 0.04235454f, 0.1206402f) :
                                    new Vector3(0.1422888f, 0.07819416f, 0.1305432f);
                                break;
                        }
                    break;
                }
            }

            //Rebind avatar
            Animator.enabled = false;
            Animator.avatar = raceElement.AnimatorAvatar;
            //Animator.Rebind();
            Animator.enabled = true;
        }

        public void TransferBones(SkinnedMeshRenderer element, GameObject target = null)
        {
            if (target == null)
                target = Bones;

            //Fix Bones
            Transform[] elementBones = element.bones;
            Transform[] targetBones = target.GetComponentsInChildren<Transform>();
            Dictionary<string, Transform> indexBones = GetBonesIndex(targetBones);

            for (int i = 0; i < elementBones.Length; i++)
            {
                try
                {
                    string boneName = elementBones[i].name;
                    bool hasBone = false;

                    if (indexBones.TryGetValue(boneName, out Transform boneRootExists))
                    {
                        elementBones[i] = boneRootExists;
                        hasBone = true;

                        if (boneRootExists.name == element.rootBone.name)
                            element.rootBone = boneRootExists;
                    }

                    if (!hasBone)
                    {
                        GameObject newBone = null;

                        if (indexBones.TryGetValue(elementBones[i].transform.parent.name, out Transform boneRoot))
                        {
                            newBone = Instantiate(elementBones[i].gameObject, boneRoot);
                            newBone.name = elementBones[i].gameObject.name;
                        }                        
                        else if(indexBones.TryGetValue(elementBones[i].transform.parent.parent.name, out Transform boneRootPivot))
                        {
                            newBone = Instantiate(elementBones[i].transform.parent.gameObject, boneRootPivot);
                            newBone.name = elementBones[i].transform.parent.name;
                        }                        
                        else if (indexBones.TryGetValue(elementBones[i].transform.parent.parent.parent.name, out Transform boneRootPivot2))
                        {
                            newBone = Instantiate(elementBones[i].transform.parent.parent.gameObject, boneRootPivot2);
                            newBone.name = elementBones[i].transform.parent.parent.name;
                        }
                        else                        
                            Debug.Log("Bone " + elementBones[i].transform.parent.parent.name + "/" + elementBones[i].transform.parent.name + " /" + elementBones[i].gameObject.name + " dont exits");

                        targetBones = target.GetComponentsInChildren<Transform>();
                        indexBones = GetBonesIndex(targetBones);

                        if (indexBones.TryGetValue(boneName, out Transform newBoneRootExists))
                        {
                            elementBones[i] = newBoneRootExists;
                            hasBone = true;

                            if (newBoneRootExists.name == element.rootBone.name)
                                element.rootBone = newBoneRootExists;
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.Log("Error bone " + element.gameObject.name + " Index: " + i);
                }                
            }

            element.bones = elementBones;
            element.renderingLayerMask = (uint)((int)RenderingLayerMask.DecalLayer1 | (int)RenderingLayerMask.LightLayerDefault); //Fix Lighting
        }

        public Dictionary<string, Transform> GetBonesIndex(Transform[] targetBones)
        {
            Dictionary<string, Transform> indexBones = new Dictionary<string, Transform>();

            for (int i = 0; i < targetBones.Length; i++)
            {
                if (targetBones[i].parent.name == "CC_Base_Pivot" && indexBones.TryGetValue(targetBones[i].name, out Transform boneRef))
                    indexBones[targetBones[i].name] = targetBones[i];
                else if (!indexBones.TryGetValue(targetBones[i].name, out Transform boneRef2))
                    indexBones.Add(targetBones[i].name, targetBones[i]);
            }

            return indexBones;
        }

        public void FixColorAndLighting(SkinnedMeshRenderer element, Color changeColor, RaceElement raceElement, bool IsItem = false)
        {          
            if(IsItem)
                element.renderingLayerMask = (uint)((int)RenderingLayerMask.DecalLayer1 | (int)RenderingLayerMask.LightLayerDefault | (int)RenderingLayerMask.LightLayer1);
            else
                element.renderingLayerMask = (uint)((int)RenderingLayerMask.DecalLayer1 | (int)RenderingLayerMask.LightLayerDefault); //Fix Lighting

            if (changeColor != Color.clear)
            {
                Material[] newMaterials = new Material[element.sharedMaterials.Length];

                for (int i = 0; i < element.sharedMaterials.Length; i++)
                {
                    switch (element.sharedMaterials[i].shader.name)
                    {
                        case "Shader Graphs/RL_HairShaderMultiPass_Variants_HDRP":
                        case "Shader Graphs/RL_HairShaderMultiPass_Variants_HDRP12_Tessellation":
                            Material newMaterial = new Material(Shader.Find(element.sharedMaterials[i].shader.name));
                            newMaterial.CopyPropertiesFromMaterial(element.sharedMaterials[i]);
                            newMaterial.SetColor("_DiffuseColor", changeColor);
                            newMaterials[i] = newMaterial;
                        break;
                        case "HDRP/Lit":

                            Material newMaterialLit = new Material(Shader.Find(element.sharedMaterials[i].shader.name));
                            newMaterialLit.CopyPropertiesFromMaterial(element.sharedMaterials[i]);
                            newMaterialLit.SetColor("_BaseColor", changeColor);
                            newMaterials[i] = newMaterialLit;
                        break;
                        default:
                            Material tmpMaterial = new Material(Shader.Find(element.sharedMaterials[i].shader.name));
                            tmpMaterial.CopyPropertiesFromMaterial(element.sharedMaterials[i]);
                            newMaterials[i] = tmpMaterial;
                        break;
                    }
                }
                                
                element.materials = newMaterials;
                ChangeMaterial(element, raceElement);               
            }
        }

        public void ChangeMaterial(SkinnedMeshRenderer element, RaceElement raceElement)
        {
            Material[] newMaterials = new Material[element.sharedMaterials.Length];

            if (element.sharedMaterials != null)
            {
                if(raceElement.Materials.Count > 0)
                {
                    MaterialVariant materialType = GetMaterialType(raceElement.Materials, MaterialType);

                    for (int i = 0; i < element.sharedMaterials.Length; i++)
                    {
                        switch (element.sharedMaterials[i].name)
                        {
                            case "Std_Skin_Arm": newMaterials[i] = materialType.Arm; break;
                            case "Std_Skin_Body": newMaterials[i] = materialType.Body; break;
                            case "Std_Skin_Head": newMaterials[i] = materialType.Head; break;
                            case "Std_Skin_Leg": newMaterials[i] = materialType.Leg; break;
                            case "Std_Nails": newMaterials[i] = materialType.Nails; break;
                            default: newMaterials[i] = element.sharedMaterials[i]; break;
                        }
                    }

                    element.materials = newMaterials;
                }
            }
        }

        public void ClearVisual()
        {
            if(InUsePrefabs.Count > 0)
            {
                foreach (GameObject element in InUsePrefabs)
                {
                    if(element != null)
                    {
                        element?.gameObject.SetActive(false);
                        MoveToPooling(element.gameObject);
                    }                    
                }
            }

            InUsePrefabs.Clear();

            if (Visual.transform.childCount > 0)
            {
                int totalChilds = Visual.transform.childCount;

                for (int i = 0; i < totalChilds; i++)
                {
                    var element = Visual.transform.GetChild(0);

                    if(element.name == BonesName && Visual.transform.childCount > 1)
                        element = Visual.transform.GetChild(1);

                    element.gameObject.SetActive(false);
                    MoveToPooling(element.gameObject);                  
                }
            }
        }

        public int GetRace(RaceType Race, GenderType Gender)
        {
            for (int i = 0; i < RacesElements.Count; i++)
            {
                if(RacesElements[i].Race == Race && RacesElements[i].Gender == Gender)
                {
                    return i;
                }
            }

            return 0;
        }

        public void AssignBone(GameObject prefab, GameObject Rig)
        {
            var skinned = prefab.GetComponent<SkinnedMeshRenderer>();

            if (skinned && skinned.rootBone)
            {
                string rootBoneName = skinned.rootBone.name;
                GameObject bone = FindChildGameObject(Rig, rootBoneName);

                if (bone)
                    skinned.rootBone = bone.transform;
            }
        }

        private GameObject FindChildGameObject(GameObject topParent, string gameObjectName) 
        { 
            for(int i = 0; i < topParent.transform.childCount; i++)
            {
                if(topParent.transform.GetChild(i).name == gameObjectName)
                {
                    return topParent.transform.GetChild(i).gameObject;
                }

                GameObject tmp = FindChildGameObject(topParent.transform.GetChild(i).gameObject, gameObjectName);

                if (tmp != null)
                    return tmp;
            }

            return null;
        }
        
        public void Assign(HumanoidAvatarControllerWrapper humanoidAvatarController)
        {
            Visual = humanoidAvatarController.Visual;
            Animator = humanoidAvatarController.Animator;
            Controller = humanoidAvatarController.Controller;
            //HumanDescription = humanoidAvatarController.HumanDescription;
            HairsDiretory = humanoidAvatarController.HairsDiretory;
            BeardDiretory = humanoidAvatarController.BeardDiretory;
            //Race = humanoidAvatarController.Race;
            //Gender = humanoidAvatarController.Gender;
            MaterialType = humanoidAvatarController.MaterialType;
            HairColor = humanoidAvatarController.HairColor;
            BeardColor = humanoidAvatarController.BeardColor;
            BodyColor = humanoidAvatarController.BodyColor;
            EyesColor = humanoidAvatarController.EyesColor;
            RacesElements = humanoidAvatarController.RacesElements;
            HairElements = humanoidAvatarController.HairElements;
            BeardElements = humanoidAvatarController.BeardElements;
            Items = humanoidAvatarController.Items;
            //Refresh();
        }

        public Vector3 GetCameraBodyPosition()
        {
            return CameraBodyPosition;
        }

        public Quaternion GetCameraBodyRotation()
        {
            return CameraBodyRotation;
        }

        public Vector3 GetCameraHeadPosition()
        {
            return CameraHeadPosition;
        }

        public Quaternion GetCameraHeadRotation()
        {
            return CameraHeadRotation;
        }

        public void CreatePooling()
        {
            if (Bones != null)
                DestroyImmediate(Bones);

            Bones = null;
            ClearVisual();
            ElementsPooling.Clear();
            PoolingIndex.Clear();            
            ClearPooling();

            foreach (var race in RacesElements)
            {
                AddToPooling(race.FullBody);
                /*AddToPooling(race.FullHands);
                AddToPooling(race.FullLegs);
                AddToPooling(race.Hands);
                AddToPooling(race.Head);
                AddToPooling(race.Legs);
                AddToPooling(race.MiddleHands);
                AddToPooling(race.MiddleLegs);
                AddToPooling(race.Underwear);
                AddToPooling(race.Bust);*/
            }

            foreach(var item in Items)
            {
                //item.Overlay.LoadPrefab();

                if (item.Overlay.PrefabFemale != null)
                    AddToPooling(item.Overlay.PrefabFemale);

                if (item.Overlay.PrefabMale != null)
                    AddToPooling(item.Overlay.PrefabMale);

                if (item.Overlay.PrefabDwarfMale != null)
                    AddToPooling(item.Overlay.PrefabDwarfMale);

                if (item.Overlay.PrefabOrcMale != null)
                    AddToPooling(item.Overlay.PrefabOrcMale);

                if (item.Overlay.PrefabDwarfFemale != null)
                    AddToPooling(item.Overlay.PrefabDwarfFemale);

                if (item.Overlay.PrefabOrcFemale != null)
                    AddToPooling(item.Overlay.PrefabOrcFemale);
            }

            foreach(var hair in HairElements)
            {
                AddToPooling(hair.Prefab);
            }

            foreach (var beard in BeardElements)
            {
                AddToPooling(beard.Prefab);
            }

            //Task.Delay(5).Wait();

            HiddenPooling();
        }

        public void ClearPooling()
        {
            if(Pooling != null)
            {
                int childCount = Pooling.transform.childCount;

                if(childCount > 0)
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        try
                        {
                            Transform element = Pooling.transform.GetChild(0);
                            DestroyImmediate(element.gameObject);
                        }
                        catch (Exception e) { }                        
                    }
                }           
            }
        }

        public void HiddenPooling()
        {
            Component[] elements = Pooling.transform.GetComponentsInChildren(typeof(Transform));

            foreach (var element in elements)
            {
                if (Pooling && Pooling.transform.childCount > 0 && element != null)
                {                    
                    if (element.gameObject.name == BonesName && Bones == null)
                    {
                        SkinnedMeshRenderer smr = element.gameObject.GetComponent<SkinnedMeshRenderer>();

                        if (smr != null)
                            smr.renderingLayerMask = (uint)((int)RenderingLayerMask.DecalLayer1 | (int)RenderingLayerMask.LightLayerDefault);

                        Bones = element.gameObject;
                        Bones.name = BonesName;
                        element.gameObject.transform.SetParent(Visual.transform.parent);
                    }
                    else if (Bones != null && !IgnoreElements.Contains(element.gameObject.name))
                    {
                        SkinnedMeshRenderer smr = element.gameObject.GetComponent<SkinnedMeshRenderer>();

                        if (smr != null)
                            TransferBones(smr);                                         
                    }

                    if (element.gameObject.name == "CC_Base_L_Hand" && LeftHandTransform == null)
                        LeftHandTransform = element.transform;

                    if (element.gameObject.name == "CC_Base_R_Hand" && RightHandTransform == null)
                        RightHandTransform = element.transform;

                    if (element.gameObject.name == "CC_Base_NeckTwist02" && BackSpineTransform == null)
                        BackSpineTransform = element.transform;

                    if (element.gameObject.name == "CC_Base_Head" && HeadTransform == null)                    
                        HeadTransform = element.transform;
                    
                }
            }

            if (Pooling != null)
            {
                int childCount = Pooling.transform.childCount;

                if (childCount > 0)
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        Transform element = Pooling.transform.GetChild(i);

                        if (element.name != "Pooling")
                        {
                            int subElements = element.transform.childCount;

                            for(int j = 0; j < subElements; j++)
                            {
                                try
                                {
                                    var subsElement = element.transform.GetChild(j);

                                    if (subsElement.name == BonesName)
                                    {
                                        DestroyImmediate(subsElement.gameObject);
                                    }
                                }
                                catch (Exception e) { }                                
                            }

                            element.gameObject.SetActive(false);
                        }                            
                    }
                }
            }
        }

        public void AddToPooling(string prefabPath, bool instantiatePrefab = true)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
                AddToPooling(prefab, true);
        }

        public void AddToPooling(GameObject prefab, bool instantiatePrefab = true)
        {
            if(prefab != null)
            {
                try
                {
                    var element = (instantiatePrefab) ? Instantiate(prefab, Pooling.transform, false) : prefab;

                    if (element != null)
                    {                        
                        element.name = prefab.name;

                        if (PoolingIndex.ContainsKey(prefab.name))
                        {
                            int Index = -1;

                            if(PoolingIndex.TryGetValue(prefab.name, out Index))
                                ElementsPooling[Index] = element;
                        }
                        else
                        {
                            ElementsPooling.Add(element);
                            PoolingIndex.Add(prefab.name, ElementsPooling.Count - 1);
                        }
                        
                    }
                }
                catch (Exception e) { }
            }            
        }

        public async Task<GameObject> GetInPooling(string name, Transform parent, bool worldSpace, GameObject prefab)
        {
            GameObject returnObject = null;

            

            if(PoolingIndex.TryGetValue(name, out int index))
            {
                returnObject = ElementsPooling[index];
                ElementsPooling[index].transform.SetParent(parent);
            }

            if(returnObject == null)
            {
                GameObject pooledObject = Instantiate(prefab, parent, worldSpace);
                pooledObject.gameObject.name = name;

                Component[] elements = pooledObject.transform.GetComponentsInChildren(typeof(Transform));

                foreach (var element in elements)
                {
                    SkinnedMeshRenderer smr = element.gameObject.GetComponent<SkinnedMeshRenderer>();

                    if (smr != null)
                        TransferBones(smr);
                }

                int childCount = pooledObject.transform.childCount;

                if (childCount > 0)
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        Transform element = pooledObject.transform.GetChild(i);

                        if (element.name == BonesName)
                        {
                            DestroyImmediate(element.gameObject);
                            break;
                        }                        
                    }
                }

                AddToPooling(pooledObject, false);

                returnObject = pooledObject;
            }

            return returnObject;
        }

        public void MoveToPooling(GameObject prefab, bool moveParent = true)
        {
            if(moveParent)
                prefab.transform.SetParent(Pooling.transform);

            if (PoolingIndex.ContainsKey(name))
            {
                int Index = -1;

                if(PoolingIndex.TryGetValue(name, out Index))
                    ElementsPooling[Index] = prefab;
            }
            else
            {
                AddToPooling(prefab, false);
            }
        }

        public void ChangeHair(int Index)
        {
            model.hair.Value = Index;
            Refresh();
        }

        public void ClearItems()
        {
            foreach(ItemAvatar item in Items)
            {
                item.Active = false;
            }
        }

        public void ClearAll()
        {
            model.race.Value = RaceType.Human;
            model.gender.Value = GenderType.Male;
            model.hair.Value = -1;
            model.beard.Value = -1;
            model.hairColor.Value = Color.black;
            model.beardColor.Value = Color.black;
            ClearItems();
        }

        public void ActiveItem(string Namespace)
        {
            foreach (ItemAvatar item in Items)
            { 
                if(item.Namespace == Namespace)
                {
                    item.Active = true;
                    break;
                }
            }
        }

        public void DesativeItemByType(ItemAvatarType type)
        {
            foreach (ItemAvatar item in Items)
            {
                if (item.Overlay.Type == type && item.Active)
                {
                    item.Active = false;
                }
            }
        }

        public void ActiveDefaultSet()
        {
            ActiveItem("Cloth_Boots");
            ActiveItem("Cloth_Chest");
            ActiveItem("Cloth_Pants");
        }

#if UNITY_EDITOR
        public void Save()
        {
            string directory = EditorUtility.OpenFolderPanel("Select Directory", "/Assets/_ToS/Player", "");
            //SerializationUtility.Save(this, directory + "/Avatar.json");
        }

        public void Load()
        {
            string file = EditorUtility.OpenFilePanel("Select Directory", "/Assets/_ToS/Player", "json");
            //this.Assign(SerializationUtility.Load<HumanoidAvatarControllerWrapper>(file));
        }

        public void MapperResources()
        {
            //Races
            RacesElements.Clear();
            string fullDirRaces = Application.dataPath + RacesDiretory;
            string[] sDirectoriesRaces = Directory.GetDirectories(fullDirRaces);

            foreach (var directory in sDirectoriesRaces)
            {
                GetRace(directory, RacesElements);
            }

            //Hair
            HairElements.Clear();
            string fullDirHairs = Application.dataPath + HairsDiretory;
            string[] sDirectories = Directory.GetDirectories(fullDirHairs);

            foreach (var directory in sDirectories)
            {
                GetPrefabs(directory, HairElements);
            }

            //Beard
            BeardElements.Clear();
            string fullDirBeard = Application.dataPath + BeardDiretory;
            string[] sDirectoriesBeard = Directory.GetDirectories(fullDirBeard);

            foreach (var directory in sDirectoriesBeard)
            {
                GetPrefabs(directory, BeardElements);
            }

            //Items
            Items.Clear();
            string[] Subdirs = { "Plates", "Cloths", "Leathers", "Others", "Robe", "NPC" };

            foreach(string subdir in Subdirs)
            {
                string fullDirItems = Application.dataPath + "/_ToS/Player/" + subdir;
                string[] sDirectoriesItems = Directory.GetDirectories(fullDirItems);

                foreach (var directory in sDirectoriesItems)
                {
                    if(Directory.Exists(directory + "/Overlays"))
                    {
                        GetPrefabsItems(directory + "/Overlays", Items);                        
                    }                    
                }
            }
        }

        public void GetRace(string directory, List<RaceElement> tmpList)
        {
            string[] sGenderDirs = Directory.GetDirectories(directory);

            foreach (var genderDirectory in sGenderDirs)
            {
                if (Directory.Exists(genderDirectory))
                {
                    string[] sSubDirs = Directory.GetDirectories(genderDirectory);
                    RaceElement tmpElement = new RaceElement();

                    foreach (var subDirectory in sSubDirs)
                    {
                        string[] aFilePaths = { };

                        try
                        {
                            aFilePaths = Directory.GetFiles(subDirectory + "/Prefabs", "*.prefab");
                        }
                        catch(Exception e){ }
                        
                        if (aFilePaths.Length <= 0)                      
                            aFilePaths = Directory.GetFiles(subDirectory, "*.prefab");
                        
                        if (aFilePaths.Length > 0)
                        {
                            string[] splitName = Path.GetFileName(aFilePaths[0].Replace(".prefab", "")).Split("_");
                            tmpElement.Namespace = splitName[0] + "_" + splitName[1];

                            switch (splitName[0])
                            {
                                case "E": tmpElement.Race = RaceType.Elve; break;
                                case "DV": tmpElement.Race = RaceType.Devas; break;
                                case "D": tmpElement.Race = RaceType.Dwarf; break;
                                case "H": tmpElement.Race = RaceType.Human; break;
                                case "O": tmpElement.Race = RaceType.Orc; break;
                            }

                            switch (splitName[1])
                            {
                                case "M": tmpElement.Gender = GenderType.Male; break;
                                case "F": tmpElement.Gender = GenderType.Female; break;
                            }

                            switch (splitName[2])
                            {
                                case "Fullbody": 
                                    tmpElement.FullBody = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), "");

                                    //Avatar
                                    if(File.Exists(subDirectory + "/Meshes/" + Path.GetFileName(aFilePaths[0]).Replace(".prefab", ".Fbx")))
                                    {
                                        string filename = "Assets" + subDirectory.Replace(Application.dataPath, "") + "/Meshes/" + Path.GetFileName(aFilePaths[0]).Replace(".prefab", ".Fbx");
                                        var avatar = AssetDatabase
                                             .LoadAllAssetsAtPath(filename)
                                             .Where(x => x is Avatar)
                                             .OfType<Avatar>()
                                             .FirstOrDefault();

                                        if (avatar != null)
                                            tmpElement.AnimatorAvatar = avatar;
                                    }
                                break;
                                case "HandFull": 
                                    tmpElement.FullHands = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "HandMiddle": 
                                    tmpElement.MiddleHands = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "HandMin": 
                                    tmpElement.Hands = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "Head": 
                                    tmpElement.Head = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "LegsFull": 
                                    tmpElement.FullLegs = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "LegsMiddle": 
                                    tmpElement.MiddleLegs = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "LegsMin": 
                                    tmpElement.Legs = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "Bust": 
                                    tmpElement.Bust = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                                case "Underwear": 
                                    tmpElement.Underwear = aFilePaths[0].Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                        .Replace(Path.GetExtension(aFilePaths[0]), ""); 
                                break;
                            }
                        }
                    }

                    tmpList.Add(tmpElement);
                }
            }
        }

        public List<RecourceAvatar> GetPrefabs(string directory, List<RecourceAvatar> tmpList)
        {
            string[] sSubDirs = Directory.GetDirectories(directory);

            foreach (var subDirectory in sSubDirs)
            {
                if(Directory.Exists(subDirectory + "/Prefabs"))
                {
                    string[] aFilePaths = Directory.GetFiles(subDirectory + "/Prefabs", "*.prefab");

                    foreach (string sFilePath in aFilePaths)
                    {
                        if (Path.GetExtension(sFilePath) == ".prefab" || Path.GetExtension(sFilePath) == ".PREFAB")
                        {
                            string[] splitName = Path.GetFileName(sFilePath).Split("_");

                            RecourceAvatar tmpElement = new RecourceAvatar();
                            tmpElement.Namespace = Path.GetFileName(sFilePath).Replace(Path.GetExtension(sFilePath), "").Replace(".prefab", "");
                            tmpElement.Prefab = sFilePath.Replace(Application.dataPath, "").Replace("/_ToS/Resources/", "").Replace("\\", "/")
                                .Replace(Path.GetExtension(sFilePath), "");

                            if(File.Exists(subDirectory + "/Icon.jpg"))
                            {
                                string iconFile = "Assets" + subDirectory.Replace(Application.dataPath, "") + "/Icon.jpg";
                                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFile);
                                tmpElement.Icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f); 
                            }

                            switch (splitName[0])
                            {
                                case "E": tmpElement.Race = RaceType.Elve; break;
                                case "DV": tmpElement.Race = RaceType.Devas; break;
                                case "D": tmpElement.Race = RaceType.Dwarf; break;
                                case "H": tmpElement.Race = RaceType.Human; break;
                                case "O": tmpElement.Race = RaceType.Orc; break;
                            }

                            switch (splitName[1])
                            {
                                case "M": tmpElement.Gender = GenderType.Male; break;
                                case "F": tmpElement.Gender = GenderType.Female; break;
                            }

                            tmpList.Add(tmpElement);
                        }                        
                    }
                }
            }

            return tmpList;
        }

        public List<ItemAvatar> GetPrefabsItems(string directory, List<ItemAvatar> tmpList)
        {
            string[] aFilePaths = Directory.GetFiles(directory, "*.asset");
            
            foreach (string sFilePath in aFilePaths)
            {
                if (Path.GetExtension(sFilePath) == ".asset" || Path.GetExtension(sFilePath) == ".asset")
                {
                    ItemAvatar tmpElement = new ItemAvatar();
                    UnityEngine.Object overlayObject = AssetDatabase.LoadAssetAtPath("Assets" + sFilePath.Replace(Application.dataPath, ""), typeof(ItemOverlay));
                    ItemOverlay overlay = overlayObject as ItemOverlay;
                    string splitName = Path.GetFileName(sFilePath).Replace(".asset", "");
                
                    tmpElement.Namespace = splitName;
                    tmpElement.Overlay = overlay;
                    tmpList.Add(tmpElement);
                }
            }

            return tmpList;
        }

        public void RecalculateBones()
        {
            BoneMappers.Clear();

            foreach (var race in RacesElements)
            {
                GameObject prefabRef = Resources.Load<GameObject>(race.FullBody);
                var prefab = Instantiate(prefabRef);
                Transform[] elements = prefab.GetComponentsInChildren<Transform>();

                BoneMapperByRace boneMapperByRace = new BoneMapperByRace();
                boneMapperByRace.Namespace = race.Race.ToString() + "_" + race.Gender.ToString();
                boneMapperByRace.BoneMappers = new List<BoneMapper>();
                boneMapperByRace.Race = race.Race;
                boneMapperByRace.Gender = race.Gender;

                foreach (var element in elements)
                {
                    if(element.name == BonesName)
                    {
                        Transform[] bones = element.GetComponentsInChildren<Transform>();

                        if (bones.Length > 0)
                        {
                            foreach(var bone in bones)
                            {
                                BoneMapper boneMapper = new BoneMapper();
                                boneMapper.Name = bone.name;                                
                                boneMapper.Position = bone.transform.position;
                                boneMapper.Rotation = bone.transform.rotation;
                                boneMapperByRace.BoneMappers.Add(boneMapper);
                            }
                        }

                        break;
                    }
                }

                BoneMappers.Add(boneMapperByRace);
                DestroyImmediate(prefab);
            }
        }

        public void ClearTextures()
        {
            //Races
            /*string fullDirRaces = Application.dataPath + RacesDiretory;
            string[] sDirectoriesRaces = Directory.GetDirectories(fullDirRaces);

            foreach (var directory in sDirectoriesRaces)
            {
                RemoveTextures(directory);
            }

            //Hair
            string fullDirHairs = Application.dataPath + HairsDiretory;
            string[] sDirectories = Directory.GetDirectories(fullDirHairs);

            foreach (var directory in sDirectories)
            {
                RemoveTexturesInHairAndBeard(directory);
            }

            //Beard
            string fullDirBeard = Application.dataPath + BeardDiretory;
            string[] sDirectoriesBeard = Directory.GetDirectories(fullDirBeard);

            foreach (var directory in sDirectoriesBeard)
            {
                RemoveTexturesInHairAndBeard(directory);
            }

            AssetDatabase.Refresh();*/
        }

        public void RemoveTexturesInHairAndBeard(string directory)
        {
            string[] sSubDirs = Directory.GetDirectories(directory);

            foreach (var subDirectory in sSubDirs)
            {
                if (Directory.Exists(subDirectory + "/textures"))
                {
                    string[] aFilePaths = Directory.GetFiles(subDirectory + "/textures", "*.*");

                    foreach (string sFilePath in aFilePaths)
                    {
                        //Debug.Log(Path.GetFileName(sFilePath).Replace(Path.GetExtension(sFilePath), ""));

                        if(TrashTextures.Contains(Path.GetFileName(sFilePath).Replace(Path.GetExtension(sFilePath), "")))
                        {
                            File.Delete(sFilePath);
                        }
                    }

                    //Remove subdiretories
                    string[] aSubDirsTextures = Directory.GetDirectories(subDirectory + "/textures");

                    foreach (var subDirectoryTextures in aSubDirsTextures)
                    {
                        string[] aSubDirsTexturesDirs = Directory.GetDirectories(subDirectoryTextures);

                        foreach (var subDirectoryTexturesDir in aSubDirsTexturesDirs)
                        {
                            string[] aSubDirsTexturesDirsSub2 = Directory.GetDirectories(subDirectoryTexturesDir);

                            foreach (var subDirectoryTexturesDir2 in aSubDirsTexturesDirsSub2)
                            {
                                //Debug.Log(Path.GetFileName(Path.GetDirectoryName(subDirectoryTexturesDir2)));

                                if (TrashDiretories.Contains(Path.GetFileName(Path.GetDirectoryName(subDirectoryTexturesDir2))))
                                {
                                    Directory.Delete(subDirectoryTexturesDir2, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreatePrefab()
        {
            RaceElement raceElement = (RacesElements.Count > RaceIndex) ? RacesElements[RaceIndex] : RacesElements[0];

            GameObject gb = new GameObject();
            gb.name = "Exported Prefab";
            
            GameObject boneRig = Instantiate(Bones, gb.transform);
            boneRig.name = "CC_Base_BoneRoot";

            Animator animatorGb = gb.AddComponent<Animator>();
            animatorGb.enabled = false;
            animatorGb.avatar = raceElement.AnimatorAvatar;
            animatorGb.enabled = true;

            int elementsCount = Visual.transform.childCount;

            for(int i = 0; i < elementsCount; i++)
            {
                GameObject part = Visual.transform.GetChild(i).gameObject;
                GameObject inPart = Instantiate(part, gb.transform);
                inPart.name = part.name;
                SkinnedMeshRenderer[] skinMeshs = inPart.GetComponentsInChildren<SkinnedMeshRenderer>();

                foreach (var skin in skinMeshs)
                    TransferBones(skin, boneRig);
            }
        }
#endif
    }
}