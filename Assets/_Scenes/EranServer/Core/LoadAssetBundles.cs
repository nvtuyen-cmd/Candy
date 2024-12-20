using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EranServer
{
    public class LoadAssetBundles : MonoBehaviour
    {
        public static LoadAssetBundles Instance { get; private set; }

        private readonly ConcurrentQueue<IEnumerator> mainThreadQueueIE = new ConcurrentQueue<IEnumerator>();
        private readonly Dictionary<string, CacheAssetBundle> assetBundleCacheList = new Dictionary<string, CacheAssetBundle>();
        private readonly Dictionary<string, CurrentDownloadFile> currentDownloadFileList = new Dictionary<string, CurrentDownloadFile>();
        private readonly ConcurrentDictionary<string, object> lockResourceList = new ConcurrentDictionary<string, object>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitInstant()
        {
            var gameObject = new GameObject("LoadAssetBundles");
            Instance = gameObject.AddComponent<LoadAssetBundles>();
            DontDestroyOnLoad(gameObject);
            Instance.Init();
        }
        private void Init()
        {
            StartCoroutine(IE_MainThreadHandle());
        }
        private IEnumerator IE_MainThreadHandle()
        {
            int coCurrent = 0;
            const int coCurrentLimit = 10;
            while (true)
            {
                if (mainThreadQueueIE.IsEmpty || coCurrent >= coCurrentLimit)
                {
                    yield return null;
                }
                else
                {
                    while (!mainThreadQueueIE.IsEmpty && coCurrent < coCurrentLimit)
                    {
                        if (mainThreadQueueIE.TryDequeue(out var action))
                        {
                            StartCoroutine(IE_Runner(action));
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
            }

            IEnumerator IE_Runner(IEnumerator action)
            {
                coCurrent++;
                yield return StartCoroutine(action);
                coCurrent--;
            }
        }
        public void LoadAsset<T>(string _resourceID, Action<T> _onSuccess, Action _onError, Action<float> _onProgress) where T : UnityEngine.Object
        {
            float fake = 0.3f;
            float real = 1f - fake;
            if (IsResourceExist(_resourceID))
            {

            }
            void LoadResourceHandle()
            {
                _onProgress?.Invoke(0.5f);
                try
                {
                    // As asset bundle
                    LoadAssetBundle(_resourceID, _onSuccess, _onError, (val) => { _onProgress?.Invoke(fake + val * real); });
                }
                catch (Exception)
                {
                    DeleteResource(_resourceID);
                    _onError?.Invoke();
                }
            }
        }
        private void LoadAssetBundle<T>(string _resourceID, Action<T> _onSuccess, Action _onError, Action<float> _onProgress) where T : UnityEngine.Object
        {
            if (assetBundleCacheList.TryGetValue(_resourceID, out var assetBundle))
            {
                _onSuccess?.Invoke(assetBundle.cacheObject as T);
                return;
            }
        }
        private IEnumerator LoadAssetBundleHandle<T>(string _resourceID, Action<T> _onSuccess, Action _onError, Action<float> _onProgress) where T : UnityEngine.Object
        {
            float frist = 0.8f;
            float last = 1f - frist;
            string path = GetLocalPath(_resourceID);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                _onProgress?.Invoke(request.progress * frist);
                yield return null;
            }
            AssetBundle assetBundle = request.assetBundle;
            if (assetBundle == null)
            {
                _onError?.Invoke();
                yield break;
            }
            string[] allAssetName = assetBundle.GetAllAssetNames();
            AssetBundleRequest assetRequest = assetBundle.LoadAssetAsync<GameObject>(allAssetName[0]);
            while (!assetRequest.isDone)
            {
                _onProgress?.Invoke(frist + assetRequest.progress * last);
                yield return null;
            }
            GameObject gObj = assetRequest.asset as GameObject;
            if (gObj == null)
            {
                _onError?.Invoke();
                yield break;
            }
            T asset;
            if (typeof(T) == typeof(GameObject))
            {
                asset = gObj as T;
            }
            else
            {
                asset = gObj.GetComponent<T>();
                if (asset == null)
                {
                    _onError?.Invoke();
                    yield break;
                }
            }
            CacheAssetBundle cacheAssetBundle = new CacheAssetBundle(assetBundle, asset);
            if (assetBundleCacheList.ContainsKey(_resourceID) == false)
            {
                assetBundleCacheList.Add(_resourceID, cacheAssetBundle);
                _onSuccess?.Invoke(asset);
            }
            yield break;
        }
        public void DownloadResource(string _resourceID, Action<bool> _onDone, Action<float> _onProgress)
        {
            if (string.IsNullOrEmpty(_resourceID))
            {
                _onDone?.Invoke(false);
                return;
            }

            if (currentDownloadFileList.TryGetValue(_resourceID, out var currentDownloadFile))
            {
                currentDownloadFile.onDone += _onDone;
                currentDownloadFile.OnProgress += _onProgress;
            }
            else
            {
                // Is not download
                if (IsResourceExist(_resourceID))
                {
                    _onDone?.Invoke(true);
                    return;
                }

                if (lockResourceList.ContainsKey(_resourceID))
                {
                    _onDone?.Invoke(false);
                    return;
                }

                // currentDownloadFile = new CurrentDownloadFile()
                // {
                //     onDone = (result) =>
                //     {
                //         currentDownloadFileList.Remove(_resourceID);
                //         lockResourceList.TryRemove(_resourceID, out _);
                //     }
                //     + _onDone,
                //     OnProgress = _onProgress,
                // };
                // currentDownloadFileList.Add(_resourceID, currentDownloadFile);
                // lockResourceList.TryAdd(_resourceID, null);
                // // Add to queue
                // mainThreadQueueIE.Enqueue(IE_DownloadFileHandle(_resourceID, currentDownloadFile));
            }
        }
        private IEnumerator IE_DownloadFileHandle(string _resourceId, CurrentDownloadFile currentDownloadFile)
        {
            //            var localPath = string.Format(_staticLocalPath, _resourceId);
            //            var serverPath = string.Format(StaticServerPath, _resourceId);
            //var downloader = UnityWebRequest.Get(serverPath);

            //            var lastBytes = downloader.downloadedBytes;
            //            var netInterval = Observable.Interval(TimeSpan.FromSeconds(DownloadIntervalTime)).Subscribe(_ =>
            //            {
            //                if (downloader.downloadedBytes == lastBytes)
            //                {
            //                    downloader.Abort();
            //                }
            //                else
            //                {
            //                    lastBytes = downloader.downloadedBytes;
            //                }
            //            });

            //            currentDownloadFile.OnSuccess = (() =>
            //            {

            //                EventDispatcher.PostEvent(EventID.DOWNLOAD_COMPLETE, new DownloadStatus(localPath, true));
            //            }) + currentDownloadFile.OnSuccess;
            //            currentDownloadFile.OnError = (() =>
            //            {
            //#if LOG_ERROR
            //                Debug.LogError(
            //                    $"Download file error: {resourceId}| {(DateTime.Now - startTime).TotalSeconds}s".Color(ColorDefine
            //                        .HoiDo));

            //#endif
            //                EventDispatcher.PostEvent(EventID.DOWNLOAD_COMPLETE, new DownloadStatus(localPath, false));
            //            }) + currentDownloadFile.OnError;
            //            currentDownloadFile.OnCompleted = (() => { }) + currentDownloadFile.OnCompleted;
            //            // If default resource. Extract default asset
            //            if (IsDefaultResource(_resourceId))
            //            {
            //                yield return StartCoroutine(ExtractDefaultAsset());
            //                currentDownloadFile.OnSuccess?.Invoke();
            //                yield break;
            //            }

            //downloader.SendWebRequest();
            //while (!downloader.isDone)
            //{
            //    currentDownloadFile.OnProgress?.Invoke(downloader.downloadProgress);
            //    yield return null;
            //}
#if LOG_VERBOSE
            Debug.Log($"Dispose net interval {resourceId}");
#endif
            //netInterval.Dispose();

            //if (downloader.result != UnityWebRequest.Result.Success)
            //{
            //    // Error 
            //    currentDownloadFile.OnError?.Invoke();
            //    yield break;
            //}

            // Success
            //var resultBytes = downloader.downloadHandler.data;
            bool isTaskComplete = false;
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        File.WriteAllBytes(localPath, resultBytes);
            //        isTaskComplete = true;
            //    }
            //    catch (Exception e)
            //    {
            //        errorStr = e.Message;
            //        isTaskComplete = true;
            //    }
            //});
            yield return new WaitUntil(() => isTaskComplete);
            //if (string.IsNullOrEmpty(errorStr))
            //{
            //    currentDownloadFile.OnSuccess?.Invoke();
            //}
            //else
            //{
            //    Debug.LogError(errorStr + " " + _resourceId + " " + localPath);
            //    currentDownloadFile.OnError?.Invoke();
            //}
        }
        public bool IsResourceExist(string _fileId)
        {
            //co file chua
            return File.Exists("localPath");
        }
        public string GetLocalPath(string _fileId)
        {
            return "string.Format(_staticLocalPath, fileId)";
        }
        public void DeleteResource(string _resourceID) { }
        public void PurgeResource(string _id)
        {
            if (assetBundleCacheList.TryGetValue(_id, out var assetBundle))
            {
                assetBundle.UnloadAsset();
                assetBundleCacheList.Remove(_id);
            }
        }
        private class CacheAssetBundle
        {
            public readonly AssetBundle assetBundle;
            public readonly UnityEngine.Object cacheObject;

            public CacheAssetBundle(AssetBundle assetBundle, UnityEngine.Object cacheObject)
            {
                this.assetBundle = assetBundle;
                this.cacheObject = cacheObject;
            }
            public void UnloadAsset()
            {
                assetBundle.Unload(true);
                UnityEngine.Object.Destroy(cacheObject);
            }
        }
        private class CurrentDownloadFile
        {
            public Action<bool> onDone;
            public Action<float> OnProgress;
        }
    }
}
