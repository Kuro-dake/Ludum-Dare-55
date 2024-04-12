using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IEnumRunner.Transitions;
using IEnumRunner;
public class MakeTransitions : MonoBehaviour
{

    public List<MakeTransitionData> transitions = new List<MakeTransitionData>();
    public List<MakeTransitionData> filtered_transitions = new List<MakeTransitionData>();
    public List<Pair<string, Pair<GameObject, float>>> group_targets = new List<Pair<string, Pair<GameObject, float>>>();
    public List<string> groups = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        //current_state = "";
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void RenameGroup(string from, string to)
    {
        transitions.FindAll(g => g.group == from).ForEach(g => g.group = to);
    }
    public Sequence GetTriggerSequence(string group)
    {
        GameObject target = group_targets.Find(g => g.first == group)?.second.first;
        Make._Batch b = Make.The(target == null ? gameObject : target);
        List<MakeTransitionData> transitions = GetFilteredTransitions(group);
        float speed = 1f / group_targets.Find(g => g.first == group).second.second;
        bool in_set = false;
        /*if(group == "spawn" && GetComponentInChildren<Waddler>() != null)
        {
            Debug.Log("speed " + speed + " " + gameObject);
            group_targets.ForEach(p => Debug.Log("speed " + p.first + " " + p.second.second));
        }*/
        foreach (MakeTransitionData mtd in transitions)
        {
            if (!in_set && mtd.is_in_setter)
            {
                b.In(mtd.duration * speed);
                in_set = true;
            }
            if (mtd.is_in_resetter)
            {
                in_set = false;
            }

            switch (mtd.type)
            {
                case MakeTransitionData.transition_type.move:
                    if (mtd.by)
                    {
                        b.MoveBy(mtd.vector_value);
                    }
                    else
                    {
                        b.MoveTo(mtd.vector_value);
                    }
                    break;
                case MakeTransitionData.transition_type.scale:
                    if (mtd.by)
                    {
                        b.ScaleBy(mtd.vector_value);
                    }
                    else
                    {
                        b.ScaleTo(mtd.vector_value);
                    }
                    break;
                case MakeTransitionData.transition_type.alpha:
                    if (mtd.by)
                    {
                        b.AlphaBy(mtd.float_value);
                    }
                    else
                    {
                        b.AlphaTo(mtd.float_value);
                    }
                    break;
                case MakeTransitionData.transition_type.then:
                    b = b.then;
                    break;
                case MakeTransitionData.transition_type.wait:
                    b.ThenWait(mtd.duration);
                    break;
                default:
                    throw new System.Exception("Undefined type action for " + mtd.type);
            }
        }
        return b;
    }
    public string current_state { get; protected set; } = "";
    public Sequence Trigger(string group)
    {
        current_state = group;
        return GetTriggerSequence(group).Run();
    }
    
    public List<MakeTransitionData> GetFilteredTransitions(string group)
    {
        transitions.RemoveAll(t => !groups.Contains(t.group));
        return transitions.FindAll(t => t.group == group);
    }

    public void FilterTransitions(string group)
    {
        MakeTransitionData.ResetCurrentID();
        transitions.ForEach(t => t.ResetID());
        
        filtered_transitions = GetFilteredTransitions(group);
        filtered_transitions.ForEach(t => t.SetID());
    }
    
}
[System.Serializable]
public class MakeTransitionData
{
    public MakeTransitionData GetClone(string group = "")
    {
        return new MakeTransitionData()
        {
            group = group == "" ? this.group : group,
            type = type,
            duration = duration,
            by = by,
            vector_value = vector_value,
            float_value = float_value
        };
    }
    public string group;
    
    public transition_type type;
    public float duration;
    public bool by = false;
    public Vector3 vector_value = Vector3.one;
    public float float_value = 1f;
    static List<transition_type> in_setters = new List<transition_type>() { transition_type.move, transition_type.scale, transition_type.alpha };
    static List<transition_type> in_resetters = new List<transition_type>() { transition_type.then, transition_type.wait };
    public bool is_in_setter => in_setters.Contains(type);
    public bool is_in_resetter => in_resetters.Contains(type);

    public enum transition_type
    {
        move,
        scale,
        alpha,
        wait,
        then
    }
    public override string ToString()
    {
        return string.Format("{0} {1} #{2}", type,duration, id);
    }
    #region id setting
    public int id { get; protected set; }
    public void ResetID()
    {
        id = -1;
    }
    public void SetID()
    {
        id = current_id++;
    }
    static int current_id = 0;
    public static void ResetCurrentID()
    {
        current_id = 0;
    }
    #endregion
}


