using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class ShaderGradient : ScriptableObject, IEnumerable<GradientStop>, IList<GradientStop>, IList
{
    [SerializeField]
    GradientStop _start;
    [SerializeField]
    GradientStop _end;
    [SerializeField]
    List<GradientStop> _middle;

    public int Count
    {
        get { return _middle.Count + 2; }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    private bool NeedsSort { get; set; }

    public bool IsFixedSize { get { return false; } }

    public bool IsSynchronized { get { return false; } }

    public object SyncRoot { get { return this; } }

    object IList.this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Count) throw new IndexOutOfRangeException();

            return this[index];
        }
        set
        {
            if (index < 0 || index >= this.Count) throw new IndexOutOfRangeException();

            if (!(value is GradientStop)) return;

            this[index] = (GradientStop)value;
        }
    }

    public GradientStop this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Count) throw new IndexOutOfRangeException();

            if (index == 0) return _start;
            if (index == this.Count - 1) return _end;

            return _middle[index - 1];
        }
        set
        {
            if (index < 0 || index >= this.Count) throw new IndexOutOfRangeException();

            if (index == 0)
            {
                _start = value;
            }
            else if (index == this.Count - 1)
            {
                _end = value;
            }
            else
            {
                _middle[index - 1] = value;
            }

            //Sort();
            this.NeedsSort = true;
        }
    }

    public static ShaderGradient Create()
    {
        ShaderGradient g = CreateInstance<ShaderGradient>();
        g.name = "Gradient";
        g._start = new GradientStop { t = 0, color = Color.black };
        g._end = new GradientStop { t = 1, color = Color.white };
        g._middle = new List<GradientStop>();
        return g;
    }

    public static ShaderGradient Create(Color start, Color end)
    {
        ShaderGradient g = CreateInstance<ShaderGradient>();
        g.name = "Gradient";
        g._start = new GradientStop { t = 0, color = start };
        g._end = new GradientStop { t = 1, color = end };
        g._middle = new List<GradientStop>();
        return g;
    }

    public static ShaderGradient Create(GradientStop start, GradientStop end)
    {
        ShaderGradient g = CreateInstance<ShaderGradient>();
        g.name = "Gradient";
        g._start = start;
        g._end = end;
        g._middle = new List<GradientStop>();
        return g;
    }

    public IEnumerator<GradientStop> GetEnumerator()
    {
        yield return _start;

        foreach (GradientStop stop in _middle)
        {
            yield return stop;
        }

        yield return _end;
    }

    public void Add(GradientStop stop)
    {
        _middle.Add(stop);

        //Sort();
        this.NeedsSort = true;
    }

    public void Add(float t, Color color)
    {
        _middle.Add(new GradientStop { t = t, color = color });

        //Sort();
        this.NeedsSort = true;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException("index");

        if (this.Count <= 2) return;

        if (index == 0)
        {
            _start = _middle.First();
            _middle.RemoveAt(0);
        }
        else if (index == this.Count - 1)
        {
            _end = _middle.Last();
            _middle.RemoveAt(_middle.Count - 1);
        }
        else
        {
            _middle.RemoveAt(index - 1);
        }

        //Sort();
        this.NeedsSort = true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Texture ToTexture(int resolution)
    {
        if (this.NeedsSort) Sort();

        Texture2D texture = new Texture2D(resolution, 1, TextureFormat.RGBAFloat, false, true);
        texture.name = "Gradient_Tex";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 1;

        Color[] colors = new Color[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;
            colors[i] = Evaluate(t);
        }

        texture.SetPixels(colors);
        texture.Apply(false, false);

        return texture;
    }

    public void WriteToTexture(Texture texture)
    {
        Texture2D tex = texture as Texture2D;

        if (tex == null) return;

        if (this.NeedsSort) Sort();

        Color[] colors = new Color[tex.height * tex.width];
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float t = x / (float)tex.width;
                colors[y * tex.width + x] = Evaluate(t);
            }
        }

        tex.SetPixels(colors);
        tex.Apply(false, false);
    }

    public Color Evaluate(float t)
    {
        if (this.NeedsSort) Sort();

        List<GradientStop> stops = GetAllStops();

        if (t <= _start.t) return _start.color;
        if (t >= _end.t) return _end.color;

        GradientStop highestBelow = stops.Where(stop => stop.t <= t).Max();
        GradientStop lowestAbove = stops.Where(stop => stop.t > t).Min();

        float u = Mathf.InverseLerp(highestBelow.t, lowestAbove.t, t);

        return Color.Lerp(highestBelow.color, lowestAbove.color, u);
    }

    private void Sort()
    {
        if (!this.NeedsSort) return;

        List<GradientStop> stops = GetAllStops();

        stops.Sort();

        _start = stops.First();
        _end = stops.Last();

        stops.Remove(_start);
        stops.Remove(_end);

        _middle.Clear();
        _middle.AddRange(stops);

        this.NeedsSort = false;
    }

    private List<GradientStop> GetAllStops()
    {
        List<GradientStop> stops = new List<GradientStop> { _start };
        stops.AddRange(_middle);
        stops.Add(_end);
        return stops;
    }

    public int IndexOf(GradientStop item)
    {
        if (GradientStop.Equals(_start, item)) return 0;
        if (GradientStop.Equals(_end, item)) return this.Count - 1;

        int index = _middle.IndexOf(item);

        return index == -1 ? -1 : index + 1;
    }

    public void Insert(int index, GradientStop item)
    {
        if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException("index");

        if (index == 0)
        {
            _middle.Insert(0, _start);
            _start = item;
        }
        else if (index == this.Count - 1)
        {
            _middle.Add(_end);
            _end = item;
        }
        else
        {
            _middle.Insert(index - 1, item);
        }

        //Sort();
        this.NeedsSort = true;
    }

    public void Clear()
    {
        _start = new GradientStop { t = 0, color = Color.black };
        _end = new GradientStop { t = 1, color = Color.white };
        _middle.Clear();
    }

    public bool Contains(GradientStop item)
    {
        return GradientStop.Equals(_start, item) || GradientStop.Equals(_end, item) || _middle.Contains(item);
    }

    public void CopyTo(GradientStop[] array, int arrayIndex)
    {
        throw new NotSupportedException();
    }

    public bool Remove(GradientStop item)
    {
        int index = IndexOf(item);

        if (index == -1) return false;

        RemoveAt(index);

        return true;
    }

    public int Add(object value)
    {
        if (!(value is GradientStop)) return -1;

        Add((GradientStop)value);

        return IndexOf((GradientStop)value);
    }

    public bool Contains(object value)
    {
        if (!(value is GradientStop)) return false;

        return Contains((GradientStop)value);
    }

    public int IndexOf(object value)
    {
        if (!(value is GradientStop)) return -1;

        return IndexOf((GradientStop)value);
    }

    public void Insert(int index, object value)
    {
        if (!(value is GradientStop)) return;

        Insert(index, (GradientStop)value);
    }

    public void Remove(object value)
    {
        if (!(value is GradientStop)) return;

        Remove((GradientStop)value);
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotSupportedException();
    }
}
