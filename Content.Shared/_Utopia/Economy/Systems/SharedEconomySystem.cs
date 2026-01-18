using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Utopia.Economy;

public sealed partial class SharedEconomySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public readonly List<BankAccount> Accounts = new();

    public BankAccount CreateAccount(int accountId = default, int startingBalance = 0)
    {
        if (TryGetAccount(accountId, out var acc))
            return acc;

        BankAccount account;
        if (accountId == default)
        {
            int accountNumber;

            do
            {
                accountNumber = _random.Next(100000, 999999);
            } while (AccountExist(accountNumber));

            account = new BankAccount(accountNumber, startingBalance, _random);
        }
        else
        {
            account = new BankAccount(accountId, startingBalance, _random);
        }

        Accounts.Add(account);

        return account;
    }

    public bool AccountExist(int accountId)
    {
        return Accounts.Any(x => x.AccountId == accountId);
    }

    public bool TryGetAccount(int accountId, [NotNullWhen(true)] out BankAccount? account)
    {
        account = Accounts.FirstOrDefault(x => x.AccountId == accountId);
        return account != null;
    }

    public int GetBalance(int accountId)
    {
        if (!TryGetAccount(accountId, out var account))
            return 0;

        if (account.CommandBudgetAccount && account.AccountPrototype != null)
        {
            var query = EntityQueryEnumerator<StationBankAccountComponent>();
            while (query.MoveNext(out var _, out var stationBank))
            {
                if (stationBank.Accounts.TryGetValue(account.AccountPrototype.Value, out var balance))
                    return balance;
            }
            return 0;
        }

        return account.Balance;
    }

    public bool TryChangeBalance(int accountId, int amount)
    {
        var cargoSystem = _entMan.System<SharedCargoSystem>();

        if (!TryGetAccount(accountId, out var account))
            return false;

        if (account.CommandBudgetAccount && account.AccountPrototype != null)
        {
            var query = EntityQueryEnumerator<StationBankAccountComponent>();
            while (query.MoveNext(out var stationUid, out var stationBank))
            {
                if (stationBank.Accounts.ContainsKey(account.AccountPrototype.Value))
                {
                    var currentBalance = stationBank.Accounts[account.AccountPrototype.Value];
                    if (currentBalance + amount < 0)
                        return false;

                    cargoSystem.UpdateBankAccount((stationUid, stationBank), amount, account.AccountPrototype.Value);
                    return true;
                }
            }
            return false;
        }

        if (account.Balance + amount < 0)
            return false;

        account.Balance += amount;
        account.History ??= new List<TransactionsHistory>();
        account.History.Add(new TransactionsHistory(
            amount,
            _timing.CurTime,
            amount > 0 ? Loc.GetString("bank-deposit") : Loc.GetString("bank-withdrawal"),
            Loc.GetString("bank-system"),
            null
        ));

        if (account.CartridgeUid != null)
        {
            var suicideEvent = new BalanceChange(account.CartridgeUid);
            RaiseLocalEvent(suicideEvent);
        }

        return true;
    }
}
