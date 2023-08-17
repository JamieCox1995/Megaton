using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    [Serializable]
    public class TextureAtlas : ScriptableObject
    {
        public Texture2D Texture;
        public List<Material> Materials;
        public List<Mesh> Meshes;
        public List<GameObject> Prefabs;

        public TextureAtlas()
        {
            this.Meshes = new List<Mesh>();
            this.Prefabs = new List<GameObject>();
        }
    }
}