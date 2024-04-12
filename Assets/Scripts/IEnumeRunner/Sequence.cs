using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IEnumRunner {
    public class Sequence
    {

        List<BaseAction> steps = new List<BaseAction>();
        
        public string created_stack_trace { get; protected set; }
        public string run_stack_trace { get; protected set; }
        static List<Sequence> pool = new List<Sequence>();
        public static Sequence New()
        {
            // add a mid step with "can be reused flag" if you need pooling
            return new Sequence();
            // Sequence ret = pool.Find(s => s.has_finished);
            //
            // if (ret != null)
            // {
            //     ret.Reset();
            //     ret.steps.Clear();
            //     ret.unid = current_unid++;
            //     ret.priority = 0;
            // }
            // else
            // {
            //     ret = new Sequence();
            //     pool.Add(ret);
            // }
            // return ret;
        }
        /// <summary>
        /// Use this with great caution! Nested IEnumerators don't execute!
        /// </summary>
        /// <param name="ienum"></param>
        /// <returns></returns>
        
        public static Sequence NewIEnumeratorSequence (IEnumerator ienum)
        {
            Sequence ret = New();
            ret.Add(new IEnumeratorAction(ienum));
            return ret;
        }
        public static Sequence New(params BaseAction[] actions)
        {
            Sequence ret = New();
            foreach(BaseAction ba in actions)
            {
                ret.Add(ba);
            }
            return ret;
        }
        public static Sequence New(params Sequence[] actions)
        {
            Sequence ret = New();
            foreach (Sequence s in actions)
            {
                ret.Add(s);
            }
            return ret;
        }
        Sequence() {
            created_stack_trace = Time.time + " \n" + System.Environment.StackTrace;
        }
        public Sequence Add(Sequence s)
        {
            steps.Add(new SequenceAction(s));
            return this;
        }
        public Sequence Add(BaseAction action)
        {
            steps.Add(action);
            return this;
        }

        public Sequence Add(float delay)
        {
            steps.Add(new WaitForSecondsAction(delay));
            return this;
        }
        public Sequence Add(System.Action action)
        {
            steps.Add(new SimpleLoopableAction(action));
            return this;
        }
        /*public Sequence Add(IEnumerator action)
        {
            steps.Add(new IEnumeratorAction(action));
            return this;
        }*/
        public Sequence Add(System.Func<bool> expression)
        {
            steps.Add(new WaitAction(expression));
            return this;
        }
        public int priority;
        public Sequence Run(int priority = 0)
        {
            run_stack_trace = Time.time + " \n" + System.Environment.StackTrace;
            if (is_running)
            {
                Stop();
            }
            this.priority = priority;
            
            SequenceRunner.Run(this);
            return this;
        }
        public void Stop()
        {
            SequenceRunner.Stop(this);
        }
        IEnumerator sequence_current;
        public void MoveNext()
        {
            if (sequence_current == null || !is_running)
            {
                sequence_current = Execute();
                is_running = true;
            }
            sequence_current.MoveNext();

        }
        public void Reset()
        {
            is_running = true;
            sequence_current = null;
            has_started = false;
        }

        public bool is_running { get; protected set; } = true;
        public bool has_started { get; protected set; } = false;
        public bool has_finished => has_started && !is_running;
        
        public bool debug = false;
        public IEnumerator Execute()
        {
            has_started = true;
            if (debug)
            {
                Debug.Log("starting sequence #" + unid);
                steps.ForEach(s => Debug.Log(s));
            }

            steps.ForEach(s => s.Reset());

            for (int i = 0; i < steps.Count; i++)
            {


                BaseAction action = steps[i];
                //Debug.Log(action);
                if (action.is_paralell)
                {
                    List<BaseAction> paralell_actions = new List<BaseAction>() { action };

                    int current_index = i + 1;
                    BaseAction currently_checked_action = current_index < steps.Count ? steps[current_index] : null;
                    while (currently_checked_action != null && currently_checked_action.is_paralell)
                    {
                        paralell_actions.Add(currently_checked_action);
                        current_index++;
                        if (current_index >= steps.Count)
                        {
                            break;
                        }
                        currently_checked_action = steps[current_index];
                    }
                    i = current_index - 1;

                    if (debug)
                    {
                        paralell_actions.ForEach(a => Debug.Log("paralell action " + a + " run at " + Time.time));

                    }

                    while (paralell_actions.Count > 0)
                    {
                        paralell_actions.ForEach(a => a.Execute().MoveNext());
                        paralell_actions.RemoveAll(a => !a.is_playing);
                        if(paralell_actions.Count != 0)
                        {

                            yield return null;
                        }
                        
                    }
                }


                else
                {
                    if (debug)
                    {
                        Debug.Log("action " + action + " run at " + Time.time);
                    }

                    while (action.is_playing)
                    {
                        
                        action.Execute().MoveNext();
                        if (action.is_playing)
                        {
                            yield return null;
                        }
                    }

                }
            }
            
            is_running = false;

            FinishedCallback?.Invoke();
        }
        public System.Action FinishedCallback;
        public System.Func<bool> break_condition { get; protected set; }
        public System.Action break_action { get; protected set; }
        public Sequence BreakIf(System.Func<bool> condition, System.Action action = null)
        {
            break_action = action;
            break_condition = condition;
            return this;
        }
        public SequenceAction LoopWhile(System.Func<bool> condition) => new SequenceAction(this).LoopWhile(condition) as SequenceAction;
        // adding actions

        public static Sequence operator +(Sequence sequence, BaseAction action) => sequence.Add(action);
        public static Sequence operator &(Sequence sequence, BaseAction action) => sequence.Add(action.Paralell());

        public static Sequence operator +(Sequence sequence, float delay) => sequence.Add(delay);
        public static Sequence operator +(Sequence sequence, System.Action action) => sequence.Add(action);
        public static Sequence operator &(Sequence sequence, System.Action action) => sequence.Add(new SimpleLoopableAction(action).Paralell());
        //public static Sequence operator +(Sequence sequence, IEnumerator action) => sequence.Add(action);

        // adding sequences together
        public static Sequence operator +(Sequence a, Sequence b) => New(a, b);


        public static Sequence operator &(Sequence a, Sequence b) => New(new SequenceAction(a).Paralell(), new SequenceAction(b).Paralell());
        public static bool operator ==(Sequence a, Sequence b) => a is null && b is null || !(a is null) && a.Equals(b);
        public static bool operator !=(Sequence a, Sequence b) => !(a == b);

        static int current_unid;
        public int unid { get; protected set; } = current_unid++;
        public override int GetHashCode()
        {
            return unid;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Sequence)) return false;
            return ((Sequence)obj).unid == unid;
        }
        /*public static Sequence operator +(Sequence sequence, (RectTransform component, Vector3 target, float duration) data)
            => sequence.Add(data.component.Transition(data.target, data.duration));
        public static Sequence operator +(Sequence sequence, (Transform component, Vector3 target, float duration) data)
            => sequence.Add(data.component.Transition(data.target, data.duration));
        public static Sequence operator +(Sequence sequence, (Transform component, float scale, float duration) data)
            => sequence.Add(data.component.Transition(data.scale, data.duration).scale);
        public static Sequence operator +(Sequence sequence, (Graphic component, float alpha, float duration) data)
            => sequence.Add(data.component.Transition(data.alpha, data.duration));
        public static Sequence operator +(Sequence sequence, (Graphic component, Color color, float duration) data)
            => sequence.Add(data.component.Transition((Vector4)data.color, data.duration));

        public static Sequence operator +(Sequence sequence, (SpriteRenderer component, float alpha, float duration) data)
            => sequence.Add(data.component.Transition(data.alpha, data.duration));
        public static Sequence operator +(Sequence sequence, (SpriteRenderer component, Color color, float duration) data)
            => sequence.Add(data.component.Transition((Vector4)data.color, data.duration));
        public static Sequence operator +(Sequence sequence, object obj)
        {
            throw new System.Exception("Don't know what to do with " + obj);
        }*/
        
        public static void EnableRunner()
        {
            SequenceRunner.Enable();
        }
        public static void DisableRunner()
        {
            SequenceRunner.Disable();
        }
        class SequenceRunner : MonoBehaviour {
            static SequenceRunner _inst;
            static void Init()
            {
                if (!enabled)
                {
                    return;
                }
                if (_inst == null) _inst = FindFirstObjectByType<SequenceRunner>();
                if (_inst == null) _inst = new GameObject("SequenceRunner").AddComponent<SequenceRunner>();
            }

            private static bool enabled = false;
            public static void Enable()
            {
                enabled = true;
            }

            public static void Disable()
            {
                enabled = false;   
            }

            private void OnDestroy()
            {
                Disable();
            }

            List<Sequence> running_sequences = new List<Sequence>();
            List<Sequence> add_sequences = new List<Sequence>();
            List<Sequence> remove_sequences = new List<Sequence>();
            public static void Run(Sequence s) {
                if(!enabled)
                {
                    return;
                }
                Init();
                _inst.add_sequences.Add(s);
                if (_inst.remove_sequences.Contains(s))
                {
                    _inst.remove_sequences.Remove(s);
                }

            }
            public static void Stop(Sequence s)
            {
                if (!enabled)
                {
                    return;
                }
                Init();
                if (_inst.running_sequences.Contains(s)) {
                    _inst.remove_sequences.Add(s);
                    //Debug.Log("stop sequence " + s.unid);
                }
            }
            public void Update()
            {
                if (!enabled)
                {
                    return;
                }
                //DevMeasureTimes times = new DevMeasureTimes("Update SequenceRunner");
                running_sequences.ForEach(delegate (Sequence s)
                {
                    if (s.break_condition != null && s.break_condition())
                    {
                        s.is_running = false;
                        s.break_action?.Invoke();
                    }
                });
                //times += "sequence break conditions";
                running_sequences.AddRange(add_sequences);
                running_sequences.Sort((a, b) => a.unid.CompareTo(b.unid));
                running_sequences.Sort((a, b) => a.priority.CompareTo(b.priority));
                //times += "adding sequences";
                
                
                add_sequences.Clear();
                remove_sequences.ForEach(s => s.is_running = false);
                remove_sequences.Clear();

                //times += "clearing sequences";
                
                List<Sequence> reset_sequences = running_sequences.FindAll(s => !s.is_running);
                running_sequences.RemoveAll(s => !s.is_running);
                //times += "removing sequences";
                List<string> sequence_run_sts = new List<string>();
                foreach (Sequence s in running_sequences)
                {
                    try
                    {
                        s.MoveNext();
                        //times += "sequence " + sequence_run_sts.Count();
                        sequence_run_sts.Add(s.run_stack_trace);
                    }
                    catch(System.Exception e)
                    {
                        Debug.LogError("Sequence created stack trace: " + s.created_stack_trace);
                        Debug.LogError("Run stack trace: " + s.run_stack_trace);
                        Debug.LogError("Exception stack trace: " + Time.time + " \n" + e.StackTrace);
                        throw e;
                    }
                }
                
                
                /**
                 * [[[]]] All finished sequences were reset here for some reason. If something breaks, refer here!!!!!
                 */

                //reset_sequences.ForEach(s => s.Reset());
                
                
                
                
                
                //times += "resetting";
                /*if(times.total_elapsed > .05f)
                {
                    Debug.Log(times);
                    for(int i = 0; i < sequence_run_sts.Count; i++)
                    {
                        Debug.Log("stack trace " + i + "\n" + sequence_run_sts[i]);
                    }

                }
                times.End();*/
                
            }

            //List<Sequence> reset_sequences = new List<Sequence>();
        }
        public IEnumerator WaitWhileRunning()
        {
            while (is_running)
            {
                yield return null;
            }
        }
    }



}
