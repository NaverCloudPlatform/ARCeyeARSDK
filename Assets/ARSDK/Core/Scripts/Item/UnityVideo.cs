using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Video;

namespace ARCeye
{
    public class UnityVideo : UnityModel
    {
        public static OrderedDictionary s_MainPlayers = new OrderedDictionary();

        [SerializeField]
        private VideoPlayer m_VideoPlayer;

        private Material m_Material;
        private float m_MaxVolume = 1.0f;
        private float m_FadeIn = 0.0f;
        private float m_FadeOut = 0.0f;
        private bool m_IsSpatial = false;
        private SpatialCurve m_SpatialCurve = SpatialCurve.NONE;
        private Coroutine m_CurrCoroutine = null;

        private void Awake() {
            if (m_Material == null) {
                m_Material = new Material(Shader.Find("ARPG/VideoRenderTexture"));
                m_Material.SetFloat("_Alpha", 0.0f);

                var renderer = GetComponent<MeshRenderer>();
                if (renderer != null) {
                    renderer.material = m_Material;
                }
            }

            if (m_VideoPlayer == null) {
                m_VideoPlayer = gameObject.GetComponent<VideoPlayer>();
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Play(UnityMediaInfo info) {
            m_MaxVolume = info.volume;
            m_FadeIn = info.fadeIn;
            m_FadeOut = info.fadeOut;

            m_IsSpatial = info.isSpatial;
            if (m_IsSpatial) {
                if (info.spatialCurve == 0) {
                    m_SpatialCurve = SpatialCurve.SMOOTHSTEP;
                } else if (info.spatialCurve == 1) {
                    m_SpatialCurve = SpatialCurve.LOGARITHMIC;
                } else if (info.spatialCurve == 2) {
                    m_SpatialCurve = SpatialCurve.INVERSE;
                } else {
                    m_SpatialCurve = SpatialCurve.NONE;
                }
            }

            if(m_VideoPlayer != null && m_VideoPlayer.isActiveAndEnabled) {
                m_VideoPlayer.url = info.fullpath;
                m_VideoPlayer.isLooping = info.isLoop;
                m_VideoPlayer.Prepare();
                m_VideoPlayer.prepareCompleted += OnVideoReady;
            }
        }

        private void OnVideoReady(VideoPlayer player) {
            if(m_Material.mainTexture != null) {
                (m_Material.mainTexture as RenderTexture).Release();
            }

            RenderTexture rt = new RenderTexture((int)m_VideoPlayer.width, (int)m_VideoPlayer.height, 24, RenderTextureFormat.ARGB32);
            m_Material.mainTexture = rt;
            m_VideoPlayer.targetTexture = rt;
            
            m_VideoPlayer.SetDirectAudioVolume(0, 0.0f);

            m_VideoPlayer.Play();
        }

        public void Stop(bool ignoreFade = false, System.Action onComplete = null) {
            if (ignoreFade || m_IsSpatial) {
                if(m_VideoPlayer != null) {
                    m_VideoPlayer.Stop();
                }
                if(onComplete != null)
                {
                    onComplete.Invoke();
                }
                return;
            }
        }

        public void Unload() {
            if(m_Material.mainTexture != null) {
                (m_Material.mainTexture as RenderTexture).Release();
            }

            Destroy(gameObject);
        }

        public void Build(float width, float height, float pivotX, float pivotY, bool hasAlphaMask, bool hasBackface, bool isBillboard) {
            // Alpha mask is not supported yet.
            if(hasAlphaMask) {
                Stop();
                gameObject.SetActive(false);
                return;
            }

            if(m_VideoPlayer == null || m_Material == null) {
                return;
            }

            gameObject.transform.localScale = new Vector3(width*-1, height, -1);
            gameObject.transform.position = gameObject.transform.position + new Vector3((0.5f-pivotX)*width, (0.5f-pivotY)*height, 0);

            if(isBillboard) {
                base.SetBillboard(true);
            }

            if(hasBackface) {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
                if (renderer != null) {
                    var material = new Material(Shader.Find("ARPG/VideoBackface"));
                    material.SetFloat("_Alpha", 0.0f);
                    renderer.material = material;
                }
                quad.transform.SetParent(gameObject.transform, false);
                quad.transform.localScale = new Vector3(1, 1, -1); // face opposite
            }
        }

        public void Fade(float duration, bool fadeIn) {
            if(!gameObject.activeSelf) {
                return;
            }

            if (m_CurrCoroutine != null) {
                StopCoroutine(m_CurrCoroutine);
            }

           m_CurrCoroutine = StartCoroutine( FadeInternal(duration, fadeIn) );
        }

        public void Mute(bool mute) {
            m_VideoPlayer.SetDirectAudioMute(0, mute);
        }

        public bool IsPlaying() {
            return m_VideoPlayer.isPlaying;
        }

        public void AttenuateVolume(float distance) {
            if (m_VideoPlayer == null) {
                return;
            }

            if (m_IsSpatial == false || m_SpatialCurve == SpatialCurve.NONE) {
                return;
            }

            if (m_SpatialCurve == SpatialCurve.SMOOTHSTEP) {
                m_VideoPlayer.SetDirectAudioVolume(0, m_MaxVolume * distance);
            } else if (m_SpatialCurve == SpatialCurve.LOGARITHMIC) {
                m_VideoPlayer.SetDirectAudioVolume(0, ((Mathf.Log(distance)/4.0f) + 1) * m_MaxVolume);
            } else if (m_SpatialCurve == SpatialCurve.INVERSE) {
                m_VideoPlayer.SetDirectAudioVolume(0, -(m_MaxVolume * distance) + m_MaxVolume);
            }
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, System.Action onComplete = null)
        {
            if(m_Material == null)
            {
                yield break;
            }

            float start = m_Material.GetFloat("_Alpha");
            float end = fadeIn ? 1 : 0;

            float startVolume = m_VideoPlayer.GetDirectAudioVolume(0);
            float endVolume = fadeIn ? m_MaxVolume : 0;

            bool isFinished = false;
            float accumTime = 0;

            while(!isFinished)
            {
                float t = accumTime / duration;

                // Alpha interpolation
                float a = Mathf.Lerp(start, end, t);

                m_Material.SetFloat("_Alpha", a);

                Material backFace = (gameObject.transform.childCount == 0) ? null : gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material; 
                if (backFace != null) {
                    backFace.SetFloat("_Alpha", a);
                }

                // Volume interpolation
                if (!m_IsSpatial) {
                    float v = Mathf.Lerp(startVolume, endVolume, t);
                    m_VideoPlayer.SetDirectAudioVolume(0, v);
                }

                yield return null;

                accumTime += Time.deltaTime;

                if(accumTime >= duration) {
                    isFinished = true;
                }
            }

            m_Material.SetFloat("_Alpha", end);
            Material backface = (gameObject.transform.childCount == 0) ? null : gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material; 
            if (backface != null) {
                backface.SetFloat("_Alpha", end);
            }

            if (!m_IsSpatial) {
                m_VideoPlayer.SetDirectAudioVolume(0, endVolume);
            }

            if(onComplete != null)
            {
                onComplete.Invoke();
            }
        }
    }
}
