using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShippingContainerSpawner : MonoBehaviour
{
    [Header("GameObjects To Spawn")]
    public GameObject[] containerPrefabs;

    public GameObject railGuidePrefab;
    public GameObject gantryCranePrefab;

    [Header("Container Spawn Settings")]
    public Vector3Int containerAreaSize = new Vector3Int(5, 4, 10);

    public Vector2 offset = new Vector2(1.5f, 0.2f);

    [Header("Rail and Crane Spawn Settings")]
    public float railDistance = 21f;
    public float railOffset = 10.5f;
    private float railLength = 0f;

    [Header("Object Randomisation")]
    [Tooltip("The Minimum and Maximum jitter to be applied to each spawned Container. X = Minimum, Y = Maximum")]
    public Vector2 rotationJitter;
    public Vector3 positionJitter = new Vector3();
    [SerializeField] private Vector3 containerBoundsSize = new Vector3();

    public bool spawnGantryAtStart = true;

    [Header("Spawned Objects")]
    [SerializeField]
    private List<ContainerStack> containers = new List<ContainerStack>();

    [SerializeField] private GameObject containerParent;
    [SerializeField] private GameObject railParent;
    [SerializeField] private GameObject gantryObject;

    public void CreateContainerArea()
    {
        SpawnContainers();

        // Now we want to spawn in the rails and the crane
        SpawnGuideRails();
    }

    public void SpawnContainers()
    {
        containerParent = new GameObject("Containers");
        containerParent.transform.parent = transform;

        // Going through each row and column of the container area
        for (int xIndex = 0; xIndex < containerAreaSize.x; xIndex++)
        {
            for (int zIndex = 0; zIndex <containerAreaSize.z; zIndex++)
            {
                containers.Add(new ContainerStack());

                // We want to generate a number between 0 and containerAreaSize.y to be the height of the stack of containers.
                int stackHeight = Random.Range(0, containerAreaSize.y);

                // Checking to see if there are any containers in this stack. If not, we're moving onto the next zIndex.
                if (stackHeight == 0) continue;

                // Iterating over all of the stack indexes
                for (int yIndex = 0; yIndex < stackHeight; yIndex++)
                {

                    // We now finally want to spawn in the container prefab
                    int prefabIndex = Random.Range(0, containerPrefabs.Length);

                    GameObject container = Instantiate(containerPrefabs[prefabIndex], containerParent.transform);

                    if (containerBoundsSize == Vector3.zero) containerBoundsSize = container.GetComponent<Collider>().bounds.size;

                    Vector3 position;
                    float halfX = (containerAreaSize.x * containerBoundsSize.x) / 2f;
                    float halfZ = (containerAreaSize.z * containerBoundsSize.z) / 2f;

                    // Here is where we are calculating the position of the containers. They are arranged so that they are centred around this object.
                    position.x = transform.position.x + (-halfX + ((containerBoundsSize.x + offset.x) * (xIndex + 1)) - (containerBoundsSize.x / 2f));
                    position.y = transform.position.y + containerBoundsSize.y * yIndex;
                    position.z = transform.position.z + (-halfZ + ((containerBoundsSize.z + offset.y) * (zIndex + 1)) - (containerBoundsSize.z / 2f));

                    container.transform.position = position;

                    // Calculating the random Y rotation to be applied to the container
                    Vector3 rJitter = new Vector3();
                    rJitter.y = Random.Range(rotationJitter.x, rotationJitter.y);

                    Quaternion rot = Quaternion.Euler(rJitter);
                    container.transform.rotation = rot;

                    // Calculating some final positional jitter
                    Vector3 positionalJitter = new Vector3();
                    positionalJitter.x = Random.Range(-positionJitter.x, positionJitter.x);
                    positionalJitter.z = Random.Range(-positionJitter.z, positionJitter.z);

                    container.transform.position += positionalJitter;

                    containers[containers.Count - 1].stack.Add(container);
                }
            }
        }
    }

    public void SpawnGuideRails()
    {
        // Creating the railParent
        railParent = new GameObject("Rail Guides");
        railParent.transform.parent = transform;

        float railLength = (containerBoundsSize.x * containerAreaSize.x) + 10f;

        #region Rail Spawning
        // Here we are spawning both of the rails and the containers are centred, we can spawn the rails and offset them by railDistance/2 +/- railOffset
        GameObject rail1 = Instantiate(railGuidePrefab, transform.position, Quaternion.identity, railParent.transform);
        Vector3 railPosition = transform.position;
        railPosition.z -= (railDistance / 2f) + railOffset;
        railPosition.y -= 0.4f;

        Vector3 railScale = new Vector3(railLength, 1f, 0.3f);

        rail1.transform.position = railPosition;
        rail1.transform.localScale = railScale;

        GameObject rail2 = Instantiate(railGuidePrefab, transform.position, Quaternion.identity, railParent.transform);
        railPosition.z += railDistance;

        rail2.transform.position = railPosition;
        rail2.transform.localScale = railScale;
        #endregion

        // Do we want to work out the start and end points for the crane here too?
        Vector3 gantryStart = transform.position;
        gantryStart.x -= ((containerBoundsSize.x * containerAreaSize.x) / 2f) - (containerBoundsSize.x / 2f);
        gantryStart.z -= railOffset;

        Vector3 gantryEnd = transform.position;
        gantryEnd.x = gantryStart.x + ((containerBoundsSize.x + offset.x) * (containerAreaSize.x - 1));

        // Spawning the Crane
        Vector3 gantryPosition = spawnGantryAtStart == true ? gantryStart : Vector3.Lerp(gantryStart, gantryEnd, Random.value);
        gantryPosition.z = gantryStart.z;

        GameObject gantry = Instantiate(gantryCranePrefab, gantryPosition, transform.rotation, transform);
        gantryObject = gantry;

        GantryCrane crane = gantry.GetComponent<GantryCrane>();
        crane.parent = this;
        crane.SetContainerInfo(containerAreaSize, containerBoundsSize);
    }

    public List<ContainerStack> GetContainers()
    {
        return containers;
    }

    public Vector3 GetContainerSize()
    {
        return containerBoundsSize;
    }

    public void ClearArea()
    {
        ClearContainers();
        ClearRailGuides();
    }

    public void ClearContainers()
    {
        foreach(LODGroup trans in GetComponentsInChildren<LODGroup>())
        {
            if (trans.gameObject != gameObject)
            {
                if (trans.gameObject != null) DestroyImmediate(trans.gameObject);
            }
        }

        if (containerParent != null)
        {
            DestroyImmediate(containerParent);
        }

        containers.Clear();

        Debug.Log("Destroying Spawned Containers");
    }

    public void ClearRailGuides()
    {
        if (railParent != null)
        {
            foreach (Transform trans in railParent.GetComponentsInChildren<Transform>())
            {
                if (trans != transform && trans != null)
                {
                    DestroyImmediate(trans.gameObject);
                }
            }

            if (railParent != null) DestroyImmediate(railParent);
        }

        if (gantryObject != null)
        {
            DestroyImmediate(gantryObject);
        }
    }

    public Vector3 GetStackLocation(int xIndex, int zIndex)
    {
        Vector3 position = transform.position;

        float halfX = (containerAreaSize.x * containerBoundsSize.x) / 2f;
        float halfZ = (containerAreaSize.z * containerBoundsSize.z) / 2f;

        position.x += (-halfX + ((containerBoundsSize.x + offset.x) * (xIndex + 1)) - (containerBoundsSize.x / 2f));
        position.z += (-halfZ + ((containerBoundsSize.z + offset.y) * (zIndex + 1)) - (containerBoundsSize.z / 2f));

        return position;
    }

    public ContainerStack GetContainerStack(int xIndex, int zIndex)
    {
        int index = xIndex * containerAreaSize.z + zIndex;

        ContainerStack stack = containers[index];

        return stack;
    }

    public GameObject GetContainerParent()
    {
        return containerParent;
    }
}

[System.Serializable]
public class ContainerStack
{
    public List<GameObject> stack = new List<GameObject>();
}
