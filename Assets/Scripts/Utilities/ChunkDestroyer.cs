using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDestroyer : MonoBehaviour
{
    public float deathTimer = Mathf.Infinity;

/*    private FracturedChunk m_chunk;
    private float timer = 0.0f;

    private Renderer _renderer;
    private int _shaderPropertyID;

	// Use this for initialization
	void Start ()
    {
        _renderer = GetComponent<MeshRenderer>();
        m_chunk = GetComponent<FracturedChunk>();
        _shaderPropertyID = Shader.PropertyToID("_DissolvePercentage");
        timer = 0f;	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (m_chunk.IsDetachedChunk)
        {
            timer += Time.deltaTime;

            float amount = timer / deathTimer;

            MaterialPropertyBlock propertyBlock = MaterialManager.Instance[_renderer];

            propertyBlock.SetFloat(_shaderPropertyID, amount);

            MaterialManager.Instance[_renderer] = propertyBlock;

            if (timer >= deathTimer)
            {
                gameObject.SetActive(false);
            }
        }	
	}*/
}
