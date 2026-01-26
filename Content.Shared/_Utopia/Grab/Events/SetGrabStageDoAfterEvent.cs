using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Utopia.Grab;

[Serializable, NetSerializable]
public sealed partial class SetGrabStageDoAfterEvent : SimpleDoAfterEvent
{
    public int Direction;

    public SetGrabStageDoAfterEvent(int direction)
    {
        Direction = direction;
    }
}
