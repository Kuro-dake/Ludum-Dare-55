

[System.Serializable]
public class Pair<T1, T2>
{
    public T1 first;
    public T2 second;
    public Pair(T1 f, T2 s)
    {
        first = f;
        second = s;
    }

    public override string ToString()
    {
        return "Pair<" + typeof(T1).ToString() + "," + typeof(T2).ToString() + "> " + first.ToString() + ", " + second.ToString();
    }
}



public static class Pair
{
    public static Pair<T1, T2> New<T1, T2>(T1 first, T2 second)
    {
        var pair = new Pair<T1, T2>(first, second);
        return pair;
    }

}
