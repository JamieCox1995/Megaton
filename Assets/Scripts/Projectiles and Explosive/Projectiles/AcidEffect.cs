using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidEffect : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)]
    public Color acidDissolveColor = Color.white;
    public float dissolveTime = 3f;

    public float dissolveProp = 0.25f;
    public float dissolveNoiseScale = 5f;

    //public string shaderToUse = "FX/Dissolve and Burn/With Shadows";

    public LayerMask dissolvableObjects;

    [Header("Acid Sizzling Audio")]
    public AudioClip[] sizzlingAudios;
    public GameObject sizzlingSFXSource;
    public Vector2 _3dAudioSettings = new Vector2(5f, 50f);
    public float minDistanceBetweenAudio = 10f;

    private List<GameObject> activeAudioSources = new List<GameObject>();

    private void OnParticleCollision(GameObject other)
    {
        // Checking to see there are any avaliable audio spaces left
        if (activeAudioSources.Count  < 25)
        {
            bool canSpawn = false;

            // Check to see if the current collision position is further than X units away from other audio sources
            for(int i = 0; i < activeAudioSources.Count; i++)
            {
                if (Vector3.Distance(other.transform.position, activeAudioSources[i].transform.position) >= minDistanceBetweenAudio)
                {
                    canSpawn = true;
                }
                else
                {
                    canSpawn = false;
                    break;
                }
            }

            if (canSpawn)
            {
                // Spawn in a new audio source
                GameObject audio = Instantiate(sizzlingSFXSource, other.transform.position, Quaternion.identity);
                AudioSource source = audio.GetComponent<AudioSource>();

                source.minDistance = _3dAudioSettings.x;
                source.maxDistance = _3dAudioSettings.y;

                int randomClip = Random.Range(0, sizzlingAudios.Length);

                source.PlayOneShot(sizzlingAudios[randomClip], 1f);

                activeAudioSources.Add(audio);

                StartCoroutine(DeleteAudioSource(audio, dissolveTime));
            }
        } 

        // Checking to see if the object we have hit is on a dissolvable layer
        if (dissolvableObjects == (dissolvableObjects | 1 << other.layer))
        {
            // Checking to see if the Other GameObject is a FracturedChunk
            FracturedChunk chunk = other.GetComponent<FracturedChunk>();

            if (chunk != null)
            {
                // Seeing as we've hit a fractured chunk, we want to check to see if 
                // can be detached from it's parent object.
                if (!chunk.IsDetachedChunk)
                {
                    // Tell the chunk to be detached
                    chunk.DetachFromObject();

                    // Delete the DieTimer so that it doesn't interfere with Dissolve.cs
                    Destroy(other.GetComponent<UltimateFracturing.DieTimer>());
                    Destroy(other.GetComponent<ChunkDestroyer>());

                    // Add the Dissolve script onto the object.
                    //Debug.Log("Dissolving Object");
                    if (!other.GetComponent<DissolveObject>())
                    {
                        DissolveObject disO = other.AddComponent<DissolveObject>();

                        Spawn3dDissolveAudio(other);

                        //disO.SwapMaterialShader(shaderToUse);
                        disO.StartDissolve(acidDissolveColor, dissolveTime, dissolveProp, dissolveNoiseScale);
                    }
                }
            }
        }
    }

    private IEnumerator DeleteAudioSource(GameObject toRemove, float delay)
    {
        yield return new WaitForSeconds(delay);

        print("Destroying 3d Audio Source");

        activeAudioSources.Remove(toRemove);

        Destroy(toRemove);
    }

    private void Spawn3dDissolveAudio(GameObject other)
    {
        if (activeAudioSources.Count < 25)
        {
            GameObject audio = Instantiate(sizzlingSFXSource, other.transform.position, Quaternion.identity);
            AudioSource source = audio.GetComponent<AudioSource>();

            source.minDistance = _3dAudioSettings.x;
            source.maxDistance = _3dAudioSettings.y;

            int randomClip = Random.Range(0, sizzlingAudios.Length);

            source.PlayOneShot(sizzlingAudios[randomClip], 1f);

            activeAudioSources.Add(audio);

            StartCoroutine(DeleteAudioSource(audio, dissolveTime));
        }
    }
}
