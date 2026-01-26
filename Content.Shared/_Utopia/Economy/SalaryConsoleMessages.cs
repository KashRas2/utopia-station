using Robust.Shared.Serialization;

namespace Content.Shared.Utopia.Economy;

[Serializable, NetSerializable]
public sealed class SalaryConsoleBuiState : BoundUserInterfaceState
{
    public bool HasCard;
    public string InfoMessage = string.Empty;
    public int AccountBalance;
    public List<EmployeeData> Employees = new();
}

[Serializable, NetSerializable]
public sealed class EmployeeData
{
    public int AccountId;
    public string Name = string.Empty;
    public string JobTitle = string.Empty;
    public string JobPrototype = string.Empty;
}

[Serializable, NetSerializable]
public sealed class SalaryPaymentMessage : BoundUserInterfaceMessage
{
    public int TargetAccountId;
    public int Amount;

    public SalaryPaymentMessage(int targetAccountId, int amount)
    {
        TargetAccountId = targetAccountId;
        Amount = amount;
    }
}
