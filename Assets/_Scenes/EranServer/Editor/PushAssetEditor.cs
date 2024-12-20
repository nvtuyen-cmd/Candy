using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace EranServer
{
    public class PushAssetEditor
    {
        public static readonly string URL = "127.0.0.1";
        public IEnumerator UploadAssetBundle(EranAssetBundles _assetBundle)
        {
            string endpoint = URL + "/file";

            List<IMultipartFormSection> uploadForm = new List<IMultipartFormSection>();
            byte[] levelBytes = EranAssetBundlesEditor.Ins.AssetBundleToBytes(_assetBundle);
            uploadForm.Add(new MultipartFormFileSection("File", levelBytes, _assetBundle.AssetId, "application/octet-stream"));
            UnityWebRequest request = UnityWebRequest.Post(endpoint, uploadForm);
            yield return request.SendWebRequest();
            var tryTime = DateTime.Now;
            while (!request.downloadHandler.isDone)
            {
                if (DateTime.Now - tryTime > TimeSpan.FromSeconds(10))
                {
                    Debug.LogError("Upload file timeout");
                    break;
                }
                yield return new WaitForSeconds(0.25f);
            }
            var returnStr = request.downloadHandler.text;
            var res = JsonConvert.DeserializeObject<UploadFileResponse>(returnStr);
            _assetBundle.resourceId = res.FileId;
        }
        public IEnumerator UploadAssetBundle(EranAssetBundles _assetBundle, Action<string> _onSuccess)
        {
            string endpoint = URL + "/file";

            List<IMultipartFormSection> uploadForm = new List<IMultipartFormSection>();
            byte[] levelBytes = EranAssetBundlesEditor.Ins.AssetBundleToBytes(_assetBundle);
            uploadForm.Add(new MultipartFormFileSection("File", levelBytes, _assetBundle.AssetId, "application/octet-stream"));
            var request = UnityWebRequest.Post(endpoint, uploadForm);
            yield return request.SendWebRequest();
            var tryTime = DateTime.Now;
            while (!request.downloadHandler.isDone)
            {
                if (DateTime.Now - tryTime > TimeSpan.FromSeconds(10))
                {
                    Debug.LogError("Upload file timeout");
                    break;
                }
                yield return new WaitForSeconds(0.25f);
            }
            var returnStr = request.downloadHandler.text;
            Debug.Log(returnStr);
            var res = JsonConvert.DeserializeObject<UploadFileResponse>(returnStr);
            _onSuccess?.Invoke(res.FileId);
        }
    }
}