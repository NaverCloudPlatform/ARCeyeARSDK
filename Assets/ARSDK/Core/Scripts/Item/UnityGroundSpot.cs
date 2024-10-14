using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityGroundSpot : UnityModel
    {
        public override void Initialize(PostEventGltfAsset model)
        {
            base.Initialize(model);
            SetOpacity(0);
        }

        public override void PlayAnimation(string animName, string playModeStr)
        {
            SetOpacity(1);
            base.PlayAnimation(animName, playModeStr);
        }

        public override void SetActive(bool value)
        {
            SetOpacity(value ? 1 : 0);
        }
    }
}