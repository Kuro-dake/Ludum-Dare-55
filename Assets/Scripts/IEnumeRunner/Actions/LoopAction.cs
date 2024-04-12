using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IEnumRunner
{
    public abstract class LoopAction : BaseAction
    {
        protected abstract void LoopStep();
        public override IEnumerator Execute()
        {
            //Debug.Log(this + " executes");
            while (is_playing)
            {
                LoopStep();
                yield return null;
            }
            
        }
    }

    public class SimpleLoopableAction : LoopAction
    {
        System.Action action;
        bool _is_playing = true;
        bool is_looping = false;
        public override void Reset()
        {
            _is_playing = true;
            is_looping = false;
        }
        public override bool is_playing => loop_condition == null ? _is_playing : loop_condition();

        System.Func<bool> loop_condition;
        public SimpleLoopableAction LoopWhile(System.Func<bool> loop_condition)
        {
            this.loop_condition = loop_condition;
            return this;
        }
        public SimpleLoopableAction Loop()
        {
            is_looping = true;
            return this;
        }
        public SimpleLoopableAction StopLoop()
        {
            is_looping = false;
            return this;
        }
        protected override void LoopStep()
        {
            action();
            _is_playing = is_looping;
            
        }
        public SimpleLoopableAction(System.Action action) => this.action = action;
    }
 
    public class SequenceAction : SimpleLoopableAction
    {
        Sequence s;
        public override bool is_playing => s.is_running || base.is_playing;
        public SequenceAction(Sequence sequence) : base(sequence.MoveNext){ 
            s = sequence;
        }
        
        public override void Reset()
        {
            base.Reset();
            s.Reset();
        }
    }

    public abstract class LerpAction : LoopAction
    {
        public override bool is_playing => t < 1f;
        public float t { get; protected set; }
        public float duration;
        float _t_increment_multiplier;
        bool speed_initialized = false;
        protected float t_increment_multiplier => (speed_initialized ? _t_increment_multiplier : (_t_increment_multiplier = 1f / duration));

        // either duration, or a condition

        protected override void LoopStep()
        {
            if(duration == 0f)
            {
                t = 1f;
            }
            else
            {
                t = Mathf.Clamp(t + Time.deltaTime * t_increment_multiplier, 0f,1f);
            }
            
        }
        public override void Reset() => t = 0f;


    }
    public class WaitForSecondsAction : LoopAction
    {
        float current_time = 0f;
        float time;
        public WaitForSecondsAction(float time) => this.time = time;
        public override bool is_playing => current_time < time;
        protected override void LoopStep()
        {
            current_time += Time.deltaTime;
            //Debug.Log("time " + current_time);
        }
        public override void Reset()  {
            current_time = 0f;
            //Debug.Log("reset time " + current_time + " " + time);
        }
    }

}
