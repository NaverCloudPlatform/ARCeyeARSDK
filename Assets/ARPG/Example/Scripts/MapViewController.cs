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



        public void Show(bool value, bool isMapFull)
        {
            ShowMapView(value);

            if(isMapFull) {
                ActivateFullMap();
            } else {
                ActivateShrinkMap();
            }
        }
        
        public void Hide()
        {
            if(m_HideMapButton)
            {
                m_HideMapButton.SetActive(false);
            }
            ShowMapView(false);
        }

        private void ShowMapView(bool value) {
            if(value) {
                StartCoroutine( WaitLoadingCartoMap() );
            } else {
                m_View?.Show(false);
                m_OnMapActivated.Invoke(false);
            }
        }

        private IEnumerator WaitLoadingCartoMap() {
            // 맵을 로딩할때는 일단 MapView를 비활성화.
            m_View.Show(false);

            // 0.1초마다 한번씩 맵 로드 여부를 확인.
            m_YieldMapLoading = new WaitForSeconds(0.1f);

            bool isLoaded = false;

            while(!isLoaded) {
                yield return m_YieldMapLoading;

                // UnityCartoMap이 아직 로딩되지 않았을 경우 MapView를 활성화하지 않는다.
                UnityCartoMap cartoMap;

                if(m_MapRoot == null) {
                    cartoMap = FindObjectOfType<UnityCartoMap>();
                } else {
                    cartoMap = m_MapRoot.GetComponentInChildren<UnityCartoMap>();
                }

                if(cartoMap == null) {
                    continue;
                }
                if(!cartoMap.isLoaded) {
                    continue;
                }
                
                // 터치 이벤트 실행을 위해 cartoMap 인스턴스 할당.
                m_View.SetMapTouchListener(this);

                // UnityCartoMap의 로딩이 완료 되었을 경우 루프 탈출.
                isLoaded = true;
            }

            m_View.Show(true);
            m_OnMapActivated.Invoke(true);
        }

        public void ActivateFullMap() {
            m_MapCameraRig.ChangeToFullMap(true);
            m_View.ShowFullMapScreen(true);

            if(m_HideMapButton)
            {
                m_HideMapButton.SetActive(false);
            }

            m_OnChangedToFull.Invoke();
        }

        public void ActivateShrinkMap() {
            m_MapCameraRig.ChangeToFullMap(false);
            m_View.ShowFullMapScreen(false);

            if(m_HideMapButton)
            {
                m_HideMapButton.SetActive(true);
            }
            m_OnChangedToShrinked.Invoke();
        }

        public void MoveMapCamera(Vector2 delta) {
            m_MapCameraRig.Move(delta);
        }

        public void ZoomMapCamera(float deltaRatio) {
            m_MapCameraRig.Zoom(deltaRatio);
        }

        public void ChangeStage(string stage) {
            m_View.SetStage(stage);
        }


        /* ------------------------------------- */
        /*           IMapTouchListener           */
        /* ------------------------------------- */

        /// <summary>
        ///   ShrinkMapScreen을 터치했을때 실행되는 메서드.
        /// </summary>
        public void OnTouch() {
            ActivateFullMap();
        }
    }
}