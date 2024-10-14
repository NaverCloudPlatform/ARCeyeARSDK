using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ARCeye
{
    /// <summary>
    ///   동시에 로딩 가능한 최대 모델 개수를 제한.
    ///   gltfast 사용 시 멀티스레딩으로 동시에 10개 가량의 모델을 로드하면 gltfast 내부에서 메모리 이슈가 발생.
    /// </summary>
    public class MainThreadLoadingHandler
    {
        private static Thread s_LoaderTaskThread;
        private static Queue<Action> s_LoaderTaskQueue = new Queue<Action>();
        private static int s_LoaderTaskCount = 0;
        private const  int kMaxSyncTaskCount = 5;


        public MainThreadLoadingHandler()
        {
            s_LoaderTaskThread = new Thread(()=>{
                while(true) {
                    if(s_LoaderTaskQueue.Count == 0) {
                        Thread.Sleep(50);
                        continue;
                    }
                    if(s_LoaderTaskCount == kMaxSyncTaskCount) {
                        continue;
                    }

                    ++s_LoaderTaskCount;
                    var loader = s_LoaderTaskQueue.Dequeue();
                    loader.Invoke();
                }
            });
            s_LoaderTaskThread.Start();
        }

        public void Enqueue(GameObject itemGo, string filePath, Action completeCallback)
        {
            lock(s_LoaderTaskQueue)
            {
                if(itemGo.GetComponent<UnityCartoMap>()
                    || itemGo.GetComponent<UnityNextStepArrow>()
                    || itemGo.GetComponent<UnityNextStepDot>()
                    || itemGo.GetComponent<UnityNextStepText>()
                    || itemGo.GetComponent<UnityTurnSpot>()) {
                    // CartoMap의 경우 즉시 로드.
                    LoadOnMainThread(itemGo, filePath, completeCallback, ignoreTaskCount:true);
                } else {
                    s_LoaderTaskQueue.Enqueue(()=>{
                        LoadOnMainThread(itemGo, filePath, completeCallback);
                    });
                }
            }
        }

        private void LoadOnMainThread(GameObject itemGo, string filePath, Action completeCallback, bool ignoreTaskCount = false)
        {
            MainThreadDispatcher.Instance().Enqueue(()=>{
                try
                {
                    if(itemGo == null) {
                        if(!ignoreTaskCount) {
                            --s_LoaderTaskCount;
                        }
                        return;
                    }

                    var gltf = itemGo.GetComponent<PostEventGltfAsset>();
                    if(gltf == null) {
                        if(!ignoreTaskCount) {
                            --s_LoaderTaskCount;
                        }
                        return;
                    }

                    gltf.PostEvent = (success) => {
                        var item = itemGo.GetComponent<UnityModel>();
                        if(item == null) {
                            if(!ignoreTaskCount) {
                                --s_LoaderTaskCount;
                            }
                            return;
                        }

                        item.Initialize(gltf);
                        completeCallback.Invoke();
                        if(!ignoreTaskCount) {
                            --s_LoaderTaskCount;
                        }
                    };
                    gltf.Load(filePath);
                }
                catch(Exception e)
                {
                    if(!ignoreTaskCount) {
                        --s_LoaderTaskCount;
                    }
                    Debug.LogWarning("[MainThreadLoadingHandler] " + e);
                }
            });
        }
    }
}