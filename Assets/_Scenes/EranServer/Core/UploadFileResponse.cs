using Newtonsoft.Json;
using System;
using UnityEngine;
namespace EranServer
{
    [Serializable]
    public class UploadFileResponse : MonoBehaviour
    {
        [JsonProperty("fileId")] public string FileId { get; set; }
    }
}
