using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    [Serializable]
    public class TextureAtlasBundle : ScriptableObject
    {
        public TextureAtlasBundleSettings Settings;
        public List<GameObject> BundledObjects;

        TextureAtlasBundle()
        {
            this.Settings = new TextureAtlasBundleSettings();
            this.BundledObjects = new List<GameObject>();
        }
    }

    [Serializable]
    public class TextureAtlasBundleSettings : ISerializationCallbackReceiver
    {
        public int MaximumSize = 10;
        public bool KeepAspectRatio = true;
        public bool ReplaceOriginalPrefabs = false;
        public UVRemapMode UVRemapMode = UVRemapMode.BakeIntoMesh;
        public Dictionary<Shader, Shader> ShaderReplacements;

        [SerializeField]
        private List<Shader> _shaderReplacementsKeys;
        [SerializeField]
        private List<Shader> _shaderReplacementsValues;

        public TextureAtlasBundleSettings()
        {
            this.ShaderReplacements = new Dictionary<Shader, Shader>();
            _shaderReplacementsKeys = new List<Shader>();
            _shaderReplacementsValues = new List<Shader>();
        }

        public void OnBeforeSerialize()
        {
            _shaderReplacementsKeys.Clear();
            _shaderReplacementsValues.Clear();

            foreach (var pair in this.ShaderReplacements)
            {
                _shaderReplacementsKeys.Add(pair.Key);
                _shaderReplacementsValues.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            this.ShaderReplacements.Clear();

            int count = Mathf.Min(_shaderReplacementsKeys.Count, _shaderReplacementsValues.Count);

            for (int i = 0; i < count; i++)
            {
                this.ShaderReplacements.Add(_shaderReplacementsKeys[i], _shaderReplacementsValues[i]);
            }
        }
    }
}