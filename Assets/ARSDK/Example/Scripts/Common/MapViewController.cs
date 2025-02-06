using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ARCeye
{
    public class MapViewController : ViewController<MapView>, IMapTouchListener
    {
        [SerializeField]
        private Transform m_MapRoot;

        [SerializeField]
        private MapCameraRig m_MapCameraRig;

        [SerializeField]
        private GameObject m_HideMapButton;


        [Header("Events")]

        [SerializeField]
        private UnityEvent<bool> m_OnMapActivated;

        [SerializeField]
        private UnityEvent m_OnChangedToFull;

        [SerializeField]
        private UnityEvent m_OnChangedToShrinked;

        private YieldInstruction m_YieldMapLoading;


        private UnityMapPOIPool m_MapPOIPool;
        private UnityMapPOIPool mapPOIPool {
            get {
                if(m_MapPOIPool == null) {
                    m_MapPOIPool = FindObjectOfType<UnityMapPOIPool>();
                }
                return m_MapPOIPool;
            }
        }


        protected override void Awake()
        {
            base.Awake();

            m_OnChangedToFull.AddListener(()=>{
                TouchSystem.Instance.onDrag.AddListener(MoveMapCamera);
                TouchSystem.Instance.onPinchZoom.AddListener(ZoomMapCamera);
                TouchSystem.Instance.onRotate.AddListener(RotateCamera);
            });

            m_OnChangedToShrinked.AddListener(()=>{
                TouchSystem.Instance.onDrag.RemoveListener(MoveMapCamera);
                TouchSystem.Instance.onPinchZoom.RemoveListener(ZoomMapCamera);
                TouchSystem.Instance.onRotate.RemoveListener(RotateCamera);
            });
        }

        public override void Show()
        {
            base.Show();
            StartCoroutine( ShowMapViewWithCartoMap() );
        }

        public override void Hide()
        {
            base.Hide();

            HideMinimap();
            StopAllCoroutines();
        }

        public void ChangeStage(string name, string label) {
            m_View.SetStageLabel(label);
        }

        public void Show(bool value, bool isMapFull)
        {
            if(value)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private IEnumerator ShowMapViewWithCartoMap() {
            // 0.1초마다 한번씩 맵 로드 여부를 확인.
            m_YieldMapLoading = new WaitForSeconds(0.1f);

            bool isLoaded = false;

            while(!isLoaded) {
                // UnityCartoMap이 아직 로딩되지 않았을 경우 MapView를 활성화하지 않는다.
                UnityCartoMap cartoMap;

                if(m_MapRoot == null) {
                    cartoMap = FindObjectOfType<UnityCartoMap>();
                } else {
                    cartoMap = m_MapRoot.GetComponentInChildren<UnityCartoMap>();
                }

                if(cartoMap == null) {
                    yield return m_YieldMapLoading;
                    continue;
                }

                if(!cartoMap.isLoaded) {
                    yield return m_YieldMapLoading;
                    continue;
                }
                
                // 터치 이벤트 실행을 위해 cartoMap 인스턴스 할당.
                m_View.SetMapTouchListener(this);

                // UnityCartoMap의 로딩이 완료 되었을 경우 루프 탈출.
                isLoaded = true;
            }

            ActivateMinimap();

            m_OnMapActivated.Invoke(true);
        }

        public void ActivateFullmap() {
            mapPOIPool.ActivateFullmapMode();

            m_MapCameraRig.ChangeToFullMap(true);
            m_View.ActivateFullMapScreen(true);

            m_OnChangedToFull.Invoke();
        }

        public void ActivateMinimap() {
            mapPOIPool.ActivateMinimapMode();
            
            m_MapCameraRig.ChangeToFullMap(false);
            m_View.ActivateFullMapScreen(false);

            m_OnChangedToShrinked.Invoke();
        }

        public void HideMinimap() {
            m_View.Show(false);

            m_OnMapActivated.Invoke(false);
        }

        public void MoveMapCamera(Vector2 delta) {
            m_MapCameraRig.Move(delta);
        }

        public void ZoomMapCamera(float deltaRatio) {
            m_MapCameraRig.Zoom(deltaRatio);
        }

        public void RotateCamera(float deltaDegree) {
            m_MapCameraRig.Rotate(deltaDegree);
        }


        /* ------------------------------------- */
        /*           IMapTouchListener           */
        /* ------------------------------------- */

        /// <summary>
        ///   ShrinkMapScreen을 터치했을때 실행되는 메서드.
        /// </summary>
        public void OnTouch() {
            ActivateFullmap();
        }
    }
}