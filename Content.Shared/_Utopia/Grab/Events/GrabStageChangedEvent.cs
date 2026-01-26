using Content.Shared.Movement.Pulling.Components;

namespace Content.Shared.Utopia.Grab;

[ByRefEvent]
public record struct GrabStageChangedEvent(Entity<PullerComponent> Puller, Entity<PullableComponent> Pulling, GrabStage OldStage, GrabStage NewStage);
