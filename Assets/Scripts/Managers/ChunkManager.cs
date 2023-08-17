using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class ChunkManager : MonoBehaviour
{
    public float minForce = 150f;

    private FracturedObject[] fracturedObjects;

    private Octree<FracturedObject> _octree;

    //private List<TrackableCoroutine> _runningCoroutines;

	// Use this for initialization
	void Start ()
    {
        GameEventManager.instance.onExplosionEvent += ChunkExplosionEvent;
        GameEventManager.instance.onCancelCoroutinesRequested += CancelCoroutines;

        fracturedObjects = FindObjectsOfType<FracturedObject>() as FracturedObject[];
        _octree = new Octree<FracturedObject>(10, 4, fracturedObjects);

        minForce = fracturedObjects.Min( f => f.explosionForceThreshold);
        //_runningCoroutines = new List<TrackableCoroutine>();
	}

    private void CancelCoroutines()
    {
        //Debug.Log("Cancel Coroutines");

        //foreach (TrackableCoroutine routine in _runningCoroutines)
        //{
        //    if (routine.IsRunning) StopCoroutine(routine);
        //}

        //_runningCoroutines.Clear();

        StopAllCoroutines();
    }

    //private void Update()
    //{
    //    CleanCoroutines();
    //}

    //private void CleanCoroutines()
    //{
    //    _runningCoroutines.RemoveAll(routine => routine.IsRunning == false);
    //}

    public void ChunkExplosionEvent(ExplosionEvent explosion)
    {
        //foreach(FracturedObject obj in GetObjectsInRadius(explosion))
        //{
        //    obj.Explode(explosion.position, explosion.size, explosion.radius, false, false, true, true);
        //}

        //this.StartCoroutineAndMatchTargetFrameRate(ChunkExplosionCoroutine(explosion, GetObjectsInRadius(explosion)));

        // the force applied at distance (d) is proportional to 1 / (d^2 + 1).

        float maxRadius = Mathf.Sqrt((Mathf.Abs(explosion.size) / minForce) - 1f);
        Debug.LogFormat("Max Radius: {0}", maxRadius);


        List<FracturedObject> affectedObjs = GetObjectsInRadius(explosion.position, maxRadius);

        TrackableCoroutine routine = ChunkExplosionCoroutine(explosion.position, explosion.size, maxRadius, affectedObjs).MatchTargetFrameRate().AsTrackable();

        StartCoroutine(routine);

        //_runningCoroutines.Add(routine);
    }

    public void ShapedExplosion(Transform explosionTransform, float angle, float size)
    {
        float maxRadius = Mathf.Sqrt((Mathf.Abs(size) / minForce) - 1f);
        List<FracturedObject> inRadius = GetObjectsInRadius(explosionTransform.position, maxRadius);

        List<FracturedObject> affected = new List<FracturedObject>();

        foreach(FracturedObject obj in inRadius)
        {
            Vector3 directionTo = obj.transform.position - explosionTransform.position;

            float dirAngle = Vector3.Angle(explosionTransform.forward, directionTo);

            if (dirAngle <= angle)
            {
                affected.Add(obj);
            }
        }

        Debug.LogFormat("{0} entries in inRadius, {1} entries in affected", inRadius.Count, affected.Count);

        TrackableCoroutine routine = ChunkExplosionCoroutine(explosionTransform.position, size, maxRadius, affected).MatchTargetFrameRate().AsTrackable();
        StartCoroutine(routine);
    }

    IEnumerator ChunkExplosionCoroutine(Vector3 pos, float size, float radius, List<FracturedObject> affectedObjects)
    {
        IEnumerable<FracturedObject> sortedObjects =
            affectedObjects.OrderBy(fracturedObject => Vector3.Distance(fracturedObject.transform.position, pos));

        foreach (FracturedObject obj in sortedObjects)
        {
            obj.Explode(pos, size, radius, false, false, true, true);
            yield return null;
        }
    }

    private List<FracturedObject> GetObjectsInRadius(Vector3 pos, float radius)
    {
        //Profiler.BeginSample("Creating the Octree");
        //Octree<FracturedObject> octree = new Octree<FracturedObject>(8, 4, fracturedObjects);
        //Profiler.EndSample();
        Profiler.BeginSample("Update Octree");
        _octree.Update();
        Profiler.EndSample();

        Profiler.BeginSample("Calculating items in radius");
        List<FracturedObject> result = _octree.IntersectSphere(pos, radius).ToList();
        Profiler.EndSample();

        return result;
    }
}
