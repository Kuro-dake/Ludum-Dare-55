using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IEnumRunner.Transitions
{

    public static class ComponentExtensions
    {
    
        public static SimpleLoopableAction Action(this System.Action action) => new SimpleLoopableAction(action);
        public static WaitAction Action(this System.Func<bool> action) => new WaitAction(action);

        public static Container Container(this Component c) => new Container(c);
        public static Container Container(this GameObject go) => new Container(go);

    }
    
    public class Container
    {
        public GameObject gameObject { get; protected set; }
        public Container(GameObject contain) => gameObject = contain;
        public Container(Component contain) => gameObject = contain.gameObject;

        public Transform transform => gameObject.transform;
        public SpriteRenderer spriteRenderer => Get<SpriteRenderer>();
        public Graphic graphic => Get<Graphic>();
        public CanvasGroup canvasGroup => Get<CanvasGroup>();

        public RectTransform rectTransform => Get<RectTransform>();
        public Vector3 position
        {
            get => rectTransform != null ? (Vector3)rectTransform.anchoredPosition : transform.position;
            set
            {
                if(rectTransform != null)
                {
                    rectTransform.anchoredPosition = value;
                }
                else
                {
                    transform.position = value;
                }
            }
        }
        Vector3 _eulers;
        Vector3 eulers
        {
            get => _eulers;
            set
            {
                _eulers = value;
                transform.localRotation = Quaternion.Euler(value);
            }
        }
        public List<Container> children => gameObject.GetComponentsInChildren<Component>().ToList().ConvertAll(c => new Container(c));
        ColorOwner color_owner => new ColorOwner(gameObject);
        public T Get<T>() where T : Component => gameObject.GetComponent<T>();
        public Container SetValue(attribute_type type, Value value)
        {
            switch (type)
            {
                case attribute_type.alpha:
                    color_owner.alpha = value;
                    break;
                case attribute_type.color:
                    color_owner.color = (Vector4)value;
                    break;
                case attribute_type.localScale:
                    transform.localScale = value.is_float ? Vector3.one * value.float_value : (Vector3)value.vector4_value;
                    break;
                case attribute_type.position:
                    position = value;
                    break;
                case attribute_type.rotation_z:
                    Vector3 rotation = transform.localEulerAngles;
                    rotation.z = value;
                    transform.localRotation = Quaternion.Euler(rotation);
                    break;
                case attribute_type.rotation:
                    eulers = value;
                    break;
                case attribute_type.canvasGroup_alpha:
                    canvasGroup.alpha = value;
                    break;
                default:
                    throw new System.Exception("Undefined");
            }
            return this;
        }
        public Value GetValue(attribute_type type)
        {
            switch (type)
            {
                case attribute_type.alpha:
                    return color_owner.alpha;

                case attribute_type.color:
                    return (Vector4)color_owner.color;

                case attribute_type.localScale:
                    return transform.localScale;

                case attribute_type.position:
                    return position;

                case attribute_type.rotation_z:
                    return transform.localRotation.eulerAngles.z;

                case attribute_type.rotation:
                    return eulers;

                case attribute_type.canvasGroup_alpha:
                    return canvasGroup.alpha;
                default:
                    throw new System.Exception("Undefined");
            }

        }
        public Container Move(Component target) => Move((Container)target);
        public Container Move(Container target) => Move(target.position);
        
        public Container Move(Vector3 target) => SetValue(attribute_type.position, target);

        public Container Rotate(float z_axis) => SetValue(attribute_type.rotation_z, z_axis);
        public Container Rotate(Vector3 euler) => SetValue(attribute_type.rotation, euler);
        public Container Scale(Value scale) => SetValue(attribute_type.localScale, scale);
        
        public Container Alpha(float alpha) => SetValue(attribute_type.alpha, alpha);
        public Container CanvasGroupAlpha(float alpha) => SetValue(attribute_type.canvasGroup_alpha, alpha);
        public Container Color(Color color) => SetValue(attribute_type.color, (Vector4)color);
        public TransitionSwitch Move(Vector3 target, float duration)
            => Transition(null, target, attribute_type.position, duration);
        public TransitionSwitch Move(Value target, float duration)
            => Transition(null, target, attribute_type.position, duration);

        public TransitionSwitch Rotate(float rotation, float duration)
            => Transition(null, rotation, attribute_type.rotation_z, duration);
        public TransitionSwitch Rotate(Vector3 rotation, float duration)
            => Transition(null, rotation, attribute_type.rotation, duration);

        public TransitionSwitch Scale(Value scale, float duration)
            => Transition(null, scale, attribute_type.localScale, duration);

        public TransitionSwitch Alpha(float alpha, float duration)
            => Transition(null, alpha, attribute_type.alpha, duration);
        public TransitionSwitch CanvasGroupAlpha(float alpha, float duration)
            => Transition(null, alpha, attribute_type.canvasGroup_alpha, duration);

        public TransitionSwitch Color(Color color, float duration)
            => Transition(null, (Vector4)color, attribute_type.color, duration);
            


        public TransitionSwitch Transition(Value origin, Value target, attribute_type attribute, float duration)
        {
            return new TransitionSwitch()
            {
                container = this,
                origin = origin,
                target = target,
                attribute = attribute,
                duration = duration,
                speed = duration
            };
        }

        public static implicit operator GameObject(Container c) => c.gameObject;
        public static implicit operator Container(Component c) => new Container(c);
        public static implicit operator Container(GameObject go) => new Container(go);

    }
}
