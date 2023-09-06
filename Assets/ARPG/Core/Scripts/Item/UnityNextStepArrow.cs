using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    public class UnityNextStepArrow : UnityModel
    {
        public override void Initialize(PostEventGltfAsset model)
        {
            base.Initialize(model);

            var children = model.gameObject.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("UI");
            }
        }

        public override void SetActive(bool value)
        {
            SetOpacity(value ? 1 : 0);
        }
    }
}