using Content.Shared.Imperial.OperationalErtCleaners.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Paper;
using Content.Shared.Roles;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Imperial.OperationalErtCleaners;

public sealed class SolutionDynamicColorOfStampSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SolutionDynamicColorOfStampComponent, SolutionContainerChangedEvent>(OnStampDynamicColor);
    }

    public void OnStampDynamicColor(EntityUid uid, SolutionDynamicColorOfStampComponent comp, ref SolutionContainerChangedEvent args)
    {
        var colorSocution = args.Solution.GetColor(_protoManager);

        /// do not stamp if the mop is reagent-free (default color = white)
        if (args.Solution.Volume == FixedPoint2.Zero)
        {
            RemComp<StampComponent>(uid);
        }
        else
        {
            if (!TryComp<StampComponent>(uid, out var stampComp))
                return;

            if (comp.CheckValidRole)
            {
                if (!TryGetItemOwner(uid, out var user) || user == null)
                    return;

                if (!IsUserDefiniteJob(user.Value, comp))
                {
                    stampComp.StampedName = comp.FalseStampedName;
                }
            }
            stampComp.StampedColor = colorSocution;
        }
    }

    private bool IsUserDefiniteJob(EntityUid user, SolutionDynamicColorOfStampComponent comp)
    {
        if (string.IsNullOrEmpty(comp.RoleName))
            return false;
        if (string.IsNullOrEmpty(comp.FalseStampedName))
            return false;

        if (!_mindSystem.TryGetMind(user, out var mindId, out var mind))
            return false;

        foreach (var role in _roleSystem.MindGetAllRoleInfo(mindId))
        {
            if (role.Name.Contains(comp.RoleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetItemOwner(EntityUid item, [NotNullWhen(true)] out EntityUid? owner)
    {
        owner = null;

        var query = EntityQueryEnumerator<MetaDataComponent, ContainerManagerComponent>();
        while (query.MoveNext(out var uid, out _, out var containerManager))
        {
            foreach (var container in containerManager.Containers.Values)
            {
                if (container.Contains(item))
                {
                    owner = uid;
                    return true;
                }
            }
        }

        return false;
    }
}

