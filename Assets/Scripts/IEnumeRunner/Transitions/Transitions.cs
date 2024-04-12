using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace IEnumRunner.Transitions
{
    public static class Wait
    {
        public static WaitAction While(System.Func<bool> condition)=> new WaitAction(condition);
        public static WaitAction Until(System.Func<bool> condition) => new WaitAction(condition).Negative();
        public static WaitForSecondsAction For(float seconds) => new WaitForSecondsAction(seconds);
    }
    public static class Make
    {
        static _Batch batch_instance = new _Batch();
        
        public static _Batch The(Component c) => new _Batch(c);
        public static _Batch The(GameObject go) => new _Batch(go);
        public static SimpleLoopableAction Happen(System.Action action)=>new SimpleLoopableAction(action);
        public static BreakIfAction BreakIf(System.Func<bool> condition) => new BreakIfAction(condition);
        
        public class _Batch
        {
            bool instantly_flag = false;

            float time = 0f;
            
            public void Reset()
            {
                instantly_flag = false;
                time = 0f;
                actions.Clear();
                loop_condition = null;
                container = null;
            }

            public _Batch instantly {
                get
                {
                    time = 0f;
                    instantly_flag = true;
                    return this;
                }
            }
            string transition_type = "fixed_sin_t";
            public _Batch FixedTransition()
            {
                transition_type = "fixed";
                return this;
            }
            float[] transition_type_params = new float[] { 1f, 0f };
            public _Batch FixedSinTTransition(float multiplier = 1f, float phase_shift = 0f)
            {
                transition_type = "fixed_sin_t";
                transition_type_params = new float[] { multiplier, phase_shift };
                return this;
            }
            //public _Batch In(string param_name, float multiplier = 1f) => In(UITransitionParams.GetFloat(param_name) * multiplier);
            public _Batch In(float time_it_takes)
            {
                time = time_it_takes;
                instantly_flag = false;
                return this;
            }
            
            public _Batch then
            {
                get
                {
                    Add(Wait.For(0f), false);
                    return this;
                }
            }
            public void SetContainer(Container container) => this.container = container;
            Container container;
            List<BaseAction> actions = new List<BaseAction>();
            public BaseAction last_action => actions.FindLast((a) => true);
            public _Batch() { }
            public _Batch(Container container) => this.container = container;
            _Batch Add(BaseAction action, bool paralell = true)
            {
                
                actions.Add(paralell ? action.Paralell() : action);
                
                return this;
            }
            _Batch Add(TransitionSwitch t_switch, bool paralell = true)
            {

                BaseAction action = t_switch[transition_type];
                switch (transition_type)
                {
                    case "fixed_sin_t":

                        FixedTimeTransitionSinusTAction fttsta = action as FixedTimeTransitionSinusTAction;
                        fttsta.multiplier = transition_type_params[0];
                        fttsta.phase_shif = transition_type_params[1];
                        
                        break;

                }

                actions.Add(paralell ? action.Paralell() : action);

                return this;
            }

            public _Batch ChangeTo(Vector3? position = null, Color? color = null, Value scale = null, Value z_rotation = null, Value rotation = null, Value alpha = null)
            {
                if(z_rotation != null && !z_rotation.is_float)
                {
                    throw new System.Exception("Provided non-float value as z_rotation parameter.");
                }
                if (alpha != null && !alpha.is_float)
                {
                    throw new System.Exception("Provided non-float value as alpha parameter.");
                }

                if (z_rotation != null)
                {
                    RotateTo((float)z_rotation);
                }
                if (rotation != null)
                {
                    RotateTo((Vector3)z_rotation);
                }
                if (scale != null)
                {
                    ScaleTo(scale);
                }
                if (alpha != null)
                {
                    AlphaTo(alpha);
                }
                if (position != null)
                {
                    MoveTo(position.Value);
                }
                if (color != null)
                {
                    Color(color.Value);
                }

                return this;
                
            }
            
            public _Batch BreakIf(System.Func<bool> condition) => Add(new BreakIfAction(condition));
            

            public _Batch MoveTo(Container target) => Add(container.Move(target.position, time));
            //public _Batch MoveTo(Vector3 target) => Add(container.Move(target, time));
            public _Batch MoveTo(Value target) => Add(container.Move(target, time));
            public _Batch MoveBy(Vector3 by)
                => Add(container.Move(by, time).fixed_in_direction);
            public _Batch RotateTo(float rotation) => Add(container.Rotate(rotation, time));
            public _Batch RotateTo(Vector3 rotation) => Add(container.Rotate(rotation, time));
            public _Batch RotateBy(float rotation) => Add(container.Rotate(rotation, time).fixed_in_direction);

            public _Batch ScaleTo(Value scale) => Add(container.Scale(scale, time));  
            
            public _Batch ScaleBy(Value scale) => Add(container.Scale(scale, time).fixed_in_direction);
            public _Batch AlphaTo(float alpha) => Add(container.Alpha(alpha, time));
            public _Batch AlphaBy(float alpha) => Add(container.Alpha(alpha, time).fixed_in_direction);
            public _Batch CGAlphaTo(float alpha) => Add(container.CanvasGroupAlpha(alpha, time));
            public _Batch CGAlphaBy(float alpha) => Add(container.CanvasGroupAlpha(alpha, time).fixed_in_direction);
            public _Batch Color(Color set_color) => Add(container.Color(set_color, time));
            _Batch AddWaitAction(BaseAction action)
            {
                Add(action, false);
                return this;
            }
            public _Batch ThenWait(float time)=>AddWaitAction(Wait.For(time));
            public _Batch ThenWaitUntil(System.Func<bool> action) => AddWaitAction(Wait.Until(action).Negative());
            public _Batch ThenWaitWhile(System.Func<bool> action) => AddWaitAction(Wait.While(action));
            System.Func<bool> loop_condition;
            public _Batch LoopWhile(System.Func<bool> condition)
            {
                loop_condition = condition;
                return this;
            }
            public _Batch MakeHappen(System.Action action, float delay = 0f)
            {
                if(delay != 0f)
                {
                    Add(new WaitForSecondsAction(delay));
                }
                Add(Make.Happen(action));
                return this;
            }
            
            public Sequence Happen(int priority = 0)
            {
                Debug.Log("step1");
                Debug.Log(loop_condition);
                if (loop_condition == null)
                {
                    Sequence ret = ((Sequence)this).Run(priority);
                    Debug.Log("step2");

                    if (instantly_flag)
                    {
                        ret.MoveNext();
                    }
                    return ret;
                }
                else
                {
                    return ((Sequence)((Sequence)this).LoopWhile(loop_condition)).Run(priority);
                }
            }
            public IEnumerator Execute() => ((Sequence)this).Execute();
            public static implicit operator Sequence(_Batch b)
            {
                Sequence ret = Sequence.New();
                b.actions.ForEach(a => ret += a);
                return ret;
            }
            public static Sequence operator +(_Batch a, _Batch b) => (Sequence)a + (Sequence)b;
            public static Sequence operator +(float a, _Batch b) => Wait.For(a) + (Sequence)b;
            public static Sequence operator +(_Batch a, float b) => (Sequence)a + Wait.For(b);

            public static Sequence operator &(_Batch a, _Batch b) => (Sequence)a & (Sequence)b;

        }
        
    }
    public enum attribute_type
    {
        position,
        color,
        localScale,
        rotation,
        rotation_z,
        alpha,
        canvasGroup_alpha
    }
    public class Value
    {
        private float _float_value;

        public float float_value
        {
            get => float_value_getter == null ? _float_value : float_value_getter();
            protected set => _float_value = value;
        }

        public System.Func<float> float_value_getter { get; protected set; }
        private Vector4 _vector4_value;

        public Vector4 vector4_value
        {
            get => vector4_value_getter == null ? _vector4_value : vector4_value_getter(); 
            protected set => _vector4_value = value;
        }

        public System.Func<Vector4> vector4_value_getter { get; protected set; }
        
        public static Value FromGetter(System.Func<Vector4> getter)
        {
            return new Value() { vector4_value_getter = getter, is_float = false };
        }
        
        public static Value FromGetter(System.Func<float> getter)
        {
            return new Value() { float_value_getter = getter, is_float = true };
        }
        
        

        public static implicit operator float(Value p) => p.float_value;
        public static implicit operator Vector4(Value p) => p.vector4_value;
        
        public static implicit operator Vector3(Value p) => p.vector4_value;
        public static implicit operator Vector2(Value p) => p.vector4_value;


        public static implicit operator Value(float value) => new Value() { float_value = value, is_float = true };
        public static implicit operator Value(Vector4 value) => new Value() { vector4_value = value, is_float = false };
        
        public static implicit operator Value(Vector3 value) => new Value() { vector4_value = value, is_float = false };
        public static implicit operator Value(Vector2 value) => new Value() { vector4_value = value, is_float = false };
        public bool is_float { get; private set; }

        public static Value Lerp(Value origin, Value target, float t)
        {
            if (origin.is_float)
            {
                if (target.is_float)
                {
                    return (Value)Mathf.Lerp(origin, target, t);
                }
                return Vector3.Lerp(Vector3.one * origin.float_value, target.vector4_value, t);
                
            }
            if (target.is_float)
            {
                return (Value)Vector4.Lerp(origin, Vector3.one * target.float_value, t);                    
            }

            return Vector4.Lerp(origin, target, t);
        }
            
        // TODO: fix - broken for ScaleTowards - if that's even a thing
        public static Value MoveTowards(Value origin, Value target, float t) =>
            target.is_float ? (Value)Mathf.MoveTowards(origin, target, t) : (Value)Vector4.MoveTowards(origin, target, t);

        public static Value InverseLerp(Value origin, Value target, Value value)
        {
            if (origin.is_float)
            {
                return Mathf.InverseLerp(origin, target, value);
            }
            float origin_value_distance = Vector4.Distance(origin, value);
            float origin_target_distance = Vector4.Distance(origin, target);
            return Mathf.InverseLerp(0f, origin_target_distance, origin_value_distance);
        }
        public static float MagnitudeDifference(Value first, Value second)
        {
            return first.is_float ? Mathf.Abs(first.float_value - second.float_value) : Vector4.Distance(first.vector4_value, second.vector4_value);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Value))
            {
                return false;
            }
            Value tp = obj as Value;
            if (tp.is_float != is_float)
            {
                return false;
            }

            if (tp.is_float)
            {
                return Mathf.Approximately(tp.float_value, float_value);
            }
            return tp.vector4_value == vector4_value;

        }
        public static Value operator + (Value a, Value b)
        {
            Value ret = new Value();
            ret.float_value = a.float_value + b.float_value;
            ret.vector4_value = a.vector4_value + b.vector4_value;
            ret.is_float = a.is_float;
            return ret;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return base.ToString() + (is_float ? "float" : "Vector4") + string.Format(" {0}:{1}", float_value, vector4_value);
        }

    }
   
    // what do we transit:
    // position (calculated from 0=>1 distance relative to current distance for movement with constant speed derived from 0=>1)
    // scale (float)
    // localScale (float)
    // rotation (z axis)
    // graphic color
    // graphic alpha

    // on
    // transform
    // recttransform
    // graphic

    // we need: 
    // - what object to modify
    // - what property to modify
    // - if it's 0=>1 or constant speed(towards)
    // - original value for 0=>1
    // - target value
    // - duration 
    //  - or speed
        
    /// <summary>
    /// We're changing floats or Vector4s in TransitionActions, and use this attrocity make the life easier in them.
    /// </summary>
    
       

       
        
    public class ColorOwner
    {
        
        Graphic g;
        SpriteRenderer sr;
        public ColorOwner(GameObject gameObject)
        {
            if(gameObject.GetComponent<SpriteRenderer>() != null)
            {
                sr = gameObject.GetComponent<SpriteRenderer>();
            }
            if (gameObject.GetComponent<Graphic>() != null)
            {
                g = gameObject.GetComponent<Graphic>();
            }
        }
        ColorOwner(Graphic g) => this.g = g;
        ColorOwner(SpriteRenderer sr) => this.sr = sr;
        public static ColorOwner New(Component c)
        {
            if (c is Graphic) return new ColorOwner(c as Graphic);
            if (c is SpriteRenderer) return new ColorOwner(c as SpriteRenderer);
            throw new UnityException("Provided " + c + " as ColorOwner parameter.");
        }
        bool is_graphic => g != null;
        public Color color
        {
            get => is_graphic ? g.color : sr.color;
            set
            {
                if (is_graphic) g.color = value;
                else sr.color = value;
            }
        }
        public float alpha
        {
            get => color.a;
            set => color = (Vector4)(Vector3)(Vector4)color + (Vector4)Color.black * value;
        }
        /*public static implicit operator SpriteRenderer(ColorOwner co) => co.sr;
        public static implicit operator Graphic(ColorOwner co) => co.g;*/
        public static implicit operator Component(ColorOwner co)
        {
            if (co.is_graphic) return co.g;
            return co.sr;
        }

        public static implicit operator ColorOwner(Component sr) => New(sr);
            
    }

   
   

}