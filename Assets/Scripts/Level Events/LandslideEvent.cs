using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class LandslideEvent : LevelEvent
{
    [Header("Related GameObjects")]
    public GameObject RockWall;
    public GameObject Soil;

    [Header("Particles")]
    public GameObject DustParticleSystem;

    [Header("Cluster Settings")]
    public int ClusterSpawnBursts = 5;
    public float ClusterSpawnInterval = 0.1f;
    public int ClusterSpawnMinimum = 30;
    public int ClusterSpawnMaximum = 60;

    [Header("Rock Spawner Properties")]
    public RockSpawner[] RockSpawners;

    private Vector3 collisionPosition;
    private Vector3 collisionDirection;

    private void OnValidate()
    {
        if (this.RockSpawners != null)
        {
            for (int i = 0; i < this.RockSpawners.Length; i++)
            {
                this.RockSpawners[i].Normal.Normalize();
            }
        }
    }

    private void Start()
    {
        var trigger = this.RockWall.AddComponent<LandslideEventTrigger>();
        trigger.LevelEvent = this;
        trigger = this.Soil.AddComponent<LandslideEventTrigger>();
        trigger.LevelEvent = this;
    }

    protected override void OnEventStart()
    {
        GameObject.Instantiate(this.DustParticleSystem, this.collisionPosition, Quaternion.FromToRotation(Vector3.up, collisionDirection));

        base.OnEventStart();
    }

    protected override void OnEventEnd()
    {
        base.OnEventEnd();
    }

    protected override IEnumerator DoEvent()
    {
        for (int i = 0; i < this.ClusterSpawnBursts; i++)
        {
            foreach (var spawner in this.RockSpawners)
            {
                int count = Random.Range(this.ClusterSpawnMinimum, this.ClusterSpawnMaximum);
                spawner.SpawnRocks(this.transform.position, count);
            }

            yield return new WaitForSeconds(ClusterSpawnInterval);
        }

        yield return base.DoEvent();
    }



    private void OnDrawGizmosSelected()
    {
        if (this.RockSpawners != null)
        {
            var gizmoColor = Gizmos.color;

            foreach (var spawner in this.RockSpawners)
            {
                var position = this.transform.position + spawner.Position;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(position, spawner.EmitterRadius);

                Gizmos.color = Color.green;
                var spawnerMinimumDirection = spawner.Normal * spawner.MinimumSpeed;
                Gizmos.DrawRay(position, spawnerMinimumDirection);

                Gizmos.color = Color.yellow;
                var spawnerMaximumDirection = spawner.Normal * (spawner.MaximumSpeed - spawner.MinimumSpeed);
                Gizmos.DrawRay(position + spawnerMinimumDirection, spawnerMaximumDirection);
            }

            Gizmos.color = gizmoColor;
        }
    }

    private class LandslideEventTrigger : LevelEventTrigger<LandslideEvent>
    {
        protected override void OnCollisionEnter(Collision collision)
        {
            ContactPoint contactPoint = collision.contacts[0];
            this.LevelEvent.collisionPosition = contactPoint.point;
            this.LevelEvent.collisionDirection = contactPoint.normal;
            this.LevelEvent.StartLevelEvent();
        }
    }

    [Serializable]
    public class RockSpawner
    {
        public Vector3 Position;
        public Vector3 Normal;
        public GameObject[] RockPrefabs;
        public float EmitterRadius = 5f;
        public float MinimumScale = 0.5f;
        public float MaximumScale = 5f;
        public float MaximumAngleDeflection = 30f;
        public float MinimumSpeed = 10f;
        public float MaximumSpeed = 30f;

        private IEnumerable<GameObject> rocks;
        private int index = 0;

        public void SpawnRock(Vector3 origin)
        {
            SpawnRocks(origin, 1);
        }

        public void SpawnRocks(Vector3 origin, int count)
        {
            if (rocks == null) rocks = GetShuffledObjects(this.RockPrefabs);

            foreach (var prefab in rocks.Skip(index).Take(count))
            {
                var position = origin + this.Position + (Random.insideUnitSphere * this.EmitterRadius);

                var obj = GameObject.Instantiate(prefab, position, Random.rotationUniform);

                var scale = new Vector3(Random.Range(this.MinimumScale, this.MaximumScale),
                                        Random.Range(this.MinimumScale, this.MaximumScale),
                                        Random.Range(this.MinimumScale, this.MaximumScale));

                var rb = obj.GetComponent<Rigidbody>();

                obj.transform.localScale = scale;
                rb.mass *= (scale.x * scale.y * scale.z);

                Vector3 direction = DeflectDirectionRandom(this.Normal, this.MaximumAngleDeflection);
                float speed = Random.Range(this.MinimumSpeed, this.MaximumSpeed);

                rb.AddForce(direction * speed, ForceMode.VelocityChange);
            }
        }

        private static Vector3 DeflectDirectionRandom(Vector3 direction, float maxDeflection)
        {
            var target = Random.rotationUniform * Vector3.forward;

            if (Vector3.Dot(direction, target) < 0)
            {
                target = -target;
            }

            float maxRadians = maxDeflection * Mathf.Deg2Rad;

            return Vector3.RotateTowards(direction, target, maxRadians, 0f);
        }

        private static IEnumerable<GameObject> GetShuffledObjects(GameObject[] source)
        {
            GameObject[] objects = source.Shuffle();

            int i = 0;
            while (true)
            {
                if (i == objects.Length)
                {
                    i = 0;
                    objects.ShuffleInPlace();
                }

                yield return objects[i];
                i++;
            }
        }
    }
}
