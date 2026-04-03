using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GLTFast;
using Unity.Jobs;
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

    protected override void OnDestroy()
    {
        // GLTFast의 GltfImport.Dispose()는 pending Job이 완료되지 않은 상태에서
        // NativeArray를 해제하려고 시도하여 InvalidOperationException이 발생할 수 있음.
        // Dispose 전에 pending accessor jobs를 강제 완료시켜 이를 방지.
        if (Importer != null)
        {
            CompleteImporterJobs(Importer);
        }

        base.OnDestroy();
    }

    private static void CompleteImporterJobs(GltfImport importer)
    {
        try
        {
            var accessorField = typeof(GltfImport).GetField("m_AccessorJobsHandle",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (accessorField != null)
            {
                var handle = (JobHandle)accessorField.GetValue(importer);
                handle.Complete();
            }

            var meshoptField = typeof(GltfImport).GetField("m_MeshoptJobHandle",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (meshoptField != null)
            {
                var handle = (JobHandle)meshoptField.GetValue(importer);
                handle.Complete();
            }
        }
        catch (System.Exception)
        {
            // Reflection 실패 시 무시 (GLTFast 버전 차이 등)
        }
    }
}
