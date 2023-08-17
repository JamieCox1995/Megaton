using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ConvexDecomposition;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MeshUtils
{
    public static IEnumerable<Mesh> GetConvexDecomposition(Mesh mesh, ConvexDecompositionParameters parameters = null)
    {
        if (mesh.vertexCount < 4)
        {
            if (parameters != null && parameters.Verbose) Debug.LogWarning("Cannot decompose degenerate mesh.");
            yield break;
        }

        using (VHACD vhacd = VHACDFactory.CreateVHACD_ASYNC())
        {
            List<Vector3> points = new List<Vector3>();
            mesh.GetVertices(points);
            uint pointsCount = (uint) points.Count;
            float[] pointsArray = GetFloatArray(points);

            List<int> indices = GetAllIndices(mesh);
            uint triCount = (uint) (indices.Count / 3);
            int[] indexArray = indices.ToArray();

            Parameters internalParams = ConvertParameters(parameters);

            using (internalParams.m_logger = parameters != null && parameters.Verbose ? new UnityLogger() : null)
            using (UnityCallback callback = parameters != null && parameters.Verbose ? new UnityCallback() : null)
            {
                internalParams.m_callback = callback;

                vhacd.Compute(pointsArray, 3u, pointsCount, indexArray, 3u, triCount, internalParams);

                while (!vhacd.IsReady())
                {
                    if (callback != null && callback.IsCancelled)
                    {
                        vhacd.Cancel();
                        yield break;
                    }
                }
            }

            uint hullCount = vhacd.GetNConvexHulls();
            for (uint i = 0; i < hullCount; i++)
            {
                ConvexHull hull = new ConvexHull();
                vhacd.GetConvexHull(i, hull);

                Mesh newMesh = BuildFromHull(hull);

                yield return newMesh;
            }
        }
    }

    private static float[] GetFloatArray(IEnumerable<Vector3> points)
    {
        return points.SelectMany(v => new[] { v.x, v.y, v.z }).ToArray();
    }

    private static List<int> GetAllIndices(Mesh mesh)
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            List<int> submeshIndices = new List<int>();

            mesh.GetIndices(submeshIndices, i);

            indices.AddRange(submeshIndices);
        }

        return indices;
    }

    private static Mesh BuildFromHull(ConvexHull hull)
    {
        double[] pointsArray = hull.m_points;
        int[] triArray = hull.m_triangles;

        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < pointsArray.Length / 3; i++)
        {
            float x = (float)pointsArray[3 * i];
            float y = (float)pointsArray[3 * i + 1];
            float z = (float)pointsArray[3 * i + 2];

            vertices.Add(new Vector3(x, y, z));
        }

        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);
        mesh.SetIndices(triArray, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    private static Parameters ConvertParameters(ConvexDecompositionParameters parameters)
    {
        Parameters internalParams = new Parameters();
        if (parameters != null)
        {
            internalParams.m_concavity = parameters.Concavity;
            internalParams.m_alpha = parameters.Alpha;
            internalParams.m_beta = parameters.Beta;
            internalParams.m_minVolumePerCH = parameters.MinHullVolume;
            internalParams.m_resolution = (uint)parameters.Resolution;
            internalParams.m_maxNumVerticesPerCH = (uint)parameters.MaxVertices;
            internalParams.m_depth = parameters.Depth;
            internalParams.m_planeDownsampling = parameters.PlaneDownsampling;
            internalParams.m_convexhullDownsampling = parameters.HullDownsampling;
            internalParams.m_pca = parameters.NormalizeMesh ? 1 : 0;
            internalParams.m_mode = parameters.UseTetrahedra ? 1 : 0;
            internalParams.m_convexhullApproximation = parameters.UseApproximation ? 1 : 0;
            internalParams.m_oclAcceleration = parameters.UseOpenCLAcceleration ? 1 : 0;
            internalParams.m_maxConvexHulls = (uint)parameters.MaxHullCount;
        }

        return internalParams;
    }

    class UnityLogger : UserLogger
    {
        public override void Log(string msg)
        {
            Debug.Log(msg);
        }
    }

    class UnityCallback : UserCallback
    {
        public bool IsCancelled;

        public override void Update(double overallProgress, double stageProgress, double operationProgress, string stage, string operation)
        {
#if UNITY_EDITOR
            IsCancelled = EditorUtility.DisplayCancelableProgressBar("Mesh Decomposition", string.Format("{0}: {1} ({2:F0}%)", stage, operation, operationProgress), (float)overallProgress);
#endif
        }
    }
}

[Serializable]
public class ConvexDecompositionParameters
{
    [Tooltip("The maximum degree of concavity that an approximately convex hull can have."), Range(0f, 1f)]
    public double Concavity;
    [Tooltip("Controls the bias toward clipping along symmetry planes."), Range(0f, 1f)]
    public double Alpha;
    [Tooltip("Controls the bias toward clipping along revolution axes."), Range(0f, 1f)]
    public double Beta;
    [Tooltip("The minimum volume for each convex hull."), Range(0f, 0.01f)]
    public double MinHullVolume;
    [Tooltip("The maximum number of voxels generated during the voxelisation stage."), Range(10000, 64000000)]
    public int Resolution;
    [Tooltip("The maximum number of vertices for each convex hull."), Range(4, 1024)]
    public int MaxVertices;
    [Tooltip("Maximum number of clipping stages. During each split stage, each part with a concavity higher than the specified threshold is clipped according to the best clipping plane."), Range(1, 32)]
    public int Depth;
    [Tooltip("Controls the granularity of the search for the best clipping plane."), Range(1, 16)]
    public int PlaneDownsampling;
    [Tooltip("Controls the precision of the convex hull generation during the clipping plane selection stage."), Range(1, 16)]
    public int HullDownsampling;
    [Tooltip("Normalizes the mesh by aligning it along its principal axes.")]
    public bool NormalizeMesh;
    [Tooltip("Use the tetrahedron-based method rather than the voxel-based method.")]
    public bool UseTetrahedra;
    [Tooltip("Enables approximation of the generated convex hulls.")]
    public bool UseApproximation;
    [Tooltip("Use OpenCL acceleration for computation.")]
    public bool UseOpenCLAcceleration;
    [Tooltip("The maximum number of hulls to divide the mesh into.")]
    public int MaxHullCount;
    [Tooltip("Output debugging information to the console.")]
    public bool Verbose;

    //public static readonly VHACDParameters Default = new VHACDParameters
    //{
    //    Concavity = 0.001,
    //    Alpha = 0.05,
    //    Beta = 0.05,
    //    MinHullVolume = 0.0001,
    //    Resolution = 100000,
    //    MaxVertices = 64,
    //    Depth = 20,
    //    PlaneDownsampling = 4,
    //    HullDownsampling = 4,
    //    UsePrincipalComponentAxes = false,
    //    UseTetrahedra = false,
    //    UseApproximation = true,
    //    UseOpenCLAcceleration = false,
    //    MaxHullCount = 1024,
    //};

    public static ConvexDecompositionParameters Default
    {
        get
        {
            return new ConvexDecompositionParameters
            {
                Concavity = 0.0025,
                Alpha = 0.05,
                Beta = 0.05,
                MinHullVolume = 0.0001,
                Resolution = 100000,
                MaxVertices = 64,
                Depth = 20,
                PlaneDownsampling = 4,
                HullDownsampling = 4,
                NormalizeMesh = false,
                UseTetrahedra = false,
                UseApproximation = true,
                UseOpenCLAcceleration = false,
                MaxHullCount = 1024,
                Verbose = false,
            };
        }
    }
}