using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using YamlDotNet.RepresentationModel;
public static class YAML
{

    static Dictionary<string, YamlMappingNode> files = new Dictionary<string, YamlMappingNode>();
    public static void ClearBuffer(string file = "")
    {
        if (file == "")
        {
            files.Clear();
        }
        else
        {
            if (files.ContainsKey(file))
            {
                files.Remove(file);
            }
        }
    }
    public static string GetYamlFilePath(string filename)
    {
        return "YAML/" + filename;
    }
    public static YamlMappingNode Read(string filename)
    {
        if (!files.ContainsKey(filename))
        {
            TextAsset asset = Resources.Load<TextAsset>(GetYamlFilePath(filename));
            if (asset == null)
            {
                throw new YamlFileNotFoundException("filename " + filename + " not found");

            }
            string ymlstring = asset.text;
            System.IO.StringReader r = new System.IO.StringReader(ymlstring);

            YamlStream yaml = new YamlStream();
            yaml.Load(r);

            files.Add(filename, (YamlMappingNode)yaml.Documents[0].RootNode);
        }

        return files[filename];
    }
    public class YamlFileNotFoundException : System.Exception { public YamlFileNotFoundException(string message) : base(message) { } }
}

public class YamlFile
{

    static Dictionary<string, YamlFile> named_setup = new Dictionary<string, YamlFile>();
    public static YamlFile GetSetup(string name)
    {
        if (!named_setup.ContainsKey(name))
        {
            named_setup.Add(name, new YamlFile(name));
        }
        return named_setup[name];
    }
    public static bool SetupExists(string filename)
    {
        return Resources.Load<TextAsset>(YAML.GetYamlFilePath(filename)) != null;
    }


    string file;
    public YamlFile(string filename)
    {
        file = filename;
    }
    public static void ClearBuffer(string setup = "")
    {
        YAML.ClearBuffer(setup);
        if (setup == "")
        {
            named_setup.Clear();
        }
        else
        {
            if (named_setup.ContainsKey(setup))
            {
                named_setup.Remove(setup);
            }
        }
    }

    Dictionary<string, YamlNode> buffer = new Dictionary<string, YamlNode>();

    public T GetChainedNode<T>(string s) where T : YamlNode
    {
        s = s.Replace(":", ";");
        if (!buffer.ContainsKey(s))
        {
            string[] steps = s.Split(new char[] { ';' });
            YamlMappingNode current = YAML.Read(file);

            if (s == "")
            {
                return current as T;
            }

            int i = 0;
            try
            {
                for (i = 0; i < steps.Length - 1; i++)
                {
                    current = current.GetNode<YamlMappingNode>(steps[i]);
                }
                buffer.Add(s, current.GetNode<T>(steps[steps.Length - 1]));
            }
            catch (KeyNotFoundException e)
            {
                Debug.Log("failed at " + i + " : " + steps[i] + " of : " + s);
                throw e;
            }
        }
        return buffer[s] as T;
    }
    string GetChainedNodeValue(string s)
    {
        return GetChainedNode<YamlNode>(s).ToString();
    }

    public float GetFloat(string name) { return float.Parse(GetChainedNodeValue(name), GM.ci); }
    public int GetInt(string name) { return int.Parse(GetChainedNodeValue(name)); }
    public string Get(string name) { return GetChainedNodeValue(name); }

    public float[] AGetFloat(string name) { return GetChainedNodeValue(name).ToFloatArray(); }
    public int[] AGetInt(string name) { return GetChainedNodeValue(name).ToIntArray(); }
    public string[] AGet(string name) { return GetChainedNodeValue(name).ToArray(); }


    public static YamlMappingNode GetFile(string filename)
    {
        return YAML.Read(filename);
    }

}

