using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
namespace a
{
    [CreateAssetMenu(fileName = "Data", menuName = "a/b", order = 1)]
    public class DataAddress : ScriptableObject
    {
        public string GetKey(TypeAsset _type, string _key)
        {
            //if (true)
            //{
            //    if (Caching.ClearCache())
            //    {

            //    }
            //}
            switch (_type)
            {
                case TypeAsset.Audio:
                    {
                        foreach (DataAsset asset in Audios)
                        {
                            if (asset.Key == _key)
                            {
                                return asset.KeyCode;
                            }
                        }
                        break;
                    }
                case TypeAsset.Sprite:
                    {
                        foreach (DataAsset asset in Sprites)
                        {
                            if (asset.Key == _key)
                            {
                                return asset.KeyCode;
                            }
                        }
                        break;
                    }
                case TypeAsset.GameObject:
                    {
                        foreach (DataAsset asset in GameObjects)
                        {
                            if (asset.Key == _key)
                            {
                                return asset.KeyCode;
                            }
                        }
                        break;
                    }
                case TypeAsset.ScriptTable:
                    {
                        foreach (DataAsset asset in ScriptTables)
                        {
                            if (asset.Key == _key)
                            {
                                return asset.KeyCode;
                            }
                        }
                        break;
                    }
            }
            return null;
        }
        public List<DataAsset> Sprites = new List<DataAsset>();
        public List<DataAsset> GameObjects = new List<DataAsset>();
        public List<DataAsset> Audios = new List<DataAsset>();
        public List<DataAsset> ScriptTables = new List<DataAsset>();
#if UNITY_EDITOR
        private void OnValidate()
        {
            List<DataAsset> datas = new List<DataAsset>();
            GameObjects.Clear();
            Sprites.Clear();
            Audios.Clear();
            ScriptTables.Clear();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings không được tìm thấy!");
                return;
            }
            List<AddressableAssetGroup> groups = settings.groups;
            foreach (var group in groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry != null && !entry.address.Equals("Resources") && !entry.address.Equals("EditorSceneList"))
                    {
                        DataAsset data = new DataAsset(entry);
                        datas.Add(data);
                    }
                }
            }
            if (HasDuplicates(datas))
            {
                Debug.Log("Gong nhau");
            }

            foreach (DataAsset asset in datas)
            {
                if (asset.Asset is GameObject)
                {
                    GameObjects.Add(asset);
                }
                else if (asset.Asset is Texture2D)
                {
                    Sprites.Add(asset);
                }
                else if (asset.Asset is ScriptableObject)
                {
                    ScriptTables.Add(asset);
                }
                else if (asset.Asset is AudioClip)
                {
                    Audios.Add(asset);
                }
                else
                {
                    Debug.Log($"Tao Them Truong {asset.Key}");
                }
            }
        }
        bool HasDuplicates(List<DataAsset> list)
        {
            HashSet<string> seen = new HashSet<string>();

            foreach (DataAsset item in list)
            {
                if (!seen.Add(item.Key))
                {
                    return true;
                }
            }
            return false;
        }
#endif
    }
    public enum TypeAsset
    {
        Sprite,
        GameObject,
        ScriptTable,
        Audio,
    }
    [System.Serializable]
    public class DataAsset
    {
        [SerializeField]
        private string key;
        public string Key => key;
        [SerializeField]
        private string keyCode;
        public string KeyCode => keyCode;
#if UNITY_EDITOR
        [SerializeField]
        private UnityEngine.Object asset;
        public UnityEngine.Object Asset => asset;
        public DataAsset(AddressableAssetEntry _data)
        {
            asset = _data.MainAsset;
            key = asset.name;
            _data.address = key;
            keyCode = EncodeToBase64($"{key}_eran_{Application.version}");
        }
        public string EncodeToBase64(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }
#endif
    }
}