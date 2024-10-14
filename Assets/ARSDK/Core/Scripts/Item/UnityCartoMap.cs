using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public class UnityCartoMap : UnityModel
    {
        /* -- Native plugin -- */
        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        public delegate void AssignNativeHandleDelegate(IntPtr gameObjectHandle, IntPtr nativeHandle);

        [DllImport(dll)] private static extern void SetAssignNativeHandleFuncNative(AssignNativeHandleDelegate func);
        [DllImport(dll)] private static extern void TapCartoMapNative(IntPtr nativeHandle);


        private Transform m_CameraTransform;

        /// CartoMap의 glb 로딩이 완료 되었는지 확인하는 플래그.
        private bool m_IsLoaded = false;
        public bool isLoaded => m_IsLoaded;

        private IntPtr m_NativeInstance;



        private void Awake()
        {
            SetAssignNativeHandleFuncNative( AssignNativeHandle );
        }

        protected override void FindMaterials()
        {
            // UnityCartoMap에서는 Shader를 변경하는 작업을 수행하지 않는다.
        }

        [MonoPInvokeCallback(typeof(AssignNativeHandleDelegate))]
        static private void AssignNativeHandle(IntPtr gameObjectHandle, IntPtr nativeHandle)
        {
            // goHandle을 UnityCartoMap 인스턴스로 Unwrap
            object obj = GCHandle.FromIntPtr(gameObjectHandle).Target;
            UnityCartoMap cartoMap = ((GameObject) obj).GetComponent<UnityCartoMap>();
            cartoMap.m_NativeInstance = nativeHandle;
        }


        public override void Initialize(PostEventGltfAsset model)
        {
            base.Initialize(model);

            var children = model.gameObject.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Map");
            }

            SetOpacity(1);

            gameObject.name = "UnityCartoMap";

            m_IsLoaded = true;
        }

        private void Start()
        {
            m_CameraTransform = Camera.main.transform;
        }

        public void Touch()
        {
            TapCartoMapNative(m_NativeInstance);
        }
    }
}