using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IEnumRunner.Transitions;
namespace IEnumRunner
{
    public class InterpolateTransitionAction : ConstantSpeedTransitionAction
    {
        public InterpolateTransitionAction(Component component, attribute_type attribute, Value origin, Value target, float duration) 
        {
            this.component = component;
            this.attribute = attribute;
            this.target = target;

            float magnitude = Value.MagnitudeDifference(origin, target);
            speed = (1f / duration) * magnitude;

        }
    }
}

