using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class Common
{
    public static float SinT(float value)
    {
        return (Mathf.Sin(value * Mathf.PI - Mathf.PI * .5f) + 1f) * .5f;
    }
    /// <summary>
    /// Returns one of the two provided numbers on random equal chance
    /// </summary>
    /// <param name="either"></param>
    /// <param name="or"></param>
    /// <returns></returns>
    public static int EitherOr(int either = 1, int or = -1)
    {
        return Random.Range(0, 2) == 1 ? either : or;
    }

    public static Vector3 GetCameraPosition(Camera ortographic, Camera perspective, Vector3 position)
    {
        return perspective.ScreenToWorldPoint(ortographic.WorldToScreenPoint(position));
    }

    public static float GetOrtographicCameraWorldWidthHalf(Camera ortographic)
    {
        return ortographic.orthographicSize * ((float)UnityEngine.Screen.width / (float)UnityEngine.Screen.height);
    }

    public static Sprite GetSprite(string sprite_name, int id)
    {
        return Resources.LoadAll<Sprite>(sprite_name).ToList().Find(s => s.name == string.Format("{0}_{1}", sprite_name, id));
    }
    static public WaitForKeyDownInstruction WaitForKeyDown(KeyCode action)
    {
        return new WaitForKeyDownInstruction(action);
    }
    public class WaitForKeyDownInstruction : CustomYieldInstruction
    {
        KeyCode action;
        public WaitForKeyDownInstruction(KeyCode key) => this.action = key;
        public override bool keepWaiting => !Input.GetKeyDown(action);
    }

    public static Vector3 PlaceOnCircle(Vector3 center, float circlepos, float radius, float angle_modifier = 0f)
    {

        // create random angle between 0 to 360 degrees 

        float ang = (angle_modifier + circlepos) * 360;
        Vector3 pos = new Vector3(center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad),
                                   center.y + radius * Mathf.Cos(ang * Mathf.Deg2Rad),
                                   center.z);
        return pos;

    }
    public static Vector2 GetCanvasPoint(Vector2 world_position, Canvas canvas)
    {
        Camera cam = canvas.worldCamera;
        if (cam == null)
        {
            cam = Camera.main;
        }
        Vector2 ViewportPosition = cam.WorldToViewportPoint(world_position);
        RectTransform CanvasRect = canvas.GetComponent<RectTransform>();
        return new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));
    }
    public const string yaml_manifest_resource_name = "YAML/manifest";
    public static List<string> GetYAMLFolderFilenames(string folder)
    {
        List<string> folder_names = Resources.Load<TextAsset>(yaml_manifest_resource_name).text.Split(new char[] { '\n' }).ToList();
        folder_names.RemoveAll(fn => fn.Length < folder.Length || fn.Substring(0, folder.Length) != folder);
        
        HashSet<TextAsset> text_assets = new HashSet<TextAsset>();

        List<string> ret = new List<string>();

        foreach (string folder_name in folder_names)
        {
            foreach (TextAsset ta in Resources.LoadAll<TextAsset>("YAML/" + folder_name))
            {
                if (text_assets.Contains(ta))
                {
                    continue;
                }
                ret.Add(folder_name + "/" + ta.name);
                text_assets.Add(ta);
            }
        }

        return ret;
    }
    public static Vector3 MultiplyVectorBy(Vector3 multiply, Vector3 by)
    {
        return new Vector3(multiply.x * by.x, multiply.y * by.y, 1f);
    }
    public static Vector2 RandomPoint(this Bounds b)
    {
        return new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
    }

    public static void SetColors(this ParticleSystem ps, Color c)
    {
        /*ParticleSystem.MainModule mm = ps.main;
        ParticleSystem.MinMaxGradient mmg = ps.main.startColor;*/


        ParticleSystem.ColorOverLifetimeModule colm = ps.colorOverLifetime;
        ParticleSystem.MinMaxGradient mmg = colm.color;
        Gradient g = mmg.gradient;
        GradientColorKey[] keys = g.colorKeys;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].color = c;

        }
        mmg.gradient.SetKeys(keys, g.alphaKeys);

        colm.color = mmg;//.gradient.SetKeys(keys, g.alphaKeys);

    }
    public static T GetComponentInParents<T>(this Component c) where T : Component
    {
        T ret = null;
        Transform t = c.transform.parent;
        while(ret == null && t != null)
        {
            ret = t.GetComponent<T>();
            t = t.parent;
        }
        return ret;
    }
    static bool CollisionAction<T>(T component, string message, System.Action action = null) where T : Component
    {
        if (component != null)
        {
            if (message != null)
            {
                component.SendMessage(message);
            }
            action?.Invoke();
            return true;
        }
        return false;
    }
    public static bool CollisionParentComponentAction<T>(Collision2D collision, string message, System.Action action = null) where T : Component
        => CollisionAction(collision.collider.GetComponentInParents<T>(), message, action);
    public static bool CollisionComponentAction<T>(Collision2D collision, string message, System.Action action = null) where T : Component 
        => CollisionAction(collision.collider.GetComponent<T>(), message, action);
        
}


public static class LineFunctions
{
    public static void AddLinePoint(LineRenderer line, Vector3 line_target)
    {
        Vector3[] a_positions = new Vector3[line.positionCount];
        line.GetPositions(a_positions);

        //List<Vector3> positions = new List<Vector3>(a_positions);

        System.Array.Resize(ref a_positions, a_positions.Length + 1);

        a_positions[a_positions.Length - 1] = line_target;

        line.positionCount++;
        line.SetPositions(a_positions);

    }

    public static void RemoveFirstLinePoint(LineRenderer line)
    {
        if (line.positionCount == 0)
        {
            return;
        }
        Vector3[] a_positions = new Vector3[line.positionCount];
        line.GetPositions(a_positions);

        //List<Vector3> positions = new List<Vector3>(a_positions);

        Vector3[] n_positions = new Vector3[a_positions.Length - 1];

        for (int i = 1; i < a_positions.Length; i++)
        {
            n_positions[i - 1] = a_positions[i];
        }

        line.positionCount--;
        line.SetPositions(n_positions);

    }

    public static void SetAlpha(this LineRenderer line, float alpha)
    {
        Color c = line.startColor;
        c.a = alpha;
        line.startColor = c;
        c = line.endColor;
        c.a = alpha;
        line.endColor = c;
    }

}
public static class TransformFunctions
{
    public static void DestroyChildren(this Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            Object.Destroy(t.GetChild(i).gameObject);
        }
        t.DetachChildren();
    }
    public static float Face2D(this Transform t, Transform face, float modifier = 0f, bool set = true) => Face2D(t, face.position, modifier, set);
    public static float Face2D(this Transform t, Vector3 face, float modifier = 0f, bool set = true)
    {
        float angle = Mathf.Atan2(face.y - t.position.y, face.x - t.position.x) * Mathf.Rad2Deg + modifier;
        if (set)
        {
            t.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        return angle;
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T RandomElement<T>(this IList<T> list)=>list[Random.Range(0, list.Count)];
    
}

public static class FloatExtensions
{
    public static int Sign(this int f)
    {
        return f == 0 ? 0 : (f > 0 ? 1 : -1);
    }
    public static float Sign(this float f)
    {
        return f == 0f ? 0f : Mathf.Sign(f);
    }
    public static int SignInt(this float f)
    {
        return Mathf.RoundToInt(f.Sign());
    }
}
public static class ColorExtensions
{
    public static Color Alpha(this Color c, float a)
    {
        c.a = a;
        return c;
    }
}

public static class StringExtension
{
    public static string UCFirst(this string s)
    {
        if(s.Length == 0)
        {
            return s;
        }
        if(s.Length == 1)
        {
            return s.ToUpper();
        }
        return s.Substring(0, 1).ToUpper() + s.Substring(1);
    }
}
public enum side
{
    left,
    right
}