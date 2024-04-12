using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IEnumRunner.Transitions;
namespace IEnumRunner
{

    public class ConstantSpeedTransitionAction : LoopAction
    {
        public Container component;
        Value origin;
        public Value target;
        public attribute_type attribute;
        public float speed;
        protected override void LoopStep()
        {
            Value current_value = component.GetValue(attribute);
            if (origin == null)
            {
                origin = current_value;
            }
            
            component.SetValue(attribute, Value.MoveTowards(current_value, target, speed * Time.deltaTime));
            
        }
        
        public override bool is_playing => !component.GetValue(attribute).Equals(target);
        public override void Reset()
        {
            if(origin != null)
            {
                component.SetValue(attribute, origin);
            }
            
        }
            
    }
}
