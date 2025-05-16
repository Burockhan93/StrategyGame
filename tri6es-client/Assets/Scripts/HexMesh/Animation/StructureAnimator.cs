using Shared.Structures;

public abstract class StructureAnimator<T> : EmptyAnimator where T : Structure
{
    public T structure;
}
