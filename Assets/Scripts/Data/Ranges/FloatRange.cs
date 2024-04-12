using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class FloatRange : Pair<float, float>
{
    public float min { get { return first; } set { first = value; } }
    public float max { get { return second; } set { second = value; } }
    
    public FloatRange(float a, float b) : base(a, b) { }
    public FloatRange() : base(0f, 0f) { }
    public FloatRange(float[] range) : base(range[0], range[1]) { }
    public FloatRange(float a) : base(a, a) { }
    public float random { get { return min == max ? min : Random.Range(min, max); } }
    public static implicit operator float(FloatRange d)
    {
        return d.random;
    }

    public override string ToString()
    {
        return string.Format("[FloatRange: min={0}, max={1}]", min, max);
    }

}
