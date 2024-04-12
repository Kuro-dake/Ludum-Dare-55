using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntRange : Pair<int, int>
{
    public bool constant => min == max;
    public int min { get { return first; } set { first = value; } }
    public int max { get { return second; } set { second = value; } }
    public IntRange(int a, int b) : base(a, b) { }
    public IntRange(int[] range) : base(range[0], range[1]) { }
   
    public int random { get { return min == max ? min : Random.Range(min, max); } }
    public static implicit operator int(IntRange d)
    {
        return d.random;
    }

    public override string ToString()
    {
        return string.Format("[IntRange: min={0}, max={1}", min, max);
    }

    public float average => ((float)min + (float)max - 1f) * .5f;
    public bool IsNumberInRangeInclusive(int number)
    {
        return number >= min && number <= max;
    }
}
