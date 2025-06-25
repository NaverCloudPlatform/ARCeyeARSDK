using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.Networking;

namespace ARCeye
{

    enum SpatialCurve
    {
        NONE = -1,
        SMOOTHSTEP = 0,
        LOGARITHMIC = 1,
        INVERSE = 2,
    }

    public class UnityAudio : MonoBehaviour
    {
        public static OrderedDictionary s_MainPlayers = new OrderedDictionary();

        [SerializeField]
        private AudioSource m_AudioSource;
        private float m_MaxVolume = 1.0f;
        private float m_FadeIn = 0.0f;
        private float m_FadeOut = 0.0f;

        private bool m_IsSpatial = false;
        private SpatialCurve m_SpatialCurve = SpatialCurve.NONE;

        private void Awake()
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = gameObject.GetComponent<AudioSource>();
                m_AudioSource.spatialize = false;
            }
        }

        public void Play(UnityMediaInfo info)
        {
            gameObject.name = info.uuid;

            m_AudioSource.loop = info.isLoop;
            m_AudioSource.volume = 0.0f;

            m_MaxVolume = info.volume;
            m_FadeIn = info.fadeIn;
            m_FadeOut = info.fadeOut;

            m_IsSpatial = info.isSpatial;
            if (m_IsSpatial)
            {
                if (info.spatialCurve == 0)
                {
                    m_SpatialCurve = SpatialCurve.SMOOTHSTEP;
                }
                else if (info.spatialCurve == 1)
                {
                    m_SpatialCurve = SpatialCurve.LOGARITHMIC;
                }
                else if (info.spatialCurve == 2)
                {
                    m_SpatialCurve = SpatialCurve.INVERSE;
                }
                else
                {
                    m_SpatialCurve = SpatialCurve.NONE;
                }
                m_AudioSource.volume = m_MaxVolume * info.distance;
            }

            StartCoroutine(PlayClipFromPath(info.fullpath));
        }

        public void Play()
        {
            m_AudioSource.Play();
        }

        IEnumerator PlayClipFromPath(string path)
        {
            string url = "file://" + path;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                    m_AudioSource.clip = clip;
                    m_AudioSource.Play();

                    if (m_IsSpatial == false)
                    {
                        StartCoroutine(FadeInternal(m_FadeIn, true));
                    }
                }
            }
        }

        public void Pause()
        {
            m_AudioSource.Pause();
        }

        public void Unpause()
        {
            m_AudioSource.UnPause();
        }

        public void Mute(bool mute)
        {
            m_AudioSource.mute = mute;
        }

        public bool IsPlaying()
        {
            return m_AudioSource.isPlaying;
        }

        public void AttenuateVolume(float distance)
        {
            if (m_IsSpatial == false || m_SpatialCurve == SpatialCurve.NONE)
            {
                return;
            }

            if (m_SpatialCurve == SpatialCurve.SMOOTHSTEP)
            {
                m_AudioSource.volume = m_MaxVolume * distance;
            }
            else if (m_SpatialCurve == SpatialCurve.LOGARITHMIC)
            {
                m_AudioSource.volume = ((Mathf.Log(distance) / 4.0f) + 1) * m_MaxVolume;
            }
            else if (m_SpatialCurve == SpatialCurve.INVERSE)
            {
                m_AudioSource.volume = -(m_MaxVolume * distance) + m_MaxVolume;
            }
        }

        public void Stop(bool ignoreFade = false, System.Action onComplete = null)
        {
            if (ignoreFade || m_IsSpatial)
            {
                m_AudioSource.Stop();
                if (onComplete != null)
                {
                    onComplete.Invoke();
                }
                return;
            }

            StartCoroutine(FadeInternal(m_FadeOut, false, onComplete));
        }

        public void Unload()
        {
            Destroy(gameObject);
        }

        private IEnumerator FadeInternal(float duration, bool fadeIn, System.Action onComplete = null)
        {
            float start = m_AudioSource.volume;
            float end = fadeIn ? m_MaxVolume : 0;

            bool isFinished = false;
            float accumTime = 0;

            while (!isFinished)
            {
                float t = accumTime / duration;

                float v = Mathf.Lerp(start, end, t);

                m_AudioSource.volume = v;

                yield return null;

                accumTime += Time.deltaTime;

                if (accumTime >= duration)
                {
                    isFinished = true;
                }
            }

            m_AudioSource.volume = end;

            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }
    }
}