using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Rebone : MonoBehaviour
{
    public SkinnedMeshRenderer basePrefab;
    public SkinnedMeshRenderer itemPrefab;

#if UNITY_EDITOR
    [CustomEditor(typeof(Rebone))]
    public class ReboneInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Rebone rebone = (Rebone)target;

            if (GUILayout.Button("Recalculate Bones"))
            {
                rebone.Recalculate();
            }
        }
    }
#endif

    void Start()
    {
        
    }

#if UNITY_EDITOR
    public void Recalculate()
    {
        if(basePrefab != null && itemPrefab != null)
        {
            TransferBones();
        }
    }

    public void TransferBones()
    {
        //Fix Bones
        Transform[] elementBones = basePrefab.bones;
        Transform[] targetBones = itemPrefab.bones;

        for (int i = 0; i < elementBones.Length; i++)
        {
            try
            {
                string boneName = elementBones[i].name;

                for (int j = 0; j < targetBones.Length; j++)
                {
                    if (targetBones[j].name == boneName)
                        targetBones[j] = elementBones[i];

                    if (itemPrefab.rootBone.name == boneName)
                        itemPrefab.rootBone = elementBones[i];
                }
            }
            catch (Exception e)
            {
                Debug.Log("Invalid bone " + basePrefab.gameObject.name + " Index: " + i);
            }
        }

        itemPrefab.bones = elementBones;
    }
#endif

}
