using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.RepresentationModel;
public abstract class Entity
{
    public YamlMappingNode node { get; protected set; }
    public YamlMappingNode parent_node { get; protected set; }
    public EntityList<T> GetEntityList<T>(string what) where T : Entity, new()
    {
        return new EntityList<T>(node.GetNode<YamlSequenceNode>(what));
    }
    public static T Create<T>(YamlMappingNode n, YamlMappingNode parent = null, string collection_name = "") where T : Entity, new()
    {
        T ret = new T();
        ret.node = n;
        ret.parent_node = parent;
        ret.collection_name = collection_name;
        return ret;
    }
    public string collection_name { get; protected set; }
    public Dictionary<T, T1> ConvertDictionaryKeyToEnum<T, T1>(Dictionary<string, T1> d) where T : System.Enum
    {
        Dictionary<T, T1> ret = new Dictionary<T, T1>();
        foreach (KeyValuePair<string, T1> kv in d)
        {
            ret.Add((T)System.Enum.Parse(typeof(T), kv.Key), kv.Value);
        }
        return ret;
    }

    public Dictionary<string, YamlNode> GetNodeDictionary(YamlMappingNode map)
    {
        Dictionary<string, YamlNode> ret = new Dictionary<string, YamlNode>();
        foreach (KeyValuePair<YamlNode, YamlNode> kv in map)
        {
            ret.Add(kv.Key.ToString(), kv.Value);
        }
        return ret;
    }

    public Dictionary<string, string> GetStringDictionary(YamlMappingNode map)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        foreach (KeyValuePair<YamlNode, YamlNode> kv in map)
        {
            ret.Add(kv.Key.ToString(), kv.Value.ToString());
        }
        return ret;
    }

    public Dictionary<string, int> GetIntDictionary(YamlMappingNode map)
    {
        Dictionary<string, int> ret = new Dictionary<string, int>();
        foreach (KeyValuePair<YamlNode, YamlNode> kv in map)
        {
            ret.Add(kv.Key.ToString(), int.Parse(kv.Value.ToString()));
        }
        return ret;
    }

    public Dictionary<string, int[]> GetIntArrayDictionary(YamlMappingNode map)
    {
        Dictionary<string, int[]> ret = new Dictionary<string, int[]>();
        foreach (KeyValuePair<YamlNode, YamlNode> kv in map)
        {
            ret.Add(kv.Key.ToString(), kv.Value.ToIntArray());
        }
        return ret;
    }

    public Dictionary<string, float> GetFloatDictionary(YamlMappingNode map)
    {
        Dictionary<string, float> ret = new Dictionary<string, float>();
        foreach (KeyValuePair<YamlNode, YamlNode> kv in map)
        {
            ret.Add(kv.Key.ToString(), float.Parse(kv.Value.ToString(), GM.ci));
        }
        return ret;
    }
    Dictionary<string, string> cache = new Dictionary<string, string>();
    public string Get(string what)
    {
        if (!cache.ContainsKey(what))
        {
            cache.Add(what, node.TryGet(what));
        }
        return cache[what];
    }
    Dictionary<string, Entity> entity_cache = new Dictionary<string, Entity>();
    /// <summary>
    /// Method for getting and caching subentities
    /// </summary>
    public T Get<T>(string what) where T : Entity, new()
    {
        if (!entity_cache.ContainsKey(what))
        {
            YamlMappingNode n = null;
            try { n = node.GetNode<YamlMappingNode>(what); }
            catch { }
            T to_add = n != null ? Create<T>(n) : null;
            entity_cache.Add(what, to_add);
        }
        return (T)entity_cache[what];
    }

    public List<string> GetStringArray(string what)
    {
        List<string> ret = new List<string>();
        YamlSequenceNode srds = node.TryGetNode<YamlSequenceNode>(what);
        if (srds != null)
        {
            foreach (YamlNode val in srds)
            {
                ret.Add(val.ToString());
            }
        }
        return ret;
    }
    /// <summary>
    /// Check the data integrity, mainly in relation to other entities
    /// </summary>
    /// <returns>False if integrity check has failed</returns>
    public virtual bool CheckIntegrity() { return true; }

    static int current_unid = 1;
    protected int _unid = current_unid++;
    public int unid { get => _unid; protected set => _unid = value; }
    protected Entity() { }

    public override int GetHashCode()
    {
        return unid;
    }

}

