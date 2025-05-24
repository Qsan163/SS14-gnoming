using Content.Server.Chemistry.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Borgs;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Log;
using System.Collections.Generic;
using Content.Shared.Chemistry;

namespace Content.Server.Imperial.ImperialBorgs
{
    public sealed class BorgHypoSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            _sawmill = _logManager.GetSawmill("borg.hypo");

            SubscribeLocalEvent<BorgHypoComponent, GetVerbsEvent<AlternativeVerb>>(AddSwitchVerb);
            SubscribeLocalEvent<BorgHypoComponent, GetItemActionsEvent>(OnGetActions);
            SubscribeLocalEvent<BorgHypoComponent, ChangeReagentAction>(OnReagentAction);
            SubscribeLocalEvent<BorgHypoComponent, ChangeReagentEvent>(OnReagentChange);
        }

        private void OnGetActions(EntityUid uid, BorgHypoComponent component, GetItemActionsEvent args)
        {
            args.AddAction(ref component.ActionEntity, component.Action);
        }

        private void OnReagentAction(EntityUid uid, BorgHypoComponent component, ChangeReagentAction args)
        {
            if (args.Handled)
                return;

            RaiseNetworkEvent(new OpenBorgHypoUIEvent(GetNetEntity(uid)));
            args.Handled = true;
        }

        private void OnReagentChange(EntityUid uid, BorgHypoComponent component, ChangeReagentEvent args)
        {
            _sawmill.Info($"OnReagentChange: ReagentId={args.ReagentId}, Entity={args.Entity}");

            if (args.ReagentId == null)
                return;

            if (_prototypeManager.TryIndex(args.ReagentId, out ReagentPrototype? reagent))
            {
                SwitchReagent(uid, component, reagent);
            }
            else
            {
                _sawmill.Warning($"Failed to get ReagentPrototype for {args.ReagentId}");
            }
        }

        private void AddSwitchVerb(EntityUid uid, BorgHypoComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (component.Solutions.Count <= 1)
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    RaiseNetworkEvent(new OpenBorgHypoUIEvent(GetNetEntity(uid)));
                },
                Text = Loc.GetString("borghypo-switchreagent"),
                Priority = 1
            };
            args.Verbs.Add(verb);
        }

        private void SwitchReagent(EntityUid uid, BorgHypoComponent component, ReagentPrototype? reagent = null)
        {
            _sawmill.Info($"SwitchReagent: uid={uid}, reagent={reagent?.ID}");

            if (!TryComp<SolutionRegenerationComponent>(uid, out var regen) ||
                !_solutionSystem.TryGetSolution(uid, regen.SolutionName, out var solution))
            {
                _sawmill.Warning($"Failed to get solution for {uid}");
                return;
            }

            if (reagent != null)
            {
                var index = component.Solutions.FindIndex(x => x.GetPrimaryReagentId() == reagent.ID);
                if (index == -1)
                {
                    _sawmill.Warning($"Reagent {reagent.ID} not found in solutions");
                    return;
                }

                component.CurrentIndex = index;
            }
            else
            {
                component.CurrentIndex = (component.CurrentIndex + 1) % component.Solutions.Count;
            }

            var newSolution = component.Solutions[component.CurrentIndex];
            var primaryId = newSolution.GetPrimaryReagentId();
            if (primaryId == null)
            {
                _sawmill.Warning("Primary reagent ID is null");
                return;
            }

            if (!_prototypeManager.TryIndex(primaryId, out ReagentPrototype? proto) || proto == null)
            {
                _sawmill.Warning($"Failed to get prototype for {primaryId}");
                return;
            }

            _sawmill.Info($"Switching to solution with primary reagent {primaryId}");

            _solutionSystem.RemoveAllSolution(solution.Value);

            if (_solutionSystem.TryGetSolution(uid, "Generated", out var generated))
            {
                _solutionSystem.RemoveAllSolution(generated.Value);
                var chemSolution = newSolution.ToChemSolution();
                _solutionSystem.TryAddSolution(generated.Value, chemSolution);
            }

            component.CurrentReagentName = proto.LocalizedName;
            component.UiUpdateNeeded = true;
            Dirty(uid, component);
        }
    }
}
