using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Octree<T> where T : Component
{
    private Node _root;
    private Dictionary<T, Transform> _transforms;
    private Dictionary<Transform, T> _components;
    private Dictionary<Transform, Bounds> _lastUpdateBounds;
    private Dictionary<Transform, Node> _nodes;

    public int MaxDepth { get; private set; }
    public float MinSize { get; private set; }
    public Bounds Bounds { get; private set; }

    public Octree(int maxDepth, float minSize)
    {
        this.MaxDepth = maxDepth;
        this.MinSize = minSize;

        float size = minSize * Mathf.Pow(2f, maxDepth);

        this.Bounds = new Bounds(Vector3.zero, Vector3.one * size);

        _root = new Node(this);
        _transforms = new Dictionary<T, Transform>();
        _components = new Dictionary<Transform, T>();
        _lastUpdateBounds = new Dictionary<Transform, Bounds>();
        _nodes = new Dictionary<Transform, Node>();
    }

    public Octree(int maxDepth, float minSize, IEnumerable<T> objects) : this(maxDepth, minSize)
    {
        Insert(objects);
    }

    public bool Insert(T obj)
    {
        if (obj.IsDestroyedOrMissing() || obj.transform.IsDestroyedOrMissing()) return false;

        Bounds transformBounds = GetBounds(obj.transform);

        bool result = _root.Insert(obj.transform, transformBounds);

        if (result)
        {
            _transforms.Add(obj, obj.transform);
            _components.Add(obj.transform, obj);
            _lastUpdateBounds.Add(obj.transform, GetBounds(obj.transform));
        }

        return result;
    }

    public void Insert(IEnumerable<T> objects)
    {
        foreach (T obj in objects)
        {
            Insert(obj);
        }
    }

    public void Update()
    {
        foreach (KeyValuePair<Transform, Bounds> pair in _lastUpdateBounds.ToList())
        {
            Transform transform = pair.Key;
            Bounds lastUpdateBounds = pair.Value;

            if (transform.IsDestroyedOrMissing())
            {
                Remove(transform);
                continue;
            }

            Bounds transformBounds = GetBounds(transform);
            // object has not changed position, rotation, or scale since the last update
            if (transformBounds == lastUpdateBounds) continue;

            if (_root.Update(transform, transformBounds))
            {
                _lastUpdateBounds[transform] = transformBounds;
            }
        }
    }

    public bool Remove(T obj)
    {
        Transform transform;
        
        if (obj.IsDestroyedOrMissing())
        {
            transform = _transforms[obj];
        }
        else
        {
            transform = obj.transform;
        }

        bool result = _root.Remove(transform);

        if (result)
        {
            _transforms.Remove(obj);
            _components.Remove(transform);
            _lastUpdateBounds.Remove(transform);
            _nodes.Remove(transform);
        }

        return result;
    }

    public void Remove(IEnumerable<T> objects)
    {
        foreach (T obj in objects)
        {
            Remove(obj);
        }
    }

    private void Remove(Transform transform)
    {
        T obj = _components[transform];

        bool result = _root.Remove(transform);

        if (result)
        {
            _transforms.Remove(obj);
            _components.Remove(transform);
            _lastUpdateBounds.Remove(transform);
            _nodes.Remove(transform);
        }
    }

    public IEnumerable<T> IntersectSphere(Vector3 center, float radius)
    {
        return _root.IntersectSphere(center, radius).Select(t => _components[t]).ToList();
    }

    private static bool TryGetBounds(Transform transform, out Bounds bounds)
    {
        Renderer renderer = transform.GetComponent<Renderer>();

        if (renderer == null)
        {
            bounds = new Bounds();

            if (transform.childCount > 0)
            {
                bool anyBoundsFound = false;

                foreach (Transform child in transform)
                {
                    Bounds childBounds;

                    if (TryGetBounds(child, out childBounds))
                    {
                        if (!anyBoundsFound) anyBoundsFound = true;

                        bounds.Encapsulate(childBounds);
                    }
                }

                return anyBoundsFound;
            }
            else
            {
                return false;
            }
        }
        else
        {
            bounds = renderer.bounds;
            return true;
        }
    }

    private static Bounds GetBounds(Transform transform)
    {
        Bounds bounds;
        if (!TryGetBounds(transform, out bounds)) bounds = new Bounds(transform.position, Vector3.zero);

        return bounds;
    }

    class Node
    {
        private Octree<T> _octree;
        private Node _parent;
        private Node[] _children;
        private List<Transform> _transforms;
        private Bounds _bounds;
        private int _depth;

        public bool IsLeaf { get { return _children == null; } }

        public bool IsActive { get { return !this.IsLeaf && _transforms != null && _transforms.Count > 0; } }

        public Node(Octree<T> octree) : this(octree, null, 0) { }

        public Node(Octree<T> octree, Node parent, int index)
        {
            _octree = octree;
            _transforms = new List<Transform>();
            _parent = parent;

            if (parent != null)
            {
                _depth = parent._depth + 1;

                _bounds = SplitBounds(parent._bounds, index);
            }
            else
            {
                _depth = 0;
                _bounds = octree.Bounds;
            }
        }

        public IEnumerable<Transform> IntersectSphere(Vector3 center, float radius)
        {
            float sqrRadius = radius * radius;

            List<Transform> result = new List<Transform>();

            if (_bounds.SqrDistance(center) > sqrRadius) return result;

            if (_transforms.Count > 0)
            {
                if (MaxSqrDistance(_bounds, center) <= sqrRadius)
                {
                    result.AddRange(_transforms);
                }
                else
                {
                    foreach (Transform transform in _transforms)
                    {
                        Bounds transformBounds = _octree._lastUpdateBounds[transform];

                        if (transformBounds.SqrDistance(center) <= sqrRadius)
                        {
                            result.Add(transform);
                        }
                    }
                }
            }

            if (_children != null)
            {
                foreach (Node child in _children)
                {
                    result.AddRange(child.IntersectSphere(center, radius));
                }
            }

            return result;
        }

        public bool Insert(Transform transform, Bounds transformBounds)
        {
            bool isAtDepthLimit =  _depth == _octree.MaxDepth - 1;

            if (EnclosesBounds(transformBounds))
            {
                if (isAtDepthLimit)
                {
                    _transforms.Add(transform);
                    _octree._nodes.Add(transform, this);
                }
                else
                {
                    bool isInsertedIntoChildNode = false;

                    if (this.IsLeaf)
                    {
                        _children = new Node[8];

                        for (int i = 0; i < 8; i++)
                        {
                            _children[i] = new Node(_octree, this, i);
                        }
                    }

                    foreach (Node childNode in _children)
                    {
                        if (childNode.Insert(transform, transformBounds))
                        {
                            isInsertedIntoChildNode = true;
                            break;
                        }
                    }

                    if (!isInsertedIntoChildNode)
                    {
                        _transforms.Add(transform);
                        _octree._nodes.Add(transform, this);
                    }
                }

                return true;
            }

            return false;
        }

        public bool Update(Transform transform, Bounds transformBounds)
        {
            Node containingNode = _octree._nodes[transform];

            if (containingNode == null) return false;

            bool result = containingNode.UpdateInternal(transform, transformBounds, true);

            if (result)
            {
                containingNode._transforms.Remove(transform);
            }

            return result;
        }

        private bool UpdateInternal(Transform transform, Bounds transformBounds, bool canVisitParent)
        {
            if (EnclosesBounds(transformBounds))
            {
                bool isAtDepthLimit = _depth == _octree.MaxDepth - 1;

                if (_children == null && !isAtDepthLimit)
                {
                    _children = new Node[8];
                    for (int i = 0; i < 8; i++)
                    {
                        _children[i] = new Node(_octree, this, i);
                    }
                }
                
                if (_children != null)
                {
                    foreach (Node child in _children)
                    {
                        if (child.UpdateInternal(transform, transformBounds, false)) return true;
                    }
                }

                _transforms.Add(transform);
                _octree._nodes[transform] = this;
                return true;
            }
            else if (_parent != null && canVisitParent)
            {
                return _parent.UpdateInternal(transform, transformBounds, true);
            }

            return false;
        }



        public bool Remove(Transform transform)
        {
            Node containingNode = _octree._nodes[transform];
            
            if (containingNode != null)
            {
                return containingNode._transforms.Remove(transform);
            }

            return false;
        }

        private float MaxSqrDistance(Bounds bounds, Vector3 point)
        {
            Vector3 direction = point - bounds.center;

            return direction.sqrMagnitude + bounds.extents.sqrMagnitude;
        }

        private bool EnclosesTransform(Transform transform)
        {
            Bounds transformBounds;

            if (TryGetBounds(transform, out transformBounds))
            {
                return EnclosesBounds(transformBounds);
            }
            else
            {
                return _bounds.Contains(transform.position);
            }
        }

        private bool EnclosesBounds(Bounds bounds)
        {
            return _bounds.Contains(bounds.min) && _bounds.Contains(bounds.max);
        }

        private static Bounds SplitBounds(Bounds bounds, int index)
        {
            bool isPositiveX = index % 2 == 1;
            bool isPositiveY = (index >> 1) % 2 == 1;
            bool isPositiveZ = (index >> 2) % 2 == 1;

            Vector3 center = bounds.center;

            Vector3 offsetMultiplier = new Vector3(isPositiveX ? 0.5f : -0.5f,
                                                   isPositiveY ? 0.5f : -0.5f,
                                                   isPositiveZ ? 0.5f : -0.5f);

            Vector3 offset = Vector3.Scale(offsetMultiplier, bounds.extents);

            Bounds result = new Bounds(center + offset, bounds.extents);

            return result;
        }
    }
}
