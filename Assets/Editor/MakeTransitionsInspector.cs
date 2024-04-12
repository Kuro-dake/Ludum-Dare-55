using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
[CustomEditor(typeof(MakeTransitions))]
public class MakeTransitionsInspector : Editor
{
    //The array property we will edit
    SerializedProperty wave, groups;

    //The Reorderable list we will be working with
    ReorderableList list, groups_list;
    MakeTransitions transitions => target as MakeTransitions;
    MakeTransitionData GetTransitionData(int index) => transitions.filtered_transitions[index];
    private void OnEnable()
    {
        //Gets the wave property in WaveManager so we can access it. 
        wave = serializedObject.FindProperty("filtered_transitions");
        groups = serializedObject.FindProperty("groups");

        //Initialises the ReorderableList. We are creating a Reorderable List from the "wave" property. 
        //In this, we want a ReorderableList that is draggable, with a display header, with add and remove buttons        
        list = new ReorderableList(serializedObject, wave, true, true, true, true);
        groups_list = new ReorderableList(serializedObject, groups, true, true, true, true);

        list.drawElementCallback = DrawCurrentTransitionsItems;
        list.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, current_group); };
        list.elementHeightCallback = ElementHeight;

        float max_height = 0f;
        transitions.groups.ForEach(g => max_height = Mathf.Max(max_height, TotalHeight(transitions.GetFilteredTransitions(g))));

        list.footerHeight = max_height;

        list.onAddCallback = (index) =>
        {
            transitions.transitions.Add(new MakeTransitionData() { group = current_group });
        };
        list.onRemoveCallback = (_list) =>
        {
            transitions.transitions.RemoveAll(t => t.id == GetTransitionData(_list.index).id);
        };

        groups_list.drawElementCallback = DrawGroupsItem;
        groups_list.drawHeaderCallback = (rect) => { EditorGUI.LabelField(rect, "Groups"); };
        groups_list.onAddCallback = (glist) =>
        {

            string new_group_template, new_group_name;
            new_group_template = new_group_name = "new_group";
            int number = 0;
            while (transitions.groups.Contains(new_group_name))
            {
                new_group_name = new_group_template + "_" + number;
                number++;
            }
            transitions.groups.Add(new_group_name);

        };
        //groups_list.elementHeightCallback = ElementHeight;

    }
    float TotalHeight(List<MakeTransitionData> transitions)
    {
        float ret = 25f;
        transitions.ForEach(t => ret += ElementHeight(t));
        return ret;
    }
    float ElementHeight(MakeTransitionData mtd)
    {
        MakeTransitionData.transition_type transition_type = mtd.type;
        float y = EditorGUIUtility.singleLineHeight + 9f;
        int length = data_display[transition_type].Length;
        y += (length + 1) * (EditorGUIUtility.singleLineHeight + 3f);

        return y + 3f;
    }
    float ElementHeight(int index) => ElementHeight(GetTransitionData(index));
    float item_y = 0f;

    string _current_group = null;
    string current_group => _current_group == null ? (_current_group = transitions.groups.Count == 0 ? null : transitions.groups[0]) : _current_group;
    MakeTransitionDataSettings Setting(string s) => data_settings[s];
    void DrawGroupsItem(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = groups_list.serializedProperty.GetArrayElementAtIndex(index); //The element in the list
        if (isFocused)
        {
            _current_group = transitions.groups[index];

        }
        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, 100f, EditorGUIUtility.singleLineHeight),
            element,
            GUIContent.none
            );

    }
    CircularList<Color> circular_colors = new CircularList<Color>()
    {
        Color.red, Color.blue, Color.black, Color.magenta, Color.cyan, Color.yellow, Color.green, Color.grey
    };
    Dictionary<MakeTransitionData.transition_type, string> symbols = new Dictionary<MakeTransitionData.transition_type, string>()
    {
        {MakeTransitionData.transition_type.alpha, "a" },
        {MakeTransitionData.transition_type.move, "▲" },
        {MakeTransitionData.transition_type.scale, "s" },
        {MakeTransitionData.transition_type.then, "»" },
        {MakeTransitionData.transition_type.wait, "w" },


    };
    void DrawCurrentTransitionsItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        MakeTransitionData mtd = GetTransitionData(index);
        if (mtd.group == "")
        {
            mtd.group = current_group;
        }
        if (mtd.group != current_group)
        {
            return;
        }

        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); //The element in the list
        item_y = 3f;
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + ElementHeight(index) - 2f, rect.width, 1f), new Color(0.5f, 0.5f, 0.5f, 1));
        float label_width = 30f;
        MakeTransitionData.transition_type transition_type = mtd.type;
        string symbol = symbols.ContainsKey(transition_type) ? symbols[transition_type] : "▮";
        symbol = string.Format("{0}{0}{0}{0}{0}", symbol);
        EditorGUI.LabelField(new Rect(rect.position + Vector2.up * item_y, new Vector2(label_width, EditorGUIUtility.singleLineHeight)), symbol, new GUIStyle() { fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = circular_colors.current } });
        circular_colors.MoveNext();
        AddPropertyField(new Rect(rect.x + label_width, rect.y, rect.width, rect.height), 100f, element, "type", "");

        foreach (string property in data_display[transition_type])
        {
            MakeTransitionDataSettings s = Setting(property);
            AddPropertyField(rect, s.width, element, property, s.label);
        }


        EditorGUI.BeginChangeCheck();

        int option = EditorGUI.Popup(new Rect(rect.x, rect.y + item_y, 20f, EditorGUIUtility.singleLineHeight), -1, popup_actions.Keys.ToArray());

        if (EditorGUI.EndChangeCheck())
        {
            popup_actions.ToList()[option].Value(index);
        }
        if (data_display[transition_type].Length == 0)
        {
            item_y += EditorGUIUtility.singleLineHeight + 3f;
        }
    }
    SortedDictionary<string, System.Action<int>> popup_actions => new SortedDictionary<string, System.Action<int>>()
    {
        {"Add Scale" ,(int index) => AddNew(index, MakeTransitionData.transition_type.scale)},
        {"Add Move" ,(int index) => AddNew(index, MakeTransitionData.transition_type.move)},
        {"Add Alpha" ,(int index) => AddNew(index, MakeTransitionData.transition_type.alpha)},
        {"Add Then" ,(int index) => AddNew(index, MakeTransitionData.transition_type.then)},
        {"Add Wait" ,(int index) => AddNew(index, MakeTransitionData.transition_type.wait)},
        { "Remove this", (index)=>{ transitions.transitions.RemoveAll(mtd=>mtd.id == GetTransitionData(index).id); } },
    };
    void AddNew(int index, MakeTransitionData.transition_type type)
    {
        transitions.transitions.Insert(index + 1, new MakeTransitionData() { group = current_group, type = type });
    }

    void AddPropertyField(Rect rect, float width, SerializedProperty element, string property_name, string label)
    {
        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y + item_y, width, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative(property_name),
            GUIContent.none
            );
        bool has_label = label != "";
        if (has_label)
        {
            EditorGUI.LabelField(new Rect(rect.x + Setting(property_name).width + 5f, rect.y + item_y, 60f, EditorGUIUtility.singleLineHeight), label);
        }
        item_y += EditorGUIUtility.singleLineHeight + 3f;
    }
    static Dictionary<MakeTransitionData.transition_type, string[]> data_display = new Dictionary<MakeTransitionData.transition_type, string[]>
    {
        {MakeTransitionData.transition_type.move, new string[]{"by", "vector_value", "duration" } },
        {MakeTransitionData.transition_type.scale, new string[]{"by", "vector_value", "duration" } },
        {MakeTransitionData.transition_type.alpha, new string[]{"by", "float_value", "duration" } },
        {MakeTransitionData.transition_type.wait, new string[]{"duration"} },
        {MakeTransitionData.transition_type.then, new string[]{} },
    };
    static Dictionary<string, MakeTransitionDataSettings> data_settings = new Dictionary<string, MakeTransitionDataSettings>() {
        {"duration", new MakeTransitionDataSettings()
        {
            label = "Duration",
            width = 50f
        } },
        {"by", new MakeTransitionDataSettings()
        {
            label = "by",
            width = 15f
        } },
        {"vector_value", new MakeTransitionDataSettings()
        {
            label = "Value",
            width = 200f
        } },
        {"float_value", new MakeTransitionDataSettings()
        {
            label = "Value",
            width = 200f
        } },
    };
    struct MakeTransitionDataSettings
    {
        public string label;
        public float width;
    }

    bool advanced_operations = false;
    //This is the function that makes the custom editor work
    public override void OnInspectorGUI()
    {
        circular_colors.Reset();
        List<string> original_groups = transitions.groups.ToList();
        transitions.FilterTransitions(current_group);
        serializedObject.Update();
        groups_list.DoLayoutList();
        if (Application.isPlaying && current_group != "")
        {
            if (GUILayout.Button("Run " + current_group))
            {
                transitions.Trigger(current_group);
            }

        }
        List<Pair<string, Pair<GameObject, float>>> pairs = transitions.group_targets;
        if (advanced_operations = GUILayout.Toggle(advanced_operations, "show advanced"))
        {
            transitions.groups.ForEach(g =>
            {
                Pair<string, Pair<GameObject, float>> current = pairs.Find(p => p.first == g);
                GameObject field_object = null;
                float time = 1f;
                if (current != null)
                {
                    field_object = current.second.first;
                    time = current.second.second;
                }
                field_object = (GameObject)EditorGUILayout.ObjectField(g + " target", field_object, typeof(GameObject), true);
                time = EditorGUILayout.FloatField(g + " speed", time);
                if (current == null)
                {
                    pairs.Add(Pair.New(g, Pair.New(field_object, 1f)));
                }
                else
                {
                    current.second.first = field_object;
                    current.second.second = time;
                }

            });

            transitions.groups.ForEach(g =>
            {

                if (current_group != "")
                {
                    if (GUILayout.Button("Replace " + current_group + " with " + g))
                    {
                        transitions.transitions.RemoveAll(t => t.group == current_group);
                        List<MakeTransitionData> datas =
                            transitions.transitions.FindAll(t => t.group == g).ConvertAll(t => t.GetClone(current_group));
                        transitions.transitions.AddRange(datas);
                        transitions.FilterTransitions(current_group);
                    }

                }
            });
            if (GUILayout.Button("Reverse " + current_group + " order"))
            {
                List<MakeTransitionData> datas =
                    transitions.transitions.FindAll(t => t.group == current_group);

                transitions.transitions.RemoveAll(t => t.group == current_group);
                datas.Reverse();
                transitions.transitions.AddRange(datas);
                transitions.FilterTransitions(current_group);
            }
        }

        transitions.group_targets = pairs;
        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        List<string> updated_groups = transitions.groups.ToList();
        string updated_groups_double = null;
        updated_groups.ForEach(g => updated_groups_double = updated_groups.Count(_g => g == _g) != 1 ? g : updated_groups_double);
        if (updated_groups_double != null)
        {
            transitions.groups.RemoveAt(transitions.groups.LastIndexOf(updated_groups_double));
        }
        List<string> previous_groups = original_groups.ToList();
        original_groups.RemoveAll(g => updated_groups.Contains(g));

        if (original_groups.Count > 0)
        {
            if (original_groups.Count != 1)
            {
                throw new UnityException("Previous group count is " + original_groups.Count + ". This shouldn't happen, logic error.");
            }
            string modified = original_groups[0];

            updated_groups.RemoveAll(g => previous_groups.Contains(g));
            if (updated_groups.Count > 0)
            {
                if (updated_groups.Count != 1)
                {
                    throw new UnityException("Updated group count is " + updated_groups.Count + ". This shouldn't happen, logic error.");
                }
                string new_value = updated_groups[0];

                transitions.RenameGroup(modified, new_value);
                //transitions.FilterTransitions(new_value);
                _current_group = new_value;

            }

        }

    }
}
