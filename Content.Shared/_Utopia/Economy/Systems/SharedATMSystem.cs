using Content.Shared.Containers.ItemSlots;

namespace Content.Shared.Utopia.Economy;

public abstract class SharedATMSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ATMComponent, ComponentInit>(OnATMInit);
        SubscribeLocalEvent<ATMComponent, ComponentRemove>(OnATMRemoved);
    }

    protected virtual void OnATMInit(EntityUid uid, ATMComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, component.SlotId, component.IdCardSlot);
    }

    private void OnATMRemoved(EntityUid uid, ATMComponent component, ComponentRemove args)
    {
        if (_itemSlotsSystem.TryEject(uid, component.IdCardSlot, null, out _))
        {
            _itemSlotsSystem.RemoveItemSlot(uid, component.IdCardSlot);
        }
    }
}
