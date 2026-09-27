using System;

public enum StatusType { Poison, Burn, Weak, Stun }
public enum StackType { Duration, Intensity }

[Serializable]
public class StatusInstance
{
    public StatusType type;
    public int duration;
    public int stacks;
    public StackType stackType;

    public StatusInstance(StatusType type, int duration, int stacks, StackType stackType)
    {
        this.type = type;
        this.duration = duration;
        this.stacks = UnityEngine.Mathf.Max(1, stacks);
        this.stackType = stackType;
    }

}