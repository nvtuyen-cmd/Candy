using System;
using UnityEngine;
namespace EranServer
{
    public class EranAssetBundles : MonoBehaviour
    {
        [SerializeField]
        private string assetId;
        public string AssetId
        {
            get => assetId;
        }
        [NonSerialized]
        public string resourceId;
        public void Reset()
        {
            if (string.IsNullOrEmpty(AssetId))
            {
                assetId = gameObject.name.ToLower();
            }
        }
    }
}
