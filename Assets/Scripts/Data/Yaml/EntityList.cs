using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class EntityList<T> : List<T> where T : Entity, new()
{

    public EntityList() : base() { }

    public EntityList(YamlSequenceNode sequence, YamlMappingNode parent = null, string collection_name = "")
    {
        AddEntitiesFromNode(sequence, parent, collection_name);
    }

    public void AddEntitiesFromNode(YamlSequenceNode sequence, YamlMappingNode parent = null, string collection_name = "")
    {
        foreach (YamlMappingNode n in sequence)
        {
            T to_add = Entity.Create<T>(n, parent, collection_name);
            //to_add.CheckIntegrity();
            Add(to_add);
        }
    }

    public static EntityList<T> CreateFromSetup(string setup_name, string node_chain = "", string collection_name = "")
    {
        YamlFile setup = YamlFile.GetSetup(setup_name);

        EntityList<T> ret = null;

        try
        {
            ret =
                new EntityList<T>(setup.GetChainedNode<YamlSequenceNode>(node_chain == "" ? setup_name : node_chain),
                    null, collection_name);
        }
        catch (YamlDotNet.Core.YamlException)
        {
            Debug.LogError($"Error parsing {setup_name} at {node_chain}");
            throw;
        }
        
        return ret;
    }

    
}

public static class EntityList
{
    public static EntityList<T1> CreateList<T1>() where T1 : Entity, new()
    {
        return new EntityList<T1>();

    }
}
