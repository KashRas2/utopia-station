using Content.Shared._Utopia.Teleportation;
using Content.Shared._Utopia.Telescience.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Interaction;

namespace Content.Shared._Utopia.Telescience.Systems;

public sealed class TelescienceComputerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelescienceComputerComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<TelescienceComputerComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<TelescienceComputerComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<TelescienceComputerComponent, TelescienceSendMessage>(OnSendMessage);
        SubscribeLocalEvent<TelescienceComputerComponent, TelescienceRetrieveMessage>(OnRetrieveMessage);
        SubscribeLocalEvent<TelescienceComputerComponent, TelescienceOpenPortalMessage>(OnOpenPortalMessage);
        SubscribeLocalEvent<TelescienceComputerComponent, TelescienceClosePortalMessage>(OnClosePortalMessage);
        SubscribeLocalEvent<TelescienceComputerComponent, TelescienceCooldownEvent>(OnCooldownEvent);
        SubscribeLocalEvent<TelescienceComputerComponent, TelesciencePositionMessage>(OnPositionMessage);
    }

    private void OnInteractUsing(Entity<TelescienceComputerComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<TeleportCrystalComponent>(args.Used, out var crys)
        || crys.CType == CrystalType.Redspace)
            return;

        args.Handled = true;

        Del(args.Used);
        TryAddCrystal(ent);
    }

    private void OnNewLink(Entity<TelescienceComputerComponent> ent, ref NewLinkEvent arg)
    {
        if (!TryComp<TelescienceTeleporterComponent>(arg.Sink, out var telepad))
            return;

        ent.Comp.TeleporterUid = arg.Sink;
        telepad.Computer = ent;
        Dirty(arg.Sink, telepad);
        Dirty(ent);
    }

    private void OnPortDisconnected(Entity<TelescienceComputerComponent> ent, ref PortDisconnectedEvent arg)
    {

        if (arg.Port != ent.Comp.LinkingPort || ent.Comp.TeleporterUid == null)
            return;

        if (TryComp<TelescienceTeleporterComponent>(ent.Comp.TeleporterUid, out var telepad))
        {
            telepad.Computer = null;
            Dirty(ent.Comp.TeleporterUid.Value, telepad);
        }

        ent.Comp.TeleporterUid = null;
        Dirty(ent);
    }

    private void OnSendMessage(Entity<TelescienceComputerComponent> ent, ref TelescienceSendMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        if (!TryConsumeCrystal(ent))
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
        var ev = new TelescienceSendEvent(arg.Coordinates);
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
    }

    private void OnRetrieveMessage(Entity<TelescienceComputerComponent> ent, ref TelescienceRetrieveMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        if (!TryConsumeCrystal(ent))
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
        var ev = new TelescienceRetrieveEvent(arg.Coordinates);
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
    }

    private void OnOpenPortalMessage(Entity<TelescienceComputerComponent> ent, ref TelescienceOpenPortalMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        if (!TryConsumeCrystal(ent))
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
        var ev = new TelescienceOpenPortalEvent(arg.Coordinates);
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
    }

    private void OnClosePortalMessage(Entity<TelescienceComputerComponent> ent, ref TelescienceClosePortalMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        var ev = new TelescienceClosePortalEvent();
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
    }

    private void OnPositionMessage(Entity<TelescienceComputerComponent> ent, ref TelesciencePositionMessage arg)
    {
        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
    }

    private void OnCooldownEvent(Entity<TelescienceComputerComponent> ent, ref TelescienceCooldownEvent arg)
    {
        ent.Comp.CooldownTime = arg.Cooldown;
        Dirty(ent);
    }

    private bool TryAddCrystal(Entity<TelescienceComputerComponent> ent)
    {
        ent.Comp.Crystals++;
        Dirty(ent);

        return true;
    }

    private bool TryConsumeCrystal(Entity<TelescienceComputerComponent> ent)
    {
        if (ent.Comp.Crystals <= 0)
            return false;

        ent.Comp.Crystals--;
        Dirty(ent);

        return true;
    }
}
