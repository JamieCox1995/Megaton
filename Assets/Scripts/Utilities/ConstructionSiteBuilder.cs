using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSiteBuilder : MonoBehaviour
{
    [Header("Construction Site Settings")]
    [SerializeField] private Vector2 zoneSize;
    [SerializeField] private float wallPrefabLength = 5f;
    [SerializeField] private GameObject[] wallPrefabs;
    private List<GameObject> walls = new List<GameObject>();

    [Header("Interior Prop Settings")]
    [SerializeField] private int propsToSpawn = 0;
    [SerializeField] private GameObject[] propPrefabs;
    private List<GameObject> props = new List<GameObject>();

    [Header("Crane Settings")]
    [SerializeField] private bool spawnCrane = false;
    [SerializeField] private Crane crane;

    private GameObject zone;
    private GameObject propContainer;
    private Vector2 lastSize = Vector2.zero;

    private void Clear()
    {
        foreach (GameObject wall in walls)
        {
            if (wall)
            {
                DestroyImmediate(wall);
            }
        }

        walls = new List<GameObject>();
    }

    public void Generate()
    {
        Clear();

        // Calculating all of the segments we will need for the length and width of the zone
        int lenghtSegments = (int)(zoneSize.x / wallPrefabLength);
        int widthSegments = (int)(zoneSize.y / wallPrefabLength);

        if (zone == null)
        {
            // Creating a new GameObject to store the walls we spawn
            zone = new GameObject();
            zone.name = "Construction Zone";
        }

        zone.transform.position = gameObject.transform.position;
        zone.transform.rotation = gameObject.transform.rotation;

        // Now we are going to spawn the north walls
        float halfLength = zoneSize.x / 2f;
        float halfWidth = zoneSize.y / 2f;

        GameObject fences = new GameObject();
        fences.transform.position = gameObject.transform.position;
        fences.transform.parent = zone.transform;
        fences.name = "Fences";

        #region Hacky Shit
        for (int index = 0; index < lenghtSegments; index++)
        {
            Vector3 position = new Vector3();
            position.z = zone.transform.position.z + halfWidth;

            position.x = zone.transform.position.x + (-halfLength + (wallPrefabLength * (index + 1)) - (wallPrefabLength / 2f));

            position.y = zone.transform.position.y;

            int prefab = Random.Range(0, wallPrefabs.Length);

            GameObject wall = Instantiate(wallPrefabs[prefab], position, Quaternion.identity, fences.transform);

            walls.Add(wall);
        }

        // South walls
        for (int index = 0; index < lenghtSegments; index++)
        {
            Vector3 position = new Vector3();
            position.z = zone.transform.position.z - halfWidth;

            position.x = zone.transform.position.x + (-halfLength + (wallPrefabLength * (index + 1)) - (wallPrefabLength / 2f));

            position.y = zone.transform.position.y;

            Quaternion rot = Quaternion.Euler(Quaternion.identity.x, Quaternion.identity.y + 180f, Quaternion.identity.z);

            int prefab = Random.Range(0, wallPrefabs.Length);

            GameObject wall = Instantiate(wallPrefabs[prefab], position, rot, fences.transform);

            walls.Add(wall);
        }

        for (int index = 0; index < widthSegments; index++)
        {
            Vector3 position = new Vector3();

            position.z = zone.transform.position.z + (-halfWidth + (wallPrefabLength * (index + 1)) - (wallPrefabLength / 2f));
            position.x = zone.transform.position.x + halfLength;

            position.y = zone.transform.position.y;

            Quaternion rot = Quaternion.Euler(Quaternion.identity.x, Quaternion.identity.y + 90f, Quaternion.identity.z);

            int prefab = Random.Range(0, wallPrefabs.Length);

            GameObject wall = Instantiate(wallPrefabs[prefab], position, rot, fences.transform);

            walls.Add(wall);
        }

        for (int index = 0; index < widthSegments; index++)
        {
            Vector3 position = new Vector3();

            position.z = zone.transform.position.z + (-halfWidth + (wallPrefabLength * (index + 1)) - (wallPrefabLength / 2f));
            position.x = zone.transform.position.x - halfLength;

            position.y = zone.transform.position.y;

            Quaternion rot = Quaternion.Euler(Quaternion.identity.x, Quaternion.identity.y - 90f, Quaternion.identity.z);

            int prefab = Random.Range(0, wallPrefabs.Length);

            GameObject wall = Instantiate(wallPrefabs[prefab], position, rot, fences.transform);

            walls.Add(wall);
        }
        #endregion

        fences.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void ClearProps()
    {
        foreach(GameObject prop in props)
        {
            if (prop)
            {
                DestroyImmediate(prop);
            }
        }

        props = new List<GameObject>();

        DestroyImmediate(propContainer);
    }

    public void SpawnProps()
    {
        ClearProps();
        crane.Clear();

        if (!propContainer)
        {
            propContainer = new GameObject();

            propContainer.transform.position = zone.transform.position;
            propContainer.name = "Props";

            propContainer.transform.parent = zone.transform;
        }


        if (propsToSpawn != 0 || propPrefabs.Length != 0)
        {
            for (int index = 0; index < propsToSpawn; index++)
            {
                int prefab = Random.Range(0, propPrefabs.Length);

                Vector3 position;
                Bounds objBounds = propPrefabs[prefab].GetComponent<Collider>().bounds;

                float halfLength = zoneSize.x / 2f;
                float halfWidth = zoneSize.y / 2f;

                position.x = Random.Range(-halfLength + (objBounds.extents.x * 2f), halfLength - (objBounds.extents.x * 2f));
                position.y = (objBounds.extents.y / 2f);
                position.z = Random.Range(-halfWidth + (objBounds.extents.z * 2f), halfWidth - (objBounds.extents.z * 2f));

                position += zone.transform.position;

                float randRotation = Random.rotation.y;

                GameObject prop = Instantiate(propPrefabs[prefab], position, new Quaternion(Quaternion.identity.x, randRotation, Quaternion.identity.z, Quaternion.identity.w), zone.transform);

                prop.transform.parent = propContainer.transform;

                props.Add(prop);

                
            }
        }

        propContainer.transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (!spawnCrane) return;

        // Create a random position within the radius of the centre
        Vector3 craneSpawn = zone.transform.position;
        craneSpawn.y = gameObject.transform.position.y;
        Vector2 circlePos = Random.insideUnitCircle * Mathf.Min(zoneSize.x - 20f, zoneSize.y - 20f);

        craneSpawn.x += circlePos.x;
        craneSpawn.z += circlePos.y;

        // Atm we are going to spawn a cube at the position
        //crane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject craneStart = new GameObject();
        craneStart.transform.position = craneSpawn;

        craneStart.name = "Crane Base";
        craneStart.transform.parent = zone.transform;

        Rigidbody rb = craneStart.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezePosition;

        crane.Create(craneStart);
}

[System.Serializable]
public class CraneSettings
{
    [Header("Crane Body")]
    public GameObject craneBodySection;
    public int craneBodyPieces = 5;

    [Header("Slewing Unit and Cab")]
    public GameObject slewingUnit;
    public GameObject slewingSections;
    public GameObject slewingEndCap;
    public int slewingSectionPieces = 2;

    [Header("Jib and Counter Jib")]
    public GameObject jibStart;
    public GameObject jibSection;
    public GameObject jibEnd;
    public int jibStartSection = 1;
    public int jibSectionsToSpawn = 5;
    public GameObject counterweightSection;
    public int counterWeightStart = 0;

    [Header("Trolley and Hook")]
    public GameObject trolley;
    public GameObject hook;
    public GameObject ropeLink;
}

[System.Serializable]
public class Crane
{
    public CraneSettings craneSettings;

    public List<GameObject> craneParts = new List<GameObject>();
    public GameObject crane;

    public void Clear()
    {
        // Here we just want to clear everything
        foreach(GameObject go in craneParts)
        {
            if (go)
            {
                DestroyImmediate(go);
            }
        }

        craneParts.Clear();

        if (crane)
        {
            DestroyImmediate(crane);
        }
    }

    public void Create(GameObject crane)
    {
            float currentHeight = 0f;

            GameObject craneSection;

            // Now we are going to spawn in the crane's body
            for (int index = 0; index < craneSettings.craneBodyPieces; index++)
            {
                craneSection = Instantiate(craneSettings.craneBodySection);

                PositionCraneSection(craneSection, crane.transform.position, crane.transform.rotation, currentHeight);

                FixedJoint sectionJoint = craneSection.GetComponent<FixedJoint>();

                if (index == 0)
                {
                    sectionJoint.connectedBody = crane.GetComponent<Rigidbody>();
                }
                else
                {
                    sectionJoint.connectedBody = craneParts[index - 1].GetComponent<Rigidbody>();
                }

                currentHeight += craneSection.GetComponent<Collider>().bounds.size.y;

                craneParts.Add(craneSection);

                craneSection.transform.parent = crane.transform;
            }

            // Now that we have the body, we can add the slewing unit and the slewing sections
            craneSection = Instantiate(craneSettings.slewingUnit);

            PositionCraneSection(craneSection, crane.transform.position, crane.transform.rotation, currentHeight);

            craneSection.GetComponent<FixedJoint>().connectedBody = craneParts[craneParts.Count - 1].GetComponent<Rigidbody>();
            currentHeight += craneSection.GetComponent<Collider>().bounds.size.y;

            craneParts.Add(craneSection);

            craneSection.transform.parent = crane.transform;

            List<GameObject> slewParts = new List<GameObject>();
            for (int index = 0; index < craneSettings.slewingSectionPieces + 1; index++)
            {
                if (index == craneSettings.slewingSectionPieces)
                {
                    // Spawn an end cap
                    craneSection = Instantiate(craneSettings.slewingEndCap);
                }
                else
                {
                    craneSection = Instantiate(craneSettings.slewingSections);
                }

                // We will want to spawn in the next part of the slew
                CraneSectionType type = (index == craneSettings.slewingSectionPieces) ? CraneSectionType.Mast : CraneSectionType.Slew;

                PositionCraneSection(craneSection, crane.transform.position, crane.transform.rotation, currentHeight, type);

                currentHeight += craneSection.GetComponent<Collider>().bounds.size.y;
                craneSection.GetComponent<FixedJoint>().connectedBody = craneParts[craneParts.Count - 1].GetComponent<Rigidbody>();

                craneParts.Add(craneSection);

                craneSection.transform.parent = crane.transform;

                slewParts.Add(craneSection);
            }


            // Now we want to spawn the counterjib
            GameObject counterJib = Instantiate(craneSettings.counterweightSection);

            Vector3 pos = slewParts[craneSettings.counterWeightStart].transform.position;
            pos.x += counterJib.GetComponent<Collider>().bounds.extents.x + 0.775f;
            counterJib.transform.position = pos;

            counterJib.GetComponent<FixedJoint>().connectedBody = slewParts[craneSettings.counterWeightStart].GetComponent<Rigidbody>();

            craneParts.Add(counterJib);

            counterJib.transform.parent = crane.transform;

            // Now we are going to spawn in the jib
            float jibLength = 0f;

            int trollySpawn = Random.Range(0, craneSettings.jibSectionsToSpawn);

            for (int index = 0; index < craneSettings.jibSectionsToSpawn + 1; index++)
            {
                GameObject jibSection;

                if (index == 0)
                {
                    // Spawn a Jib Start piece
                    jibSection = Instantiate(craneSettings.jibStart);
                    Vector3 position = slewParts[craneSettings.jibStartSection].transform.position;
                    position.x -= jibSection.GetComponent<Collider>().bounds.extents.x + 1.2487f;
                    jibLength += jibSection.GetComponent<Collider>().bounds.extents.x + 1.2487f;

                    jibSection.transform.position = position;


                    jibSection.GetComponent<FixedJoint>().connectedBody = slewParts[craneSettings.jibStartSection].GetComponent<Rigidbody>();

                    craneParts.Add(jibSection);

                    jibSection.transform.parent = crane.transform;
                }
                else if (index == craneSettings.jibSectionsToSpawn)
                {
                    // Spawn End B O I
                    jibSection = Instantiate(craneSettings.jibEnd);
                    Vector3 position = slewParts[craneSettings.jibStartSection].transform.position;
                    position.x -= jibLength + jibSection.GetComponent<Collider>().bounds.size.x - 1.0317f;
                    jibLength += jibSection.GetComponent<Collider>().bounds.size.x;

                    jibSection.transform.position = position;


                    jibSection.GetComponent<FixedJoint>().connectedBody = craneParts[craneParts.Count - 1].GetComponent<Rigidbody>();

                    craneParts.Add(jibSection);

                    jibSection.transform.parent = crane.transform;
                }
                else
                {
                    jibSection = Instantiate(craneSettings.jibSection);
                    Vector3 position = slewParts[craneSettings.jibStartSection].transform.position;
                    position.x -= jibLength + jibSection.GetComponent<Collider>().bounds.size.x;
                    jibLength += jibSection.GetComponent<Collider>().bounds.size.x;


                    jibSection.transform.position = position;

                    jibSection.GetComponent<FixedJoint>().connectedBody = craneParts[craneParts.Count - 1].GetComponent<Rigidbody>();

                    craneParts.Add(jibSection);

                    jibSection.transform.parent = crane.transform;
                }

                if (index == trollySpawn)
                {
                    // Spawn the trolley game object here
                    GameObject trolley = Instantiate(craneSettings.trolley, jibSection.transform.position, jibSection.transform.rotation, jibSection.transform);

                    trolley.GetComponent<FixedJoint>().connectedBody = craneParts[craneParts.Count - 1].GetComponent<Rigidbody>();

                    GameObject hook = Instantiate(craneSettings.hook, crane.transform);

                    Vector3 hookPos = trolley.transform.position;
                    // Here we are creating a random length to apply to the hook object
                    float bobLength = Random.Range(10f, (int)currentHeight - 10f);

                    hookPos.y -= bobLength + 1f;

                    hook.transform.position = hookPos;
                    hook.transform.rotation = trolley.transform.rotation;

                    craneParts.Add(trolley);
                    craneParts.Add(hook);

                    // Now we want to iterate over all of the bob length to spawn the 'chain'
                    int ropeLinksToSpawn = (int)(bobLength);

                    GameObject link;
                    List<GameObject> links = new List<GameObject>();

                    Vector3 startPos = trolley.GetComponentInChildren<Transform>().position;
                    startPos.y -= 0.5f; 

                    for(int i = 0; i < ropeLinksToSpawn; i++)
                    {
                        link = Instantiate(craneSettings.ropeLink);

                        startPos.y -= link.GetComponent<Collider>().bounds.size.y;

                        link.transform.position = startPos;

                        if (i == 0)
                        {
                            link.GetComponent<HingeJoint>().connectedBody = trolley.GetComponent<Rigidbody>();
                        }
                        else
                        {
                            link.GetComponent<HingeJoint>().connectedBody = links[links.Count - 1].GetComponent<Rigidbody>();
                        }

                        craneParts.Add(link);

                        link.transform.parent = crane.transform;

                        links.Add(link);
                    }


                    hook.GetComponent<HingeJoint>().connectedBody = links[links.Count - 1].GetComponent<Rigidbody>();
                }
            }
        }

        private void PositionCraneSection(GameObject newSection, Vector3 cranePosition, Quaternion craneRotation, float currentCraneHeight, CraneSectionType type = CraneSectionType.Body)
        {
            Vector3 positionToPlace = cranePosition;

            if (type != CraneSectionType.Mast)
            {
                positionToPlace.y = (currentCraneHeight + newSection.GetComponent<Collider>().bounds.extents.y) + cranePosition.y;
            }
            else
            {
                positionToPlace.y = currentCraneHeight;
            }

            newSection.transform.position = positionToPlace;
            newSection.transform.rotation = craneRotation;
        }
    }
}

public enum CraneSectionType
{
    Body,
    Slew,
    Mast,
}

