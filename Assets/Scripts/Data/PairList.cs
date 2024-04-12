using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class PairList<T1, T2> : List<Pair<T1, T2>>
{
    public T2 this[T1 t]
    {
        get
        {
            try
            {
                return Find(p => p.first.Equals(t)).second;
            }
            catch
            {
                throw new System.Exception("Key " + t + " was not found!");
            }
        }

    }
    public T2 GetByIndex(int i) => this[i].second;
    public void Add(T1 a, T2 b)
    {
        Add(Pair.New(a, b));
    }
    public bool HasKey(T1 key) => Find(p => p.first.Equals(key)) != null;
    public void Remove(T1 key)
    {
        if (HasKey(key))
        {
            Remove(Find(p => p.first.Equals(key)));
        }
    }
}
