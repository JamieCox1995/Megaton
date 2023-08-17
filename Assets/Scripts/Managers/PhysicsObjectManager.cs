using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class PhysicsObjectManager : MonoBehaviour
{
    public List<PhysObject> physicsObjects = new List<PhysObject>();

    private Octree<PhysObject> _octree = new Octree<PhysObject>(10, 4);

    private float minForce;

	// Use this for initialization
	void Start ()
    {
        physicsObjects = FindObjectsOfType<PhysObject>().ToList();
        _octree.Insert(physicsObjects);

        if (physicsObjects.Count > 0) minForce = physicsObjects.Min(f => f.damageThreshold);

        GameEventManager.instance.onExplosionEvent += OnExplosion;
	}

    private void OnExplosion(ExplosionEvent eventData)
    {
        //Profiler.BeginSample("Creating Octree");
        //Octree<PhysicsObject> octree = new Octree<PhysicsObject>(8, 4, physicsObjects);
        //Profiler.EndSample();
        _octree.Update();

        float maxRadius = Mathf.Sqrt((Mathf.Abs(eventData.size) / minForce) - 1f);

        List<PhysObject> result = _octree.IntersectSphere(eventData.position, maxRadius).ToList();

        this.StartCoroutineAndMatchTargetFrameRate(ExplosionReactionCoroutine(eventData.position, eventData.size, maxRadius, result));
    }

    private static IEnumerator ExplosionReactionCoroutine(Vector3 position, float size, float radius, List<PhysObject> affectedObjects)
    {
        //SortedList<float, PhysicsObject> sortedObjects =
        //    new SortedList<float, PhysicsObject>(affectedObjects.ToDictionary(physicsObject => Vector3.Distance(physicsObject.transform.position, eventData.position)));

        var sortedObjects = affectedObjects.Select(physicsObject => new { distance = Vector3.Distance(physicsObject.transform.position, position), physicsObject }).OrderBy(x => x.distance);

        //foreach (KeyValuePair<float, PhysicsObject> pair in sortedObjects)
        foreach (var obj in sortedObjects)
        {
            //float distance = pair.Key;
            //PhysicsObject physicsObject = pair.Value;

            float distance = obj.distance;
            PhysObject physicsObject = obj.physicsObject;

            if (physicsObject == null) continue;

            RaycastHit hit;

            if (Physics.Raycast(position, (physicsObject.transform.position - position).normalized, out hit, obj.distance))
            {
                if (hit.collider.gameObject == physicsObject.gameObject)
                {
                    //physicsObject.ReactToExplosion(position, distance, radius, size);
                    physicsObject.OnExplosion(position, distance, radius, size);
                    yield return null;
                }
            }
        }
    }
}
