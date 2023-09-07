using System;
using System.Collections.Generic;
using UnityEngine;
using ToS.Model;
using Shared;

namespace ToS
{
    [Serializable]
    public class HumanoidAvatarControllerWrapper
    {
        public GameObject Visual;
        public Animator Animator;
        public GameObject Controller;

        public string HairsDiretory;
        public string BeardDiretory;

        public RaceType Race = RaceType.Human;
        public GenderType Gender = GenderType.Male;
        public MaterialType MaterialType = MaterialType.Base;
        public Color HairColor;
        public Color BeardColor;
        public Color BodyColor;
        public Color EyesColor;

        public List<RaceElement> RacesElements = new List<RaceElement>();
        public List<RecourceAvatar> HairElements = new List<RecourceAvatar>();
        public List<RecourceAvatar> BeardElements = new List<RecourceAvatar>();
        public List<ItemAvatar> Items = new List<ItemAvatar>();
    }
}
