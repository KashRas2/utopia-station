using Content.Server.Roles.Jobs;
using Content.Shared.Roles;
using Content.Shared.Utopia.Economy;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Prototypes;

namespace Content.Server.Utopia.Economy;

public sealed class SalaryConsoleSystem : SharedEconomySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SalaryConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SalaryConsoleComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<SalaryConsoleComponent, EntInsertedIntoContainerMessage>(OnCardInserted);
        SubscribeLocalEvent<SalaryConsoleComponent, EntRemovedFromContainerMessage>(OnCardRemoved);

        Subs.BuiEvents<SalaryConsoleComponent>(SalaryConsoleUiKey.Key, subs =>
        {
            subs.Event<SalaryPaymentMessage>(OnPaymentRequested);
            subs.Event<BoundUIOpenedEvent>(OnBoundUIOpened);
        });
    }

    private void OnInit(EntityUid uid, SalaryConsoleComponent component, ComponentInit args)
    {
        _itemSlotsSystem.AddItemSlot(uid, component.SlotId, component.IdCardSlot);
    }

    private void OnRemoved(EntityUid uid, SalaryConsoleComponent component, ComponentRemove args)
    {
        if (!_itemSlotsSystem.TryGetSlot(uid, component.SlotId, out var slot))
            return;

        _itemSlotsSystem.TryEject(uid, slot, null, out _);
        _itemSlotsSystem.RemoveItemSlot(uid, slot);
    }

    private void OnCardInserted(EntityUid uid, SalaryConsoleComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != component.SlotId)
            return;

        if (!TryComp<BankCardComponent>(args.Entity, out var bankCard) || !bankCard.AccountId.HasValue || !bankCard.CommandBudgetCard)
        {
            _container.EmptyContainer(args.Container);
            return;
        }

        UpdateUiState(uid, component);
    }

    private void OnCardRemoved(EntityUid uid, SalaryConsoleComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != component.SlotId)
            return;

        UpdateUiState(uid, component);
    }

    private void OnBoundUIOpened(EntityUid uid, SalaryConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUiState(uid, component);
    }

    private void OnPaymentRequested(EntityUid uid, SalaryConsoleComponent component, SalaryPaymentMessage args)
    {
        var cardEntity = _itemSlotsSystem.GetItemOrNull(uid, component.SlotId);
        if (cardEntity == null || !TryComp<BankCardComponent>(cardEntity.Value, out var bankCard) || !bankCard.AccountId.HasValue || !bankCard.CommandBudgetCard)
            return;

        var sourceAccountId = bankCard.AccountId.Value;
        var targetAccountId = args.TargetAccountId;
        var amount = args.Amount;

        if (amount == 0)
            return;

        var sourceBalance = _bankCardSystem.GetBalance(sourceAccountId);
        if (sourceBalance + amount < 0)
        {
            UpdateUiState(uid, component);
            return;
        }

        if (_bankCardSystem.TryChangeBalance(sourceAccountId, -amount) && _bankCardSystem.TryChangeBalance(targetAccountId, amount))
        {
            UpdateUiState(uid, component);
        }
    }

    private void UpdateUiState(EntityUid uid, SalaryConsoleComponent component)
    {
        var cardEntity = _itemSlotsSystem.GetItemOrNull(uid, component.SlotId);
        BankCardComponent? bankCard = null;
        var hasCard = false;

        if (cardEntity != null
            && TryComp<BankCardComponent>(cardEntity.Value, out var bankCardComp)
            && bankCardComp.AccountId.HasValue
            && bankCardComp.CommandBudgetCard)
        {
            bankCard = bankCardComp;
            hasCard = true;
        }

        var balance = 0;
        var infoMessage = Loc.GetString("salary-console-insert-card");
        var employees = new List<EmployeeData>();

        if (hasCard && bankCard != null)
        {
            balance = _bankCardSystem.GetBalance(bankCard.AccountId!.Value);
            infoMessage = string.Empty;
            if (bankCard.Jobs != null && bankCard.Jobs.Count > 0)
                employees = GetEmployeesForJobs(bankCard.Jobs);
        }

        var state = new SalaryConsoleBuiState
        {
            AccountBalance = balance,
            HasCard = hasCard,
            InfoMessage = infoMessage,
            Employees = employees
        };

        _ui.SetUiState(uid, SalaryConsoleUiKey.Key, state);
    }

    private List<EmployeeData> GetEmployeesForJobs(List<ProtoId<JobPrototype>> jobPrototypes)
    {
        var employees = new List<EmployeeData>();

        foreach (var account in Accounts)
        {
            if (account.Mind == null)
                continue;

            var mind = account.Mind.Value;
            if (!_job.MindTryGetJob(mind.Owner, out var jobPrototype))
                continue;

            if (!jobPrototypes.Contains(jobPrototype.ID))
                continue;

            var name = account.Name;
            if (string.IsNullOrEmpty(name) && mind.Comp.CharacterName != null)
            {
                name = mind.Comp.CharacterName;
            }
            if (string.IsNullOrEmpty(name))
            {
                name = Loc.GetString("salary-console-unknown-employee");
            }

            employees.Add(new EmployeeData
            {
                AccountId = account.AccountId,
                Name = name,
                JobTitle = jobPrototype.LocalizedName,
                JobPrototype = jobPrototype.ID
            });
        }

        return employees;
    }
}
