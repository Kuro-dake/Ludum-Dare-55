using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.RepresentationModel;
public static class NodeExtensions
{
    public static string Get(this YamlMappingNode n, string what)
    {
        return n.GetNode(what).ToString();
    }
    public static string TryGet(this YamlMappingNode n, string what, string _default = "")
    {
        try
        {
            return n.Get(what);
        }
        catch (KeyNotFoundException)
        {
            return _default;
        }

    }
    public static float GetFloat(this YamlMappingNode n, string what)
    {
        return float.Parse(n.GetNode(what).ToString(),GM.ci);
    }
    public static int GetInt(this YamlMappingNode n, string what)
    {
        return int.Parse(n.GetNode(what).ToString());
    }
    public static int TryGetInt(this YamlMappingNode n, string what, int _default = 0)
    {
        try
        {
            return n.GetInt(what);
        }
        catch (KeyNotFoundException)
        {
            return _default;
        }
    }
    public static float TryGetFloat(this YamlMappingNode n, string what, float _default = 0)
    {
        try
        {
            return n.GetFloat(what);
        }
        catch (KeyNotFoundException)
        {
            return _default;
        }
    }
    public static bool TryGetBool(this YamlMappingNode n, string what, bool _default = false)
    {
        try
        {
            return n.GetBool(what);
        }
        catch (KeyNotFoundException)
        {
            return _default;
        }
    }
    public static bool GetBool(this YamlMappingNode n, string what)
    {
        return n.Get(what) == "true" ? true : (n.Get(what) == "false" ? false : n.GetInt(what) != 0);
    }
    public static string[] TryGetArray(this YamlMappingNode n, string what, string[] _default = null)
    {
        try
        {
            return n.GetArray(what);
        }
        catch
        {
            return _default == null ? new string[] { } : _default;
        }
    }

    public static float[] TryGetFloatArray(this YamlMappingNode n, string what, float[] _default = null)
    {
        try
        {
            return n.GetFloatArray(what);
        }
        catch
        {
            return _default == null ? new float[] { } : _default;
        }
    }

    public static string[] GetArray(this YamlMappingNode n, string what)
    {
        return n.Get(what).Split(new char[] { ',' });
    }
    public static float[] GetFloatArray(this YamlMappingNode n, string what)
    {
        return new List<string>(n.Get(what).Split(new char[] { ',' })).ConvertAll<float>(delegate (string input)
        {
            return float.Parse(input,GM.ci);
        }).ToArray();
    }
    public static Color TryGetColor(this YamlMappingNode n, string what, Color _default = default)
    {
        return n.HasProperty(what) ? n.GetColor(what) : _default;
    }
    public static Color GetColor(this YamlMappingNode n, string what)
    {
        Color ret;
        ColorUtility.TryParseHtmlString(n.Get(what), out ret);
        return ret;
    }
    public static Color[] GetColorArray(this YamlMappingNode n, string what)
    {
        return new List<string>(n.Get(what).Split(new char[] { ',' })).ConvertAll(delegate (string input)
        {
            Color ret;

            ColorUtility.TryParseHtmlString(input, out ret);

            return ret;
        }).ToArray();
    }
    public static int[] ToIntArray(this YamlNode n)
    {
        return new List<string>(n.ToString().Split(new char[] { ',' })).ConvertAll<int>(delegate (string input)
        {
            return int.Parse(input);
        }).ToArray();
    }
    public static int[] GetIntArray(this YamlMappingNode n, string what)
    {
        return new List<string>(n.Get(what).Split(new char[] { ',' })).ConvertAll<int>(delegate (string input)
        {
            return int.Parse(input);
        }).ToArray();
    }
    public static Vector2Int GetVector2Int(this YamlMappingNode n, string what)
    {
        int[] coords = n.GetIntArray(what);
        return new Vector2Int(coords[0], coords[1]);
    }
    public static Vector2Int TryGetVector2Int(this YamlMappingNode n, string what, int default_x = 0, int default_y = 0)
    {
        return n.HasProperty(what) ? n.GetVector2Int(what) : new Vector2Int(default_x, default_y);
    }
    public static bool HasProperty(this YamlMappingNode n, string what)
    {
        return n.Children.ContainsKey(what);
    }
    public static IntRange TryGetIntRange(this YamlMappingNode n, string what, int default_min = 1, int default_max = 2)
    {
        try
        {
            return n.GetIntRange(what);
        }
        catch
        {
            return new IntRange(default_min, default_max);
        }
    }
    public static IntRange GetIntRange(this YamlMappingNode n, string what)
    {
        int[] range = n.GetIntArray(what);
        return new IntRange(range[0], (range.Length > 1 ? range[1] + 1 : range[0]));
    }
    public static FloatRange TryGetFloatRange(this YamlMappingNode n, string what, float default_min = 0f, float default_max = 0f)
    {
        try
        {
            return n.GetFloatRange(what);
        }
        catch
        {
            return new FloatRange(default_min, default_max);
        }
    }
    public static FloatRange GetFloatRange(this YamlMappingNode n, string what)
    {
        return new FloatRange(n.GetFloatArray(what));
    }
    public static ColorRange TryGetColorRange(this YamlMappingNode n, string what, Color default_min = default, Color default_max = default)
    {
        return n.HasProperty(what) ? n.GetColorRange(what) : new ColorRange(default_min == default ? Color.white : default_min, default_max == default ? Color.white : default_max);
    }
    public static ColorRange GetColorRange(this YamlMappingNode n, string what)
    {
        return new ColorRange(n.GetColorArray(what));
    }

    public static YamlNode GetNode(this YamlMappingNode n, string what)
    {
        return n.Children[new YamlScalarNode(what)];
    }
    public static T TryGetNode<T>(this YamlMappingNode n, string what) where T : YamlNode
    {
        try
        {
            return n.GetNode<T>(what);
        }
        catch
        {
            return null;
        }
    }
    public static T GetNode<T>(this YamlMappingNode n, string what) where T : YamlNode
    {
        return (T)n.GetNode(what);
    }
    public static string[] ToArray(this string n)
    {
        return n.Split(new char[] { ',' });
    }
    public static float[] ToFloatArray(this string n)
    {
        return new List<string>(n.ToArray()).ConvertAll<float>(delegate (string input)
        {
            return float.Parse(input, GM.ci);
        }).ToArray();
    }
    public static int[] ToIntArray(this string n)
    {
        return new List<string>(n.ToArray()).ConvertAll<int>(delegate (string input)
        {
            return int.Parse(input);
        }).ToArray();
    }

}
