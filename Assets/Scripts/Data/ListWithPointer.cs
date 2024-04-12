using System.Collections.Generic;
public class ListWithPointer<T> : List<T>
{
    protected int index { get; private set; } = 0;
    public T current => Get(index);
    public int current_index => index;
    public T next => Get(next_index);
    public T previous => Get(prev_index);
    public virtual bool can_move_prev => index - 1 >= 0;
    public virtual bool can_move_next => index + 1 < Count;
    T Get(int index)
    {
        if (index >= Count || index < 0)
        {
            return default;
        }
        return this[index];
    }
    public bool has_any_elements => Count > 0;
    protected virtual int next_index => !can_move_next ? Count - 1 : index + 1;
    protected virtual int prev_index => !can_move_prev ? 0 : index - 1;
    public bool MoveNext()
    {
        if (!can_move_next)
        {
            return false;
        }
        BeforeChange?.Invoke(this, null);
        index = next_index;
        AfterChange?.Invoke(this, null);
        return true;

    }
    public bool MovePrevious()
    {
        if (!can_move_prev)
        {
            return false;
        }
        BeforeChange?.Invoke(this, null);
        index = prev_index;
        AfterChange?.Invoke(this, null);
        return true;
    }
    public void Reset()
    {
        index = 0;
    }
    public bool SetCurrent(T to_set)
    {
        BeforeChange?.Invoke(this, null);
        int i = IndexOf(to_set);
        if (i == -1)
        {
            return false;
        }
        index = i;
        AfterChange?.Invoke(this, null);
        return true;
    }
    public event System.EventHandler BeforeChange;
    public event System.EventHandler AfterChange;
}

public class CircularList<T> : ListWithPointer<T>
{
    public override bool can_move_next => true;
    public override bool can_move_prev => true;
    protected override int next_index => index + 1 >= Count ? 0 : index + 1;
    protected override int prev_index => index - 1 < 0 ? Count - 1 : index - 1;


}
