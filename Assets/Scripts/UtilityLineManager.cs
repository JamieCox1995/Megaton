using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class UtilityLineManager : MonoBehaviour {

    [Header("Visual Properties")]
    public GameObject UtilityPoleInstance;

    public Material CableMaterial;

    public int LineSegmentCount = 12;

    public float CableThickness = 0.05f;

    [Header("Generation Config")]
    public List<UtilityConnector> Connectors;

    public List<UtilityLine> UtilityLines;

    //private List<GameObject> _children = new List<GameObject>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            // do nothing if the native object has been destroyed
            if (!this) return;

            Refresh();
        };
    }
#endif

    public void Refresh()
    {
        ClearChildren();

        Generate();
    }

    private void Generate()
    {
#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.GetPrefabType(gameObject) == UnityEditor.PrefabType.Prefab) return;
#endif
        List<GameObject> instances = InstantiateUtilityLineInstances();

        for (int i = 0; i < instances.Count; i++)
        {
            ConnectSelf(instances[i], this.Connectors);
        }

        foreach (UtilityLine line in this.UtilityLines)
        {
            ConnectUtilityLine(line);
        }
    }

    public void ClearChildren()
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        children.ForEach(child => DestroyImmediate(child));
        
    }

    private List<GameObject> InstantiateUtilityLineInstances()
    {
        if (this.UtilityLines == null) return new List<GameObject>();

        List<GameObject> utilityLines = new List<GameObject>();

        foreach (UtilityLine line in this.UtilityLines)
        {
            GameObject obj = GameObject.Instantiate(this.UtilityPoleInstance, line.Position, Quaternion.Euler(line.Rotation));
            obj.transform.SetParent(gameObject.transform);

            utilityLines.Add(obj);
        }

        return utilityLines;
    }

    private void ConnectSelf(GameObject instance, List<UtilityConnector> connectors)
    {
        var ids = connectors.Select(connector => connector.Id).Distinct();
        var connectorTypes = connectors.GroupBy(connector => connector.Type).ToDictionary(grp => grp.Key, grp => grp.ToList());

        List<GameObject> cables = new List<GameObject>();

        foreach (int id in ids)
        {
            UtilityConnector inConnector = connectorTypes[ConnectorType.In].FirstOrDefault(connector => connector.Id == id);
            UtilityConnector outConnector = connectorTypes[ConnectorType.Out].FirstOrDefault(connector => connector.Id == id);

            if (inConnector != null && outConnector != null)
            {
                GameObject obj = new GameObject("Connector " + id);

                obj.transform.SetParent(instance.transform);

                RopeRenderer cable = obj.AddComponent<RopeRenderer>();
                cable.RopeLength = 1.25f * Vector3.Distance(inConnector.Position, outConnector.Position);
                cable.PointA = Transform(inConnector.Position, instance.transform.position, instance.transform.rotation);
                cable.PointB = Transform(outConnector.Position, instance.transform.position, instance.transform.rotation);
                cable.RopeMaterial = this.CableMaterial;
                cable.RopeThickness = this.CableThickness;
                cable.LineSegmentCount = this.LineSegmentCount;

                cable.Generate();
            }
        }
    }

    private void ConnectUtilityLine(UtilityLine line)
    {
        if (this.UtilityLines == null || line.Previous < 0) return;

        UtilityLine previous = this.UtilityLines[line.Previous];

        var lineConnectors = this.Connectors
                                 .Where(connector => connector.Type == ConnectorType.In)
                                 .ToDictionary(connector => connector.Id, connector => Transform(connector.Position, line.Position, Quaternion.Euler(line.Rotation)));
        var previousConnectors = this.Connectors
                                     .Where(connector => connector.Type == ConnectorType.Out)
                                     .ToDictionary(connector => connector.Id, connector => Transform(connector.Position, previous.Position, Quaternion.Euler(previous.Rotation)));

        var connectorIds = this.Connectors.Select(connector => connector.Id).Distinct();

        foreach (int id in connectorIds)
        {
            Vector3 pointA, pointB;

            if (lineConnectors.TryGetValue(id, out pointA) && previousConnectors.TryGetValue(id, out pointB))
            {
                GameObject obj = new GameObject("Line");

                obj.transform.SetParent(gameObject.transform);

                RopeRenderer cable = obj.AddComponent<RopeRenderer>();
                cable.RopeLength = 1.005f * Vector3.Distance(pointA, pointB);
                cable.PointA = pointA;
                cable.PointB = pointB;
                cable.RopeMaterial = this.CableMaterial;
                cable.RopeThickness = this.CableThickness;
                cable.LineSegmentCount = this.LineSegmentCount;

                cable.Generate();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color defaultColor = Gizmos.color;

        Mesh mesh = GetMeshFromObject(this.UtilityPoleInstance);

        if (mesh != null) Gizmos.DrawMesh(mesh);
    }

    private Mesh GetMeshFromObject(GameObject obj)
    {
        if (obj == null) return null;

        LODGroup lodGroup = obj.GetComponent<LODGroup>();

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            foreach (MeshFilter childMeshFilter in obj.GetComponentsInChildren<MeshFilter>())
            {
                if (childMeshFilter != null && childMeshFilter.sharedMesh != null)
                {
                    meshFilter = childMeshFilter;
                    break;
                }
            }
        }

        Mesh mesh = meshFilter.sharedMesh;

        if (mesh != null) return mesh;

        return null;
    }

    private static Vector3 Transform(Vector3 vector, Vector3 offset, Quaternion rotation)
    {
        return offset + rotation * vector;
    }

    [Serializable]
    public class UtilityConnector
    {
        public Vector3 Position;
        public ConnectorType Type;
        public int Id;
    }

    public enum ConnectorType
    {
        In,
        Out,
    }

    [Serializable]
    public class UtilityLine
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public int Previous = -1;
    }
}
