using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IEnumRunner
{
    public abstract class BaseAction
    {
        public bool is_paralell = false;
        public abstract bool is_playing { get; }
        public abstract IEnumerator Execute();
        public BaseAction Paralell()
        {
            is_paralell = true;
            return this;
        }
        public virtual void Reset()
        {
            Debug.Log("reset action");
        }
        public static Sequence operator +(BaseAction a, BaseAction b) => Sequence.New(a, b);
        public static Sequence operator &(BaseAction a, BaseAction b) => Sequence.New(a.Paralell(), b.Paralell());
        public static Sequence operator +(BaseAction a, System.Action b) => Sequence.New(a, new SimpleLoopableAction(b));
        public static Sequence operator &(BaseAction a, System.Action b) => Sequence.New(a.Paralell(), new SimpleLoopableAction(b).Paralell());
        public static Sequence operator +(BaseAction a, float b) => Sequence.New(a) + b;
        public static Sequence operator +(float a, BaseAction b)
        {
            Sequence ret = Sequence.New();
            ret.Add(a);
            ret.Add(b);
            return ret;
        }

        public static implicit operator Sequence(BaseAction a)=>Sequence.New(a);
        public Sequence ToSequence() => (Sequence)this;

        static int current_unid;
        int unid = current_unid++;
        public override int GetHashCode() => unid;
        public override string ToString()
        {
            return base.ToString() + " " + GetHashCode();
        }
    }

   
    public class IEnumeratorAction : BaseAction
    {
        bool _is_playing = true;
        public override bool is_playing => _is_playing;
        IEnumerator action;
        public IEnumeratorAction(IEnumerator action) => this.action = action;
        public override IEnumerator Execute()
        {
            while(action.MoveNext())
            {
                yield return null;
            }
            
            _is_playing = false;
            
        }
        public override void Reset() => _is_playing = true;
    }
    public class WaitAction : BaseAction
    {
        System.Func<bool> expression;
        public WaitAction(System.Func<bool> expression) => this.expression = expression;
        public override bool is_playing => negative ? !expression() : expression();
        public override IEnumerator Execute()
        {
            while (is_playing)
            {
                yield return null;
            }
        }
        public override void Reset() { }
        bool negative = false;
        public WaitAction Negative() { 
            negative = true;
            return this;
        }
    }

    public class BreakIfAction : BaseAction
    {
        bool play_flag = true;
        public override bool is_playing => play_flag;
        public override void Reset()
        {
            base.Reset();
            play_flag = true;
        }
        System.Func<bool> condition;
        public BreakIfAction(System.Func<bool> condition) => this.condition = condition;
        public override IEnumerator Execute()
        {
            if (condition())
            {
                yield break;
            }
        }
    }
  
   
}
