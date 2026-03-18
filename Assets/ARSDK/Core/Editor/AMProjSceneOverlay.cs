using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace ARCeye
{
    /// <summary>
    /// Scene 뷰에서 amproj 파일의 지도 및 POI를 미리 보여주는 오버레이 도구.
    /// </summary>
    [Overlay(typeof(SceneView), "AMProj Viewer", defaultDisplay = true)]
    [Icon("d_SceneViewTools@2x")]
    public class AMProjSceneOverlay : Overlay, ITransientOverlay
    {
        // EditorPrefs 키
        private const string PrefKeyAmprojPath = "AMProjSceneOverlay_AmprojPath";
        private const string PrefKeyStageName = "AMProjSceneOverlay_StageName";
        private const string PrefKeyVisible = "AMProjSceneOverlay_Visible";
        private const string PrefKeyGeometryVisible = "AMProjSceneOverlay_GeometryVisible";
        private const string PrefKeyPOIVisible = "AMProjSceneOverlay_POIVisible";

        // amproj 파싱 및 렌더링 모듈
        private readonly AMProjEditorParser m_Parser = new AMProjEditorParser();
        private readonly AMProjSceneRenderer m_Renderer = new AMProjSceneRenderer();

        // 시각화 표시 여부
        private bool m_Visible = true;
        private bool m_GeometryVisible = true;
        private bool m_POIVisible = true;

        // 외부에서 amproj 로드를 트리거하기 위한 정적 이벤트
        public static event System.Action<string> OnLoadAMProjRequested;

        // UI 요소
        private DropdownField m_StageDropdown;
        private Button m_VisibilityToggle;
        private Button m_GeometryToggle;
        private Button m_POIToggle;

        public bool visible => true;

        #region Overlay Lifecycle

        public override void OnCreated()
        {
            base.OnCreated();

            // 이벤트 등록
            SceneView.duringSceneGui += m_Renderer.OnSceneGUI;
            OnLoadAMProjRequested += HandleLoadAMProjRequested;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // 이전 세션 상태 복원
            RestoreState();
        }

        public override void OnWillBeDestroyed()
        {
            // 이벤트 해제
            SceneView.duringSceneGui -= m_Renderer.OnSceneGUI;
            OnLoadAMProjRequested -= HandleLoadAMProjRequested;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignItems = Align.Center;

            // 시각화 토글 버튼
            m_VisibilityToggle = CreateIconButton("d_scenevis_visible_hover", "Toggle Visibility", OnVisibilityToggleClicked);
            root.Add(m_VisibilityToggle);

            // Geometry 시각화 토글 버튼
            m_GeometryToggle = CreateIconButton("d_RectTool", "Toggle Geometry", OnGeometryToggleClicked);
            root.Add(m_GeometryToggle);

            // POI 시각화 토글 버튼
            m_POIToggle = CreateIconButton("d_Text Icon", "Toggle POI", OnPOIToggleClicked);
            root.Add(m_POIToggle);

            // 스테이지 선택 드롭다운
            m_StageDropdown = new DropdownField(new List<string>(), 0);
            m_StageDropdown.RegisterValueChangedCallback(evt =>
            {
                int index = m_Parser.StageNames.IndexOf(evt.newValue);
                if (index >= 0) SelectStage(index);
            });
            m_StageDropdown.SetEnabled(false);
            m_StageDropdown.style.minWidth = 140;
            m_StageDropdown.style.height = 22;
            root.Add(m_StageDropdown);

            // 복원된 데이터를 UI에 반영
            SyncUIWithState();
            return root;
        }

        #endregion

        #region State Management

        // EditorPrefs에서 이전 세션 상태 복원
        private void RestoreState()
        {
            m_Visible = EditorPrefs.GetBool(PrefKeyVisible, true);
            m_GeometryVisible = EditorPrefs.GetBool(PrefKeyGeometryVisible, true);
            m_POIVisible = EditorPrefs.GetBool(PrefKeyPOIVisible, true);
            m_Renderer.Visible = m_Visible;
            m_Renderer.GeometryVisible = m_GeometryVisible;
            m_Renderer.POIVisible = m_POIVisible;

            if (Object.FindFirstObjectByType<ARPlayGround>() == null) return;

            string savedPath = EditorPrefs.GetString(PrefKeyAmprojPath, "");
            if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
            {
                LoadAMProj(savedPath);
            }
        }

        // 파싱된 데이터를 UI 요소에 동기화
        private void SyncUIWithState()
        {
            UpdateVisibilityIcon();
            UpdateToggleOpacity(m_GeometryToggle, m_GeometryVisible);
            UpdateToggleOpacity(m_POIToggle, m_POIVisible);

            if (m_Parser.StageNames.Count > 0)
            {
                m_StageDropdown.choices = m_Parser.StageNames;
                m_StageDropdown.SetEnabled(true);

                if (m_Parser.SelectedStageName != null)
                {
                    m_StageDropdown.SetValueWithoutNotify(m_Parser.SelectedStageName);
                }
            }
        }

        // amproj 파일 로드 및 스테이지 복원
        private void LoadAMProj(string path)
        {
            // 파싱 중 덮어쓰기 방지를 위해 미리 읽어둠
            string savedStage = EditorPrefs.GetString(PrefKeyStageName, "");

            if (!m_Parser.Load(path)) return;

            EditorPrefs.SetString(PrefKeyAmprojPath, path);

            // 드롭다운 갱신
            if (m_StageDropdown != null)
            {
                m_StageDropdown.SetValueWithoutNotify(null);
                m_StageDropdown.choices = m_Parser.StageNames;
                m_StageDropdown.SetEnabled(true);
            }

            // 저장된 스테이지 복원, 없으면 첫 번째 선택
            int stageIndex = string.IsNullOrEmpty(savedStage) ? 0 : m_Parser.StageNames.IndexOf(savedStage);
            SelectStage(stageIndex >= 0 ? stageIndex : 0);
        }

        // 스테이지 선택 및 렌더러에 레이어 전달
        private void SelectStage(int index)
        {
            var layers = m_Parser.SelectStage(index);
            EditorPrefs.SetString(PrefKeyStageName, m_Parser.SelectedStageName);

            m_Renderer.SetLayers(layers);

            if (m_StageDropdown != null)
            {
                m_StageDropdown.SetValueWithoutNotify(m_Parser.SelectedStageName);
            }

            SceneView.RepaintAll();
        }

        #endregion

        #region Callbacks

        // 시각화 토글
        private void OnVisibilityToggleClicked()
        {
            m_Visible = !m_Visible;
            m_Renderer.Visible = m_Visible;
            EditorPrefs.SetBool(PrefKeyVisible, m_Visible);
            UpdateVisibilityIcon();
            SceneView.RepaintAll();
        }

        // Geometry 시각화 토글
        private void OnGeometryToggleClicked()
        {
            m_GeometryVisible = !m_GeometryVisible;
            m_Renderer.GeometryVisible = m_GeometryVisible;
            EditorPrefs.SetBool(PrefKeyGeometryVisible, m_GeometryVisible);
            UpdateToggleOpacity(m_GeometryToggle, m_GeometryVisible);
            SceneView.RepaintAll();
        }

        // POI 시각화 토글
        private void OnPOIToggleClicked()
        {
            m_POIVisible = !m_POIVisible;
            m_Renderer.POIVisible = m_POIVisible;
            EditorPrefs.SetBool(PrefKeyPOIVisible, m_POIVisible);
            UpdateToggleOpacity(m_POIToggle, m_POIVisible);
            SceneView.RepaintAll();
        }

        // Play/Edit 모드 전환 처리
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    m_Renderer.SetLayers(null);
                    SceneView.RepaintAll();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    RestoreState();
                    SceneView.RepaintAll();
                    break;
            }
        }

        /// <summary>
        /// ARPlayGroundEditor 등 외부에서 amproj 로드를 요청할 때 호출.
        /// </summary>
        public static void RequestLoadAMProj(string amprojPath)
        {
            OnLoadAMProjRequested?.Invoke(amprojPath);
        }

        private void HandleLoadAMProjRequested(string amprojPath)
        {
            if (string.IsNullOrEmpty(amprojPath) || !File.Exists(amprojPath)) return;
            LoadAMProj(amprojPath);
        }

        #endregion

        #region UI Helpers

        // 아이콘 버튼 생성
        private Button CreateIconButton(string iconName, string tooltip, System.Action clicked)
        {
            var button = new Button(clicked) { tooltip = tooltip };
            button.style.width = 26;
            button.style.height = 22;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            button.style.paddingTop = 0;
            button.style.paddingBottom = 0;
            button.style.marginLeft = 1;
            button.style.marginRight = 1;
            button.style.marginTop = 0;
            button.style.marginBottom = 0;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;

            var icon = new Image
            {
                image = EditorGUIUtility.IconContent(iconName).image
            };
            icon.style.width = 14;
            icon.style.height = 14;
            button.Add(icon);

            return button;
        }

        // 토글 버튼 활성/비활성 시각 표현 (불투명도)
        private void UpdateToggleOpacity(Button button, bool active)
        {
            if (button == null) return;
            button.style.opacity = active ? 1f : 0.35f;
        }

        // 시각화 토글 아이콘 갱신
        private void UpdateVisibilityIcon()
        {
            if (m_VisibilityToggle == null) return;
            string iconName = m_Visible ? "d_scenevis_visible_hover" : "d_scenevis_hidden_hover";
            m_VisibilityToggle.Q<Image>().image = EditorGUIUtility.IconContent(iconName).image;
            m_VisibilityToggle.tooltip = m_Visible ? "Hide Visualization" : "Show Visualization";
        }

        #endregion
    }
}
