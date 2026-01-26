using JetBrains.Annotations;
using Content.Shared.Utopia.Economy;

namespace Content.Client._Utopia.Economy.UI;

[UsedImplicitly]
public sealed class SalaryConsoleBui : BoundUserInterface
{
    private SalaryConsoleWindow? _window;

    public SalaryConsoleBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new SalaryConsoleWindow();
        _window.OnClose += Close;
        _window.OnPaymentRequested += SendMessage;

        if (State != null)
        {
            UpdateState(State);
        }

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        _window?.UpdateState(state);
    }

    protected override void Dispose(bool disposing)
    {
        _window?.Close();
        base.Dispose(disposing);
    }
}
