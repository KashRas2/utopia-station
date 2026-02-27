using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared._Utopia.Economy;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Utopia.Economy;

public sealed class ATMSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ATMComponent, EntInsertedIntoContainerMessage>(OnCardInserted);
        SubscribeLocalEvent<ATMComponent, EntRemovedFromContainerMessage>(OnCardRemoved);
        SubscribeLocalEvent<ATMComponent, ATMRequestWithdrawMessage>(OnWithdrawRequest);
        SubscribeLocalEvent<ATMComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<ATMComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<ATMComponent, ComponentRemove>(OnComponentRemoved);
        SubscribeLocalEvent<ATMComponent, GotEmaggedEvent>(OnEmag);
    }

    private void OnEmag(EntityUid uid, ATMComponent component, ref GotEmaggedEvent args)
    {
        args.Handled = true;
    }

    private void OnComponentStartup(EntityUid uid, ATMComponent component, ComponentStartup args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnComponentRemoved(EntityUid uid, ATMComponent component, ComponentRemove args)
    {
        if (!_itemSlots.TryGetSlot(uid, component.SlotId, out var slot))
            return;

        if (_itemSlots.TryEject(uid, slot, null, out _))
        {
            _itemSlots.RemoveItemSlot(uid, slot);
        }
    }

    private void OnInteractUsing(EntityUid uid, ATMComponent component, InteractUsingEvent args)
    {
        if (!_itemSlots.TryGetSlot(uid, component.SlotId, out var slot))
            return;

        if (!TryComp<CurrencyComponent>(args.Used, out var currency) || !currency.Price.Keys.Contains(component.CurrencyType))
            return;

        if (!slot.Item.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-trying-insert-cash-error"), args.Target, args.User, PopupType.Medium);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        var stack = Comp<StackComponent>(args.Used);
        var bankCard = Comp<BankCardComponent>(slot.Item.Value);
        var amount = stack.Count;

        if (_random.Prob(component.ErrorChance))
        {
            Del(args.Used);
            args.Handled = true;

            _stackSystem.SpawnAtPosition(amount, _prototypeManager.Index(component.CreditStackPrototype),
                Transform(uid).Coordinates);

            _audioSystem.PlayPvs(component.SoundWithdrawCurrency, uid);
            _popupSystem.PopupEntity(Loc.GetString("atm-error"), uid);
            return;
        }

        if (!_bankCardSystem.TryChangeBalance(bankCard.AccountId!.Value, amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-deposit-failed"), uid, args.User, PopupType.Medium);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        Del(args.Used);
        args.Handled = true;

        _audioSystem.PlayPvs(component.SoundInsertCurrency, uid);
        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.AccountId.Value), true,
            Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardInserted(EntityUid uid, ATMComponent component, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp<BankCardComponent>(args.Entity, out var bankCard) || !bankCard.AccountId.HasValue)
            return;

        UpdateUiState(uid, _bankCardSystem.GetBalance(bankCard.AccountId.Value), true,
            Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void OnCardRemoved(EntityUid uid, ATMComponent component, EntRemovedFromContainerMessage args)
    {
        UpdateUiState(uid, -1, false, Loc.GetString("atm-ui-insert-card"));
    }

    private void OnWithdrawRequest(EntityUid uid, ATMComponent component, ATMRequestWithdrawMessage args)
    {
        if (!_itemSlots.TryGetSlot(uid, component.SlotId, out var slot))
            return;

        if (!TryComp<BankCardComponent>(slot.Item, out var bankCard)
        || !bankCard.AccountId.HasValue)
        {
            if (slot.ContainerSlot != null)
            {
                _container.EmptyContainer(slot.ContainerSlot);
            }

            return;
        }

        if (!_bankCardSystem.TryGetAccount(bankCard.AccountId.Value, out var account)
        || account.AccountPin != args.Pin && !HasComp<EmaggedComponent>(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-wrong-pin"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        if (!_bankCardSystem.TryChangeBalance(account.AccountId, -args.Amount))
        {
            _popupSystem.PopupEntity(Loc.GetString("atm-not-enough-cash"), uid);
            _audioSystem.PlayPvs(component.SoundDeny, uid);
            return;
        }

        _stackSystem.SpawnAtPosition(args.Amount, _prototypeManager.Index(component.CreditStackPrototype),
            Transform(uid).Coordinates);

        _audioSystem.PlayPvs(component.SoundWithdrawCurrency, uid);

        UpdateUiState(uid, account.Balance, true, Loc.GetString("atm-ui-select-withdraw-amount"));
    }

    private void UpdateUiState(EntityUid uid, int balance, bool hasCard, string infoMessage)
    {
        var state = new ATMBuiState
        {
            AccountBalance = balance,
            HasCard = hasCard,
            InfoMessage = infoMessage
        };

        _ui.SetUiState(uid, ATMUiKey.Key, state);
    }
}
