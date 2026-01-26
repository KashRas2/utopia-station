using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Serialization;

namespace Content.Shared.Utopia.Economy;

[RegisterComponent]
public sealed partial class SalaryConsoleComponent : Component
{
    [DataField]
    public ItemSlot IdCardSlot = new();

    public string SlotId = "IdCardSlot";
}

[Serializable, NetSerializable]
public enum SalaryConsoleUiKey
{
    Key
}
