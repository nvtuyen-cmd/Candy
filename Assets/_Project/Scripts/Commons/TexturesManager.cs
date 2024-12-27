using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowerWin
{
    [CreateAssetMenu(fileName = "TexturesManager", menuName = "Data/Textures Manager", order = 1)]
    public class TexturesManager : ScriptableObject
    {
        [SerializeField] private DataTexture[] textures;
    }

    [System.Serializable]
    public class DataTexture
    {
        [SerializeField] private ItemID id;
        [SerializeField] private Texture2D texture;
    }
}