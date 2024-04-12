using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IEnumRunner.Transitions
{
    public class TransitionSwitch : TransitionArgs
    {
        public BaseAction this[string name]
        {
            get
            {
                switch (name)
                {
                    case "fixed":
                        return fixed_to_point;
                    case "fixed_sin_t":
                        return fixed_to_point_sin_t;
                    case "fixed_in_direction":
                        return fixed_in_direction;
                    case "constant_speed":
                        return constant_speed;
                    default:
                        throw new System.Exception(string.Format("Unknown transition '{0}'.", name));
                }
            }
        }



        public BaseAction fixed_to_point => new FixedTimeTransitionAction()
        {
            container = container,
            origin = origin,
            target = target,
            attribute = attribute,
            duration = duration
        };
        public BaseAction fixed_in_direction => new FixedTimeTransitionDirectionAction()
        {
            container = container,
            origin = origin,
            target = target,
            attribute = attribute,
            duration = duration
        };
        public BaseAction constant_speed => new ConstantSpeedTransitionAction()
        {
            component = container,
            target = target,
            attribute = attribute,
            speed = speed,
        };
        public BaseAction fixed_to_point_sin_t => new FixedTimeTransitionSinusTAction()
        {
            container = container,
            origin = origin,
            target = target,
            attribute = attribute,
            duration = duration
        };

        protected override BaseAction ToBaseAction() => fixed_to_point;


    }

    public abstract class TransitionArgs
    {

        public Container container;
        public Value origin;
        public Value target;
        public attribute_type attribute;
        public float speed;
        public float duration;

        public static implicit operator BaseAction(TransitionArgs args) => args.ToBaseAction();
        public static implicit operator Sequence(TransitionArgs args) => (BaseAction)args;

        public static Sequence operator +(TransitionArgs a, TransitionArgs b) => Sequence.New(a, b);

        protected abstract BaseAction ToBaseAction();
        public BaseAction Paralell() => ToBaseAction().Paralell();
    }

    public class ScaleRotationSwitch : TransitionArgs
    {
        public Value rotation_origin;
        public Value scale_origin;

        public TransitionSwitch scale => container.Transition(scale_origin, target, attribute_type.localScale, duration);
        public TransitionSwitch rotation => container.Transition(rotation_origin, target, attribute_type.rotation_z, duration);

        protected override BaseAction ToBaseAction() => scale;

    }

}
