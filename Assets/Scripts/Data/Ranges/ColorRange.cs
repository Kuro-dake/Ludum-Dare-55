using UnityEngine;

public class ColorRange : Pair<Color, Color>
{
    public ColorRange(Color a, Color b) : base(a, b) { }
    public ColorRange(Color[] a) : base(a[0], a[0])
    {

        first = a[0];
        if (a.Length > 1)
        {
            second = a[1];
        }
        else
        {
            second = a[0];
        }
    }
    public Color random => Color.Lerp(first, second, Random.Range(0f, 1f));

}