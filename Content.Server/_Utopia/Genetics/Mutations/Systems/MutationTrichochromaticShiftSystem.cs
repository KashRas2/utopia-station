using System.Numerics;
using Content.Server._Utopia.Genetics.Mutations.Components;
using Content.Server.Humanoid;
using Content.Shared._Utopia.Genetics.Events;
using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Random;

namespace Content.Server._Utopia.Genetics.Mutations.Systems;

public sealed class MutationTrichochromaticShiftSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutationTrichochromaticShiftComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MutationTrichochromaticShiftComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutationTrichochromaticShiftComponent, TrichochromaticShiftActionEvent>(OnActivate);
    }

    private void OnStartup(EntityUid uid, MutationTrichochromaticShiftComponent comp, ref ComponentStartup args)
    {
        _actions.AddAction(uid, ref comp.GrantedAction, comp.ActionId);

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        var originalHair = new List<(string, List<Color>)>();
        var originalFacial = new List<(string, List<Color>)>();

        if (humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.Hair, out var hairMarkings))
        {
            foreach (var marking in hairMarkings)
            {
                originalHair.Add((marking.MarkingId, new List<Color>(marking.MarkingColors)));
            }
        }

        if (humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.FacialHair, out var facialMarkings))
        {
            foreach (var marking in facialMarkings)
            {
                originalFacial.Add((marking.MarkingId, new List<Color>(marking.MarkingColors)));
            }
        }

        comp.OriginalHairMarkings = originalHair;
        comp.OriginalFacialHairMarkings = originalFacial;
        comp.UsesSinceOriginal = 0;
    }

    private void OnShutdown(EntityUid uid, MutationTrichochromaticShiftComponent comp, ref ComponentShutdown args)
    {
        if (comp.GrantedAction is { Valid: true } action)
        {
            _actions.RemoveAction(action);
        }

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        humanoid.MarkingSet.RemoveCategory(MarkingCategories.Hair);
        humanoid.MarkingSet.RemoveCategory(MarkingCategories.FacialHair);

        if (comp.OriginalHairMarkings is { } hair)
        {
            foreach (var (id, colors) in hair)
            {
                _humanoid.AddMarking(uid, id, colors, forced: true);
            }
        }

        if (comp.OriginalFacialHairMarkings is { } facial)
        {
            foreach (var (id, colors) in facial)
            {
                _humanoid.AddMarking(uid, id, colors, forced: true);
            }
        }
    }

    private void OnActivate(EntityUid uid, MutationTrichochromaticShiftComponent comp, TrichochromaticShiftActionEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        comp.UsesSinceOriginal = (comp.UsesSinceOriginal + 1) % 4;

        if (comp.UsesSinceOriginal == 3)
        {
            humanoid.MarkingSet.RemoveCategory(MarkingCategories.Hair);
            humanoid.MarkingSet.RemoveCategory(MarkingCategories.FacialHair);

            if (comp.OriginalHairMarkings is { } hair)
            {
                foreach (var (id, colors) in hair)
                {
                    _humanoid.AddMarking(uid, id, colors, forced: true);
                }
            }

            if (comp.OriginalFacialHairMarkings is { } facial)
            {
                foreach (var (id, colors) in facial)
                {
                    _humanoid.AddMarking(uid, id, colors, forced: true);
                }
            }
        }
        else
        {
            var hue = _random.NextFloat(0f, 1f);
            var saturation = _random.NextFloat(0f, 1f);
            var value = _random.NextFloat(0f, 1f);
            var randomColor = Color.FromHsv(new Vector4(hue, saturation, value, 1f));

            if (humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.Hair, out var hairMarkings))
            {
                for (var i = 0; i < hairMarkings.Count; i++)
                {
                    _humanoid.SetMarkingColor(uid, MarkingCategories.Hair, i, new List<Color> { randomColor });
                }
            }

            if (humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.FacialHair, out var facialMarkings))
            {
                for (var i = 0; i < facialMarkings.Count; i++)
                {
                    _humanoid.SetMarkingColor(uid, MarkingCategories.FacialHair, i, new List<Color> { randomColor });
                }
            }
        }
    }
}
