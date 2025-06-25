using System.Collections;
using System.Collections.Generic;
using GLTFast;
using UnityEngine;
using UnityEngine.Events;

public class PostEventGltfAsset : GLTFast.GltfAsset
{
    private UnityAction<bool> m_PostEvent;
    public UnityAction<bool> PostEvent
    {
        get => m_PostEvent;
        set => m_PostEvent = value;
    }

    protected void Awake()
    {
        ImportSettings = new ImportSettings();
        ImportSettings.NodeNameMethod = NameImportMethod.OriginalUnique;
    }

    protected override void PostInstantiation(IInstantiator instantiator, bool success)
    {
        base.PostInstantiation(instantiator, success);
        m_PostEvent?.Invoke(success);
    }
}
