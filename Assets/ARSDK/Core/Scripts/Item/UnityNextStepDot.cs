using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityNextStepDot : UnityModel
    {
        public override void Initialize(PostEventGltfAsset model)
        {
            base.Initialize(model);
        }
        
        public override void SetActive(bool value)
        {
            SetOpacity(value ? 1 : 0);
        }
    }
}