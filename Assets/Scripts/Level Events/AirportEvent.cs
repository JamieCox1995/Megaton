using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirportEvent : LevelEvent
{
    public List<GameObject> AeroplanePrefabs;
    public List<MotionPath> MotionPaths;

    public int PlaneCountMinimum;
    public int PlaneCountMaximum;

    public float MinimumTimeToSpawn;
    public float MeanTimeToSpawn;

    private Queue<GameObject> _shuffledPrefabs;
    private Queue<MotionPath> _shuffledMotionPaths;
    private List<Aeroplane> _aeroplanes;

    private float _timeSinceLastSpawn;

    private void Start()
    {
        if (this.MotionPaths == null || this.MotionPaths.Count == 0)
        {
            this.MotionPaths = new List<MotionPath>(GetComponentsInChildren<MotionPath>());
        }

        _aeroplanes = new List<Aeroplane>();
        _shuffledPrefabs = new Queue<GameObject>(this.AeroplanePrefabs.Shuffle());
        _shuffledMotionPaths = new Queue<MotionPath>(this.MotionPaths.Shuffle());
    }

    private void Update()
    {
        if (!this.EventTriggered)
        {
            if (_aeroplanes.Count < this.PlaneCountMinimum)
            {
                if (_timeSinceLastSpawn > this.MinimumTimeToSpawn) SpawnPlane();
            }
            else if (_aeroplanes.Count < this.PlaneCountMaximum)
            {
                float threshold = Time.deltaTime / (this.MeanTimeToSpawn - this.MinimumTimeToSpawn);

                if (_timeSinceLastSpawn > this.MinimumTimeToSpawn && Random.value < threshold)
                {
                    SpawnPlane();
                }
            }
        }

        _timeSinceLastSpawn += Time.deltaTime;
    }

    private void SpawnPlane()
    {
        if (_shuffledPrefabs.Count == 0) EnqueueRange(_shuffledPrefabs, this.AeroplanePrefabs.Shuffle());
        if (_shuffledMotionPaths.Count == 0) EnqueueRange(_shuffledMotionPaths, this.MotionPaths.Shuffle());

        GameObject obj = GameObject.Instantiate(_shuffledPrefabs.Dequeue());
        Aeroplane component = obj.GetComponent<Aeroplane>();
        _aeroplanes.Add(component);

        component.MotionPath = _shuffledMotionPaths.Dequeue();
        component.OnPathComplete += DestroyPlaneObject;

        _timeSinceLastSpawn = 0f;
    }

    private void DestroyPlaneObject(Aeroplane obj)
    {
        _aeroplanes.Remove(obj);
        Destroy(obj.gameObject);
    }

    protected override IEnumerator DoEvent()
    {
        foreach (Aeroplane plane in _aeroplanes)
        {
            plane.IsCrashing = true;
        }

        yield return base.DoEvent();
    }

    private void EnqueueRange<T>(Queue<T> queue, IList<T> source)
    {
        foreach (T item in source)
        {
            queue.Enqueue(item);
        }
    }
}
