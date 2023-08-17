using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class AnimatedWater : MonoBehaviour
{
    private GameObject _attachedGameObject;
    private Mesh _waterMesh;
    private BoxCollider _collider;
    private List<Collider> _currentCollisions;

    public Vector2 WaterScale;
    public int Divisions;

    public Material WaterMaterial;
    public WaveInfo WaveInfo;
    public GameObject SplashParticleSystem;

	// Use this for initialization
	void Start()
    {
        _attachedGameObject = this.gameObject;
        _currentCollisions = new List<Collider>();

        Vector3 waterScale = new Vector3(this.WaterScale.x, 1, this.WaterScale.y);

        //Surface waterSurface = new PlaneSurface(Vector3.zero, Vector3.zero, waterScale, this.Divisions, this.Divisions);

        _waterMesh = GeneratePlane(waterScale, this.Divisions, this.Divisions); //waterSurface.ToMesh();

        //_collider = _attachedGameObject.AddComponent<BoxCollider>();

        //_collider.size = new Vector3(waterScale.x, this.WaveInfo.Height, waterScale.z);
        //_collider.isTrigger = true;

        GameObject water = MeshToGameObject(_waterMesh, this.WaterMaterial);
        water.name = "Water";
        water.transform.parent = _attachedGameObject.transform;
        water.transform.position = _attachedGameObject.transform.position;
        water.transform.rotation = _attachedGameObject.transform.rotation;
        water.transform.localScale = _attachedGameObject.transform.localScale;
	}
	
	// Update is called once per frame
	void Update()
    {
        _waterMesh.vertices = _waterMesh.vertices.Select(vertex => new Vector3(vertex.x, GetWaveHeight(this.WaveInfo, vertex.x, vertex.z, Time.time), vertex.z)).ToArray();
        _waterMesh.RecalculateNormals();
	}


    //void OnCollisionStay(Collision collision)
    //{
    //    bool isSubmerged = true;

    //    foreach (ContactPoint contact in collision.contacts)
    //    {
    //        if (contact.point.y > GetWaveHeight(this.WaveInfo, contact.point.x, contact.point.z, Time.time))
    //        {
    //            isSubmerged = false;
    //            break;
    //        }
    //    }

    //    if (isSubmerged)
    //    {
    //        collision.gameObject.SetActive(false);
    //    }
    //}

    void OnTriggerStay(Collider other)
    {
        Vector3 position = other.transform.position - _attachedGameObject.transform.position;
        if (position.y <= GetWaveHeight(this.WaveInfo, position.x, position.z, Time.time) && this.SplashParticleSystem != null)
        {
            if (!_currentCollisions.Contains(other))
            {
                //AudioClip clipToPlay = AudioManager.instance.GetSound("Splash");
                //AudioManager.instance.PlaySound(clipToPlay);
                //Utilities.SpawnObjectAtPosition(this.SplashParticleSystem, other.transform.position);
                GameObject.Instantiate<GameObject>(this.SplashParticleSystem);
                _currentCollisions.Add(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_currentCollisions.Contains(other)) _currentCollisions.Remove(other);
    }

    /// <summary>
    /// Gets the wave height from the specified parameters.
    /// </summary>
    /// <param name="waveInfo">The wave information.</param>
    /// <param name="x">The X coordinate of the current vertex.</param>
    /// <param name="z">The Z coordinate of the current vertex.</param>
    /// <param name="t">The current time.</param>
    /// <returns>The Y coordinate of the current vertex, displaced as if by a wave.</returns>
    private float GetWaveHeight(WaveInfo waveInfo, float x, float z, float t)
    {

        return waveInfo.Height * (Mathf.Sin(x / waveInfo.HorizontalScale + t * waveInfo.TimeScale) + Mathf.Cos(z / waveInfo.VerticalScale + t * waveInfo.TimeScale)) / 4; //* 0.5f;
    }

    /// <summary>
    /// Converts a mesh to a game object.
    /// </summary>
    /// <param name="mesh">The mesh to instantiate as a game object.</param>
    /// <param name="material">The material to give to the game object.</param>
    /// <returns>The instantiated game object.</returns>
    GameObject MeshToGameObject(Mesh mesh, Material material)
    {
        GameObject obj = new GameObject();
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = material;

        return obj;
    }

    private Mesh GeneratePlane(Vector3 scale, int horizontalDivisions, int verticalDivisions)
    {
        Mesh result = new Mesh();

        Vector3[] vertices = new Vector3[(horizontalDivisions + 1) * (verticalDivisions + 1)];
        //int[] indices = new int[horizontalDivisions * verticalDivisions * 3];
        List<int> indices = new List<int>();

        int stride = horizontalDivisions + 1;

        for (int y = 0; y <= verticalDivisions; y++)
        {
            for (int x = 0; x <= horizontalDivisions; x++)
            {
                float xFactor = (float)x / horizontalDivisions;
                float yFactor = (float)y / verticalDivisions;

                vertices[y * stride + x] = new Vector3((xFactor - 0.5f) * scale.x, 0, (yFactor - 0.5f) * scale.z);

                //vertices[y * stride + x] = new Vector3(scale.x * x / horizontalDivisions - (scale.x / 2f), 0, scale.z * y / verticalDivisions - (scale.z / 2f));
            }
        }

        result.vertices = vertices;

        //int i = 0;

        for (int y = 0; y < verticalDivisions; y++)
        {
            for (int x = 0; x < horizontalDivisions; x++)
            {
                //indices[i++] = x + y * stride;
                //indices[i++] = x + (y + 1) * stride;
                //indices[i++] = (x + 1) + y * stride;
                //indices[i++] = x + (y + 1) * stride;
                //indices[i++] = (x + 1) + (y + 1) * stride;
                //indices[i++] = (x + 1) + y * stride;

                indices.Add(x + y * stride);
                indices.Add(x + (y + 1) * stride);
                indices.Add((x + 1) + y * stride);
                indices.Add(x + (y + 1) * stride);
                indices.Add((x + 1) + (y + 1) * stride);
                indices.Add((x + 1) + y * stride);
            }
        }

        result.triangles = indices.ToArray();

        return result;
    }
}

/// <summary>
/// Represents wave information for the animated water.
/// </summary>
[Serializable]
public class WaveInfo
{
    /// <summary>
    /// The difference in height between a peak and a trough in the wave.
    /// </summary>
    public float Height;

    /// <summary>
    /// The horizontal scale of the wave.
    /// </summary>
    public float HorizontalScale;

    /// <summary>
    /// The vertical scale of the wave.
    /// </summary>
    public float VerticalScale;

    /// <summary>
    /// The speed of the wave.
    /// </summary>
    public float TimeScale;
}
