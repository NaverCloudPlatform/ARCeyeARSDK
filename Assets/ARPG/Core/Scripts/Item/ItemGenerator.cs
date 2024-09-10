using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace ARCeye
{
    public class ItemGenerator : MonoBehaviour
    {   
        /* -- Native plugin -- */
        #if UNITY_IOS && !UNITY_EDITOR
            const string dll = "__Internal";
        #else
            const string dll = "ARPG-plugin";
        #endif

        [DllImport(dll)] private static extern void SetCreateFuncNative(CreateFuncDelegate func);
        [DllImport(dll)] private static extern void SetLoadModelFuncNative(LoadModelFuncDelegate func);
        [DllImport(dll)] private static extern void SetUnloadModelFuncNative(UnloadModelFuncDelegate func);
        [DllImport(dll)] private static extern void SetSetNameFuncNative(SetNameFuncDelegate func);
        [DllImport(dll)] private static extern void SetSetActiveFuncNative(SetActiveFuncDelegate func);
        [DllImport(dll)] private static extern void SetSetPickableFuncNative(SetSetPickableFuncDelegate func);
        [DllImport(dll)] private static extern void SetSetMatrixModelFuncNative(SetMatrixModelFuncDelegate func);
        [DllImport(dll)] private static extern void SetPlayAnimationFuncNative(SetPlayAnimationFuncDelegate func);
        [DllImport(dll)] private static extern void SetHasAnimationFuncNative(SetHasAnimationFuncDelegate func);
        [DllImport(dll)] private static extern float SetAnimationDurationFuncNative(SetAnimationDurationFuncDelegate func);
        [DllImport(dll)] private static extern float SetFadeFuncNative(SetFadeFuncDelegate func);
        [DllImport(dll)] private static extern float SetFadeCallbackFuncNative(SetFadeCallbackFuncDelegate func);
        [DllImport(dll)] private static extern float SetSetOpacityFuncNative(SetSetOpacityFuncDelegate func);
        [DllImport(dll)] private static extern float SetSetBillboardFuncNative(VB_Func func);

        [DllImport(dll)] private static extern float OnLoadingCompleteNative(IntPtr nativeModelPtr);
        [DllImport(dll)] private static extern float OnFadingCompleteNative(IntPtr nativeModelPtr);

        [DllImport(dll)] private static extern void SetSetRouteNative(VFpI_Func func);
        [DllImport(dll)] private static extern void SetUnloadPathNative(V_Func func);


        // UnityTurnSpot
        [DllImport(dll)] private static extern void SetSetTurnSpotLabelFuncNative(SetNameFuncDelegate func);


        // UnityMapPOIPool
        [DllImport(dll)] private static extern void SetSetFontSizeFuncNative(VF_Func func);
        [DllImport(dll)] private static extern void SetSetOutlineThickness(VF_Func func);
        [DllImport(dll)] private static extern void SetInsertPOIEntityFuncNative(VIISFFFI_Func func);
        [DllImport(dll)] private static extern void SetRemoveNodeFuncNative(V_Func func);
        [DllImport(dll)] private static extern void SetSetConfigFullpathFuncNative(S_Func func);


        // UnitySignPOI
        [DllImport(dll)] private static extern void SetSignPOISetTypeFuncNative(VI_Func func);
        [DllImport(dll)] private static extern void SetSignPOIHasCodeFuncNative(HasCodeFunc func);
        [DllImport(dll)] private static extern void SetSignPOIGetDefaultCodeFuncNative(GetDefaultIconCodeFunc func);
        [DllImport(dll)] private static extern void SetSignPOISetIconCodeFuncNative(SetIconCodeFunc func);
        [DllImport(dll)] private static extern void SetSignPOISetLabelFuncNative(SetLabelFunc func);
        [DllImport(dll)] private static extern void SetSignPOISetAutoRotateModeFuncNative(SetAutoRotateModeFunc func);


        // UnityInfoPanel
        [DllImport(dll)] private static extern void SetInfoPanelTypeFuncNative(VI_Func func);
        [DllImport(dll)] private static extern void SetUseFrameFuncNative(VB_Func func);
        [DllImport(dll)] private static extern void SetUseRoundedCornerFuncNative(VB_Func func);
        [DllImport(dll)] private static extern void SetSetInfoPanelTextFuncNative(VS_Func func);
        [DllImport(dll)] private static extern void SetSetInfoPanelImageFuncNative(VS_Func func);


        public delegate IntPtr CreateFuncDelegate(IntPtr className, IntPtr nativeModelPtr);
        public delegate IntPtr LoadModelFuncDelegate(IntPtr filePath, IntPtr className, IntPtr nativeModelPtr);
        public delegate void UnloadModelFuncDelegate(IntPtr itemPtr);
        public delegate void SetNameFuncDelegate(IntPtr itemPtr, IntPtr namePtr);
        public delegate void SetActiveFuncDelegate(IntPtr itemPtr, bool active);
        public delegate void SetSetPickableFuncDelegate(IntPtr itemPtr, bool pickable);
        public delegate void SetMatrixModelFuncDelegate(IntPtr itemPtr, IntPtr modelMatrix);
        public delegate void SetPlayAnimationFuncDelegate(IntPtr itemPtr, IntPtr animNamePtr, IntPtr playModePtr);
        public delegate bool SetHasAnimationFuncDelegate(IntPtr itemPtr, IntPtr animNamePtr);
        public delegate float SetAnimationDurationFuncDelegate(IntPtr itemPtr, IntPtr animNamePtr);
        public delegate void SetFadeFuncDelegate(IntPtr itemPtr, float duration, bool fadeIn);
        public delegate void SetFadeCallbackFuncDelegate(IntPtr itemPtr, IntPtr nativeModelPtr, float duration, bool fadeIn);
        public delegate void SetSetOpacityFuncDelegate(IntPtr itemPtr, float opacity);


        public delegate void V_Func(IntPtr vp);
        public delegate void VI_Func(IntPtr vp, int i);
        public delegate void VF_Func(IntPtr vp, float f);
        public delegate void VS_Func(IntPtr vp, IntPtr str);
        public delegate void S_Func(IntPtr p);
        public delegate void VB_Func(IntPtr itemPtr, bool active);
        public delegate void VFpI_Func(IntPtr vp, IntPtr fp, int i);
        public delegate void VIISFFFI_Func(IntPtr vp, int i1, int i2, IntPtr ptr1, float f1, float f2, float f3, int i3);

        public delegate bool HasCodeFunc(int i);
        public delegate int  GetDefaultIconCodeFunc(IntPtr vp);
        public delegate void SetIconCodeFunc(IntPtr vp, int i);
        public delegate void SetLabelFunc(IntPtr vp, IntPtr str);
        public delegate void SetAutoRotateModeFunc(IntPtr vp, int i);


        private static ItemGenerator s_Instance;
        public  static ItemGenerator Instance => s_Instance;

        private static POIGenerator s_POIGenerator;
        private static InfoPanelGenerator s_InfoPanelGenerator;
        private static MainThreadLoadingHandler s_MainThreadLoadingHandler;


        [Header("Scene")]
        [SerializeField]
        private Transform m_Scene;

        [Header("Style")]
        [SerializeField]
        private Font m_Font;
        public  Font font => m_Font;
        [SerializeField]
        private Material m_PathMaterial;
        [SerializeField]
        private GameObject m_MapPathBulletBegin;
        [SerializeField]
        private GameObject m_MapPathBulletEnd;
        

        private Material m_POITextMaterial;
        public  Material poiTextMaterial => m_POITextMaterial;
        private Material m_InfoPanelTextMaterial;
        public  Material infoPanelTextMaterial => m_InfoPanelTextMaterial;



        void Awake()
        {
            s_Instance = this;
            s_POIGenerator = GetComponent<POIGenerator>();
            s_InfoPanelGenerator = GetComponent<InfoPanelGenerator>();

            s_MainThreadLoadingHandler = new MainThreadLoadingHandler();

            InitScene();

            CreateMaterials();

            SetCreateFuncNative(Create);
            SetLoadModelFuncNative(LoadModel);
            SetUnloadModelFuncNative(UnloadModel);
            SetSetNameFuncNative(SetName);
            SetSetActiveFuncNative(SetActive);
            SetSetPickableFuncNative(SetPickable);
            SetSetMatrixModelFuncNative(SetMatrixModel);
            SetPlayAnimationFuncNative(PlayAnimation);
            SetHasAnimationFuncNative(HasAnimation);
            SetAnimationDurationFuncNative(AnimationDuration);
            SetFadeFuncNative(Fade);
            SetSetOpacityFuncNative(SetOpacity);

            // UnityTurnSpot
            SetFadeCallbackFuncNative(FadeCallback);
            SetSetBillboardFuncNative(SetBillboard);
            SetSetTurnSpotLabelFuncNative(SetTurnSpotLabel);

            // UnityMapPathIndicator
            SetSetRouteNative(SetRoute);
            SetUnloadPathNative(UnloadPath);

            // UnityMapPOIPool
            SetSetFontSizeFuncNative(SetFontSize);
            SetSetOutlineThickness(SetOutlineThickness);
            SetInsertPOIEntityFuncNative(InsertPOIEntity);
            SetRemoveNodeFuncNative(RemoveNode);
            SetSetConfigFullpathFuncNative(SetConfigFullpath);

            // UnitySignPOI
            SetSignPOISetTypeFuncNative(SetType);
            SetSignPOIHasCodeFuncNative(HasCode);
            SetSignPOIGetDefaultCodeFuncNative(GetDefaultIconCode);
            SetSignPOISetIconCodeFuncNative(SetIconCode);
            SetSignPOISetLabelFuncNative(SetLabel);
            SetSignPOISetAutoRotateModeFuncNative(SetAutoRotateMode);

            // UnityInfoPanel
            SetInfoPanelTypeFuncNative(SetInfoPanelType);
            SetUseFrameFuncNative(UseFrame);
            SetUseRoundedCornerFuncNative(UseRoundedCorner);
            SetSetInfoPanelTextFuncNative(SetInfoPanelText);
            SetSetInfoPanelImageFuncNative(SetInfoPanelImage);
        }

        private void InitScene()
        {
            if(m_Scene != null)
            {
                m_Scene.position = Vector3.zero;
                m_Scene.rotation = Quaternion.identity;
                m_Scene.localScale = Vector3.one;
            }
        }

        private void CreateMaterials()
        {
            Shader poiTextShader = FindShader("ARPG/POI Text");
            Shader infoPanelTextShader = FindShader("ARPG/InfoPanel Text");

            m_POITextMaterial = new Material(poiTextShader);
            m_InfoPanelTextMaterial = new Material(infoPanelTextShader);
        }

        private Shader FindShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);

            if (shader == null)
            {
                NativeLogger.Print(LogLevel.ERROR, $"[ARPlayGround] Fail to find '{shaderName}' shader");
                return null;
            }

            return shader;
        }

        [MonoPInvokeCallback(typeof(CreateFuncDelegate))]
        static public IntPtr Create(IntPtr classNamePtr, IntPtr nativeModelPtr)
        {
            // className과 일치하는 컴포넌트를 추가
            string ns = "ARCeye";
            string className = Marshal.PtrToStringAnsi(classNamePtr);
            
            GameObject go;
            Type modelType = Type.GetType($"{ns}.{className}");

            if(modelType == typeof(UnitySignPOI)) {
                go = s_POIGenerator.GenerateSignPOI();
            } else if(modelType == typeof(UnityInfoPanel)) {
                GameObject empty = new GameObject("itemInfoPanel");

                go = s_InfoPanelGenerator.GenerateInfoPanel();
                go.transform.parent = empty.transform;

                SetParentToModel(empty, modelType);
            } else if(modelType == typeof(UnityMapPOIPool)) {
                var prev = GameObject.Find("UnityMapPOIPool");
                Destroy(prev);
                
                go = new GameObject();
            } else {
                go = new GameObject();
            }
            go.name = className;
            
            // 각 Item별 적당한 root로 이동.
            if(modelType != typeof(UnityInfoPanel))
            SetParentToModel(go, modelType);

            // 각 Item들을 초기화.
            var model = go.GetComponent(modelType);
            if(model == null) {
                model = go.AddComponent(modelType);
            }

            if(modelType == typeof(UnityMapPathIndicator)) {
                (model as UnityMapPathIndicator).SetMaterial(s_Instance.m_PathMaterial);
            }

            var unityModel = model as UnityModel;
            unityModel.RunCoroutine(()=>{
                OnLoadingCompleteNative(nativeModelPtr);
            }, 0.1f);
            unityModel.Initialize();

            return Wrap(go);
        }

        [MonoPInvokeCallback(typeof(LoadModelFuncDelegate))]
        static public IntPtr LoadModel(IntPtr filePathPtr, IntPtr classNamePtr, IntPtr nativeModelPtr)
        {
            // className과 일치하는 컴포넌트를 추가.
            string ns = "ARCeye";
            string className = Marshal.PtrToStringAnsi(classNamePtr);
            string filePath = Marshal.PtrToStringAnsi(filePathPtr);

            filePath = PathUtil.Validate(filePath);

            bool useNextStep = FindObjectOfType<NextStep>() != null;
            if(className == nameof(UnityNextStepDot) || className == nameof(UnityNextStepArrow) || className == nameof(UnityNextStepText)) {
                if(!useNextStep) {
                    return IntPtr.Zero;
                }
            }

            GameObject go = new GameObject(filePath);
            Type modelType = Type.GetType($"{ns}.{className}");
            
            go.AddComponent(modelType);
            go.AddComponent<PostEventGltfAsset>();

            // 각 Item별 적당한 root로 이동.
            SetParentToModel(go, modelType);

            // glb 파일이 없는 Item인지 확인. filesystem을 사용하지 않는 Android 환경에서 호출된다.
            if(NativeFileSystemHelper.IsGLBNotAssigned(filePath))
            {
                NativeLogger.Print(LogLevel.WARNING, $"glb model file is not assigned to an instance of {className}");
            }
            else
            {
                s_MainThreadLoadingHandler.Enqueue(go, filePath, ()=>{
                    OnLoadingCompleteNative(nativeModelPtr);
                });
            }

            return Wrap(go);
        }

        static public void SetParentToModel(GameObject modelGo, Type modelType)
        {
            if(s_Instance.m_Scene != null) {
                modelGo.transform.parent = s_Instance.m_Scene.transform;
            }
        }

        [MonoPInvokeCallback(typeof(UnloadModelFuncDelegate))]
        static public void UnloadModel(IntPtr itemPtr)
        {
            // 이미 GameObject가 Destroy 된 상태에서
            // native의 background thread가 UnloadModel을 호출해서 발생하는 문제 방지.
            try {
                GCHandle.FromIntPtr(itemPtr);
                GameObject item = Unwrap<GameObject>(itemPtr);

                if(item.GetComponent<UnityModel>().GetType() == typeof(UnityInfoPanel))
                {
                    Destroy(item.transform.parent.gameObject);
                }

                Destroy(item);
            } catch (Exception e) {
                // NativeLogger.Print(LogLevel.WARNING, e.ToString());
            }
        }

        [MonoPInvokeCallback(typeof(SetNameFuncDelegate))]
        static public void SetName(IntPtr itemPtr, IntPtr namePtr)
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                try {
                    GameObject item = Unwrap<GameObject>(itemPtr);
                    string name = Marshal.PtrToStringAnsi(namePtr);
                    item.name = name;
                } catch (Exception e) {
                    NativeLogger.Print(LogLevel.WARNING, e.ToString());
                    return;
                }
            });
        }

        // Background thread에서 실행
        [MonoPInvokeCallback(typeof(SetActiveFuncDelegate))]
        static public void SetActive(IntPtr itemPtr, bool active)
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                item.GetComponent<UnityModel>().SetActive(active); 
            });
        }

        [MonoPInvokeCallback(typeof(SetSetPickableFuncDelegate))]
        static public void SetPickable(IntPtr itemPtr, bool pickable)
        {
            // TODO. interaction 관련 기능을 구현한 뒤 연동.
        }

        [MonoPInvokeCallback(typeof(SetMatrixModelFuncDelegate))]
        static public void SetMatrixModel(IntPtr itemPtr, IntPtr matrix)
        {
            Matrix4x4 rhm = PoseHelper.UnmanagedToMatrix4x4(matrix);
            Matrix4x4 lhm = PoseHelper.ConvertLHRH(rhm);

            Matrix4x4 modelMatrix = lhm;

            GameObject item = Unwrap<GameObject>(itemPtr);

            item.transform.localPosition = modelMatrix.GetPosition();
            item.transform.localRotation = modelMatrix.rotation;
            item.transform.localScale = modelMatrix.lossyScale;

            item.transform.Rotate(0, 180, 0);
        }

        // Background thread에서 실행
        [MonoPInvokeCallback(typeof(SetPlayAnimationFuncDelegate))]
        static public void PlayAnimation(IntPtr itemPtr, IntPtr animNamePtr, IntPtr playModePtr) 
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);

                string animName = Marshal.PtrToStringAnsi(animNamePtr);
                string playMode = Marshal.PtrToStringAnsi(playModePtr);

                item.GetComponent<UnityModel>().PlayAnimation(animName, playMode);
            });
        }

        [MonoPInvokeCallback(typeof(SetHasAnimationFuncDelegate))]
        static public bool HasAnimation(IntPtr itemPtr, IntPtr animNamePtr) 
        {
            bool value = true;
            
            GameObject item = Unwrap<GameObject>(itemPtr);
            string animName = Marshal.PtrToStringAnsi(animNamePtr);
            value = item.GetComponent<UnityModel>().HasAnimation(animName);

            return value;
        }

        [MonoPInvokeCallback(typeof(SetAnimationDurationFuncDelegate))]
        static public float AnimationDuration(IntPtr itemPtr, IntPtr animNamePtr)
        {
            GameObject item = Unwrap<GameObject>(itemPtr);
            if(item == null) {
                return 0.0f;
            }

            string animName = Marshal.PtrToStringAnsi(animNamePtr);
            UnityModel unityModel = item.GetComponent<UnityModel>();

            if(unityModel == null) {
                return 0.0f;
            }
            float value = unityModel.AnimationDuration(animName);
            return value;
        }

        [MonoPInvokeCallback(typeof(SetFadeFuncDelegate))]
        static public void Fade(IntPtr itemPtr, float duration, bool fadeIn) 
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                
                var infoPanel = item.GetComponent<UnityInfoPanel>();
                if(infoPanel != null) {
                    infoPanel.Fade(duration, fadeIn);
                    return;
                }

                var unityModel = item.GetComponent<UnityModel>();
                if(unityModel != null) {
                    unityModel.Fade(duration, fadeIn);
                    return;
                }
                
                NativeLogger.Print(LogLevel.WARNING, $"[ItemGenerator] Fade - {item.name} doesn't have a UnityModel component");
            });
        }

        [MonoPInvokeCallback(typeof(SetFadeCallbackFuncDelegate))]
        static public void FadeCallback(IntPtr itemPtr, IntPtr nativeModelPtr, float duration, bool fadeIn) 
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityTurnSpot model = item.GetComponent<UnityTurnSpot>();
                if(model != null) {
                    model.Fade(duration, fadeIn, ()=>{
                        OnFadingCompleteNative(nativeModelPtr);
                    });
                } else {
                    NativeLogger.Print(LogLevel.WARNING, $"[ItemGenerator] Fade - {item.name} doesn't have a UnityModel component");
                }
            });
        }


        // Background thread에서 실행
        [MonoPInvokeCallback(typeof(SetSetOpacityFuncDelegate))]
        static public void SetOpacity(IntPtr itemPtr, float opacity) 
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityModel unityModel = item.GetComponent<UnityModel>();
                if(unityModel != null) {
                    unityModel.SetOpacity(opacity);
                } else {
                    NativeLogger.Print(LogLevel.WARNING, $"[ItemGenerator] SetOpacity - {item.name} doesn't have a UnityModel component");
                }
            });
        }



        //// UnityTurnSpot
        [MonoPInvokeCallback(typeof(VB_Func))]
        static public void SetBillboard(IntPtr itemPtr, bool active)
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);

                UnityModel model = item.GetComponent<UnityModel>();
                if(model != null) {
                    model.SetBillboard(active);
                    return;
                }
                    
                NativeLogger.Print(LogLevel.WARNING, $"[ItemGenerator] SetBillboard - {item.name} doesn't have a UnityTurnSpot component");
            });
        }

        [MonoPInvokeCallback(typeof(SetNameFuncDelegate))]
        static public void SetTurnSpotLabel(IntPtr itemPtr, IntPtr labelPtr) 
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityTurnSpot turnSpot = item.GetComponent<UnityTurnSpot>();

                string label = Marshal.PtrToStringAnsi(labelPtr);
                turnSpot.SetLabel(label);
            });
        }


        //// UnityMapPathIndicator
        [MonoPInvokeCallback(typeof(VFpI_Func))]
        static public void SetRoute(IntPtr ptr, IntPtr buffer, int count)
        {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                int length = count * 3;
                float[] pathBuffer = new float[length];
                Marshal.Copy(buffer, pathBuffer, 0, length);

                GameObject item = Unwrap<GameObject>(ptr);
                UnityMapPathIndicator mapPath = item.GetComponent<UnityMapPathIndicator>();

                int pathIndex = mapPath.AddPath();
                mapPath.SetPath(pathIndex, pathBuffer, s_Instance.m_MapPathBulletBegin, s_Instance.m_MapPathBulletEnd);
            });
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        static public void UnloadPath(IntPtr ptr)
        {
            try {
                GCHandle.FromIntPtr(ptr);
            } catch (Exception e) {
                NativeLogger.Print(LogLevel.WARNING, e.ToString());
                return;
            }

            GameObject item = Unwrap<GameObject>(ptr);
            Destroy(item.gameObject);
        }


        //// UnityMapPOIPool
        [MonoPInvokeCallback(typeof(VF_Func))]
        static public void SetFontSize(IntPtr itemPtr, float fontSize) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityMapPOIPool model = item.GetComponent<UnityMapPOIPool>();
                model.SetFontSize(fontSize);
            });
        }

        [MonoPInvokeCallback(typeof(VF_Func))]
        static public void SetOutlineThickness(IntPtr itemPtr, float fontSize) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityMapPOIPool model = item.GetComponent<UnityMapPOIPool>();
                model.SetOutlineThickness(fontSize);
            });
        }

        [MonoPInvokeCallback(typeof(VIISFFFI_Func))]
        static public void InsertPOIEntity(IntPtr itemPtr, int id, int dpCode, IntPtr labelRaw, float px, float py, float pz, int drawType) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityMapPOIPool mapPOIPool = item.GetComponent<UnityMapPOIPool>();

                UnityMapPOI mapPOI = s_POIGenerator.GenerateMapPOI();
                s_POIGenerator.SetIconCode(mapPOI, dpCode);

                string label = Marshal.PtrToStringAnsi(labelRaw);
                Vector3 position = new Vector3(px, py, pz);

                mapPOIPool.InsertPOIEntity(mapPOI, id, dpCode, label, position, drawType);
            });
        }

        [MonoPInvokeCallback(typeof(V_Func))]
        static public void RemoveNode(IntPtr itemPtr) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);

                // Scene을 변경하는 경우에는 item이 null이 되는 경우가 발생.
                if(item != null)
                {
                    UnityMapPOIPool model = item.GetComponent<UnityMapPOIPool>();
                    model.RemoveAllMapPOIs();
                }
            });
        }

        [MonoPInvokeCallback(typeof(S_Func))]
        static public void SetConfigFullpath(IntPtr atlasFullpathRaw) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                // GameObject item = Unwrap<GameObject>(itemPtr);
                // UnityMapPOIPool model = item.GetComponent<UnityMapPOIPool>();

                string fullpath = Marshal.PtrToStringAnsi(atlasFullpathRaw);
                UnityMapPOIPool.atlasFullpath = fullpath;
            });
        }



        //// UnitySignPOI
        [MonoPInvokeCallback(typeof(VI_Func))]
        static public void SetType(IntPtr itemPtr, int type) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnitySignPOI model = item.GetComponent<UnitySignPOI>();
                model.SetType(type);
            });
        }

        [MonoPInvokeCallback(typeof(HasCodeFunc))]
        static public bool HasCode(int dpCode) {
            return true;
        }

        [MonoPInvokeCallback(typeof(GetDefaultIconCodeFunc))]
        static public int GetDefaultIconCode(IntPtr itemPtr) {
            return 0;
        }

        [MonoPInvokeCallback(typeof(SetIconCodeFunc))]
        static public void SetIconCode(IntPtr itemPtr, int type) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnitySignPOI model = item.GetComponent<UnitySignPOI>();
                s_POIGenerator.SetIconCode(model, type);
            });
        }

        [MonoPInvokeCallback(typeof(SetLabelFunc))]
        static public void SetLabel(IntPtr itemPtr, IntPtr rawStr) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnitySignPOI model = item.GetComponent<UnitySignPOI>();

                string content = Marshal.PtrToStringAnsi(rawStr);
                model.gameObject.name = content;
                model.SetLabel(content);
            });
        }

        [MonoPInvokeCallback(typeof(SetAutoRotateModeFunc))]
        static public void SetAutoRotateMode(IntPtr itemPtr, int rotateMode) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnitySignPOI model = item.GetComponent<UnitySignPOI>();

                ///
                /// Auto Rotate Type
                /// 0:None, 1:AxisY, 2:AxisZ, 3:Camera
                /// 
                model.SetAutoRotateMode(rotateMode);
            });
        }


        /// UnityInfoPanel
        [MonoPInvokeCallback(typeof(VI_Func))]
        static public void SetInfoPanelType(IntPtr itemPtr, int type) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityInfoPanel model = item.GetComponent<UnityInfoPanel>();
                model.SetInfoPanelType(type);
            });
        }

        [MonoPInvokeCallback(typeof(VB_Func))]
        static public void UseFrame(IntPtr itemPtr, bool value) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityInfoPanel model = item.GetComponent<UnityInfoPanel>();
                model.UseFrame(value);
            });
        }

        [MonoPInvokeCallback(typeof(VB_Func))]
        static public void UseRoundedCorner(IntPtr itemPtr, bool value) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityInfoPanel model = item.GetComponent<UnityInfoPanel>();
                model.UseRoundedCorner(value);
            });
        }

        [MonoPInvokeCallback(typeof(VS_Func))]
        static public void SetInfoPanelText(IntPtr itemPtr, IntPtr strPtr) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityInfoPanel model = item.GetComponent<UnityInfoPanel>();

                string text = Marshal.PtrToStringAnsi(strPtr);
                model.SetInfoPanelText(text);
            });
        }

        [MonoPInvokeCallback(typeof(VS_Func))]
        static public void SetInfoPanelImage(IntPtr itemPtr, IntPtr imagePathPtr) {
            MainThreadDispatcher.Instance()?.Enqueue(()=>{
                GameObject item = Unwrap<GameObject>(itemPtr);
                UnityInfoPanel model = item.GetComponent<UnityInfoPanel>();

                string imagePath = Marshal.PtrToStringAnsi(imagePathPtr);
                model.SetInfoPanelImage(imagePath);
            });
        }

        //// 

        static private T Unwrap<T>(IntPtr ptr) {
            object itemObj = GCHandle.FromIntPtr(ptr).Target;
            T original = (T) itemObj;
            return original;
        }

        static private IntPtr Wrap(UnityEngine.Object obj) {
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Weak);
            return GCHandle.ToIntPtr(handle);
        }
    }
}
