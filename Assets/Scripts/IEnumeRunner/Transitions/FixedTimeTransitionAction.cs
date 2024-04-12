using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using IEnumRunner.Transitions;
namespace IEnumRunner
{
    public class FixedTimeTransitionAction : LerpAction
    {   
        
        protected override void LoopStep()
        {

            base.LoopStep();

            Update();
            
            
        }
        protected void SetOriginIfNull()
        {
            // setting origin makes the target be retyped during lerp
            if (origin == null)
            {
                origin = container.GetValue(attribute);
                origin_started_null_flag = true;
            }
        }
        protected virtual void Update()
        {
            SetOriginIfNull(); 
            container.SetValue(attribute, Value.Lerp(origin, target, t));
        }

        public Container container;
        public Value origin;
        public Value target;
        public attribute_type attribute;
        protected bool origin_started_null_flag = false;
        public override void Reset()
        {

            base.Reset();
            if (origin_started_null_flag)
            {
                origin = null;

            }
            
        }
    }
    
    public class FixedTimeTransitionDirectionAction : FixedTimeTransitionAction
    {
        protected override void Update()
        {
            SetOriginIfNull();
            container.SetValue(attribute, Value.Lerp(origin, target + origin, t));
        }
    }

    public class FixedTimeTransitionSinusTAction : FixedTimeTransitionAction {

        public float multiplier = 1f;
        public float phase_shif = 0f;

        protected override void Update()
        {
            SetOriginIfNull();
            container.SetValue(attribute, Value.Lerp(origin, target, Common.SinT((t + phase_shif) * multiplier)));
        }

    }

}
