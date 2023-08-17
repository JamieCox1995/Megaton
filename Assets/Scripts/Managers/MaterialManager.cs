using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    private Dictionary<Renderer, MaterialPropertyBlock> _propertyBlocks;

    public static MaterialManager Instance;

    public Shader DissolveShader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _propertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
            DissolveObject.dissolveShader = this.DissolveShader;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public MaterialPropertyBlock this[Renderer renderer]
    {
        get
        {
            MaterialPropertyBlock propertyBlock;

            if (!_propertyBlocks.TryGetValue(renderer, out propertyBlock))
            {
                propertyBlock = new MaterialPropertyBlock();

                _propertyBlocks.Add(renderer, propertyBlock);
            }

            renderer.GetPropertyBlock(propertyBlock);

            return propertyBlock;
        }
        set
        {
            if (_propertyBlocks.ContainsKey(renderer))
            {
                _propertyBlocks[renderer] = value;
            }
            else
            {
                _propertyBlocks.Add(renderer, value);
            }

            renderer.SetPropertyBlock(value);
        }
    }
}
