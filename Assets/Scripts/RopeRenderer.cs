using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class RopeRenderer : MonoBehaviour
{
    public int LineSegmentCount = 12;
    public float RopeLength = 1f;
    public float RopeThickness = 0.1f;
    public Vector3 PointA;
    public Vector3 PointB;
    public Material RopeMaterial;

    private LineRenderer LineRenderer;

    private void OnValidate()
    {
        Generate();
    }

    public void Generate()
    {
        if (this.LineSegmentCount < 0) this.LineSegmentCount = 12;

        float distance = Vector3.Distance(this.PointA, this.PointB);

        if (this.RopeLength < distance) this.RopeLength = distance;

        if (this.LineRenderer == null) this.LineRenderer = GetComponent<LineRenderer>();
        if (this.LineRenderer == null) this.LineRenderer = gameObject.AddComponent<LineRenderer>();

        IEnumerable<Vector3> points = CalculateCatenary(this.PointA, this.PointB, this.LineSegmentCount, this.RopeLength);

        this.LineRenderer.positionCount = this.LineSegmentCount + 1;
        this.LineRenderer.SetPositions(points.ToArray());
        this.LineRenderer.startWidth = this.RopeThickness;
        this.LineRenderer.endWidth = this.RopeThickness;
        this.LineRenderer.material = this.RopeMaterial;
    }

    private static IEnumerable<Vector3> CalculateCatenary(Vector3 p1, Vector3 p2, int segmentCount, float length)
    {
        Vector3 gravity = Physics.gravity;

        Vector3 direction = p2 - p1;

        Vector3 verticalDirection = -gravity.normalized;
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, verticalDirection);

        float vertical = Vector3.Project(direction, verticalDirection).magnitude;
        float horizontal = horizontalDirection.magnitude;

        verticalDirection.Normalize();
        horizontalDirection.Normalize();

        float t = Mathf.Sqrt(length * length - vertical * vertical);

        Func<float, float> sinh = x => (Mathf.Exp(x) - Mathf.Exp(-x)) * 0.5f;
        Func<float, float> cosh = x => (Mathf.Exp(-x) + Mathf.Exp(x)) * 0.5f;

        Func<float, float> f = x => 2 * x * sinh(horizontal / (2 * x)) - t;
        Func<float, float> fPrime = x => 2 * sinh(horizontal / (2 * x)) - (horizontal * cosh(horizontal / (2 * x))) / x;

        float x0 = Bisect(10, 1e-5f, length, f);

        float a = Newton(10, x0, f, fPrime); //Newton(100, 1, f, fPrime);
        
        if (float.IsNaN(a)) return new List<Vector3>();

        a = Mathf.Abs(a);

        //Debug.LogFormat("x0: {0}", x0);
        //Debug.LogFormat("a: {0}", a);

        float x1 = 0.5f * (a * Mathf.Log((length + vertical) / (length - vertical)) - horizontal);
        float x2 = 0.5f * (a * Mathf.Log((length + vertical) / (length - vertical)) + horizontal);

        List<Vector3> points = new List<Vector3>(segmentCount + 1);

        float y1 = a * cosh(x1 / a);

        for (int i = 0; i <= segmentCount; i++)
        {
            float iPercent = (float)(i) / segmentCount;

            float x = Mathf.Lerp(x1, x2, iPercent);

            float y = a * cosh(x / a);

            Vector3 point = Vector3.Lerp(p1, p2, iPercent) + (verticalDirection * (y - y1));
            points.Add(point);
        }

        return points;
    }

    private static float Bisect(int iterations, float a, float b, Func<float, float> f)
    {
        if (iterations < 0) throw new ArgumentOutOfRangeException("iterations");
        if (f == null) throw new ArgumentNullException("f");

        const float epsilon = 1e-5f;

        float c = float.NaN;

        for (int i = 0; i < iterations; i++)
        {
            c = (a + b) * 0.5f;

            float x = f(c);

            if (Mathf.Abs(a - c) < epsilon || Mathf.Abs(c) < epsilon) break;

            if (Mathf.Sign(x) == Mathf.Sign(f(a)))
            {
                a = c;
            }
            else
            {
                b = c;
            }
        }

        return c;
    }

    private static float Newton(int iterations, float x0, Func<float, float> f, Func<float, float> fPrime)
    {
        if (iterations < 0) throw new ArgumentOutOfRangeException("iterations");
        if (f == null) throw new ArgumentNullException("f");
        if (fPrime == null) throw new ArgumentNullException("fPrime");

        const float epsilon = 1e-5f;

        float x = x0;

        for (int i = 0; i < iterations; i++)
        {
            x = x0 - (f(x0) / fPrime(x0));

            if (Mathf.Abs(x - x0) < epsilon) break;

            x0 = x;
        }

        return x;
    }
}
