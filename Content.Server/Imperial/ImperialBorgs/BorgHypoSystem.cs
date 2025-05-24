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
            SubscribeNetworkEvent<ChangeReagentEvent>(OnReagentChange);
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

        private void OnReagentChange(ChangeReagentEvent msg)
        {
            var uid = GetEntity(msg.Entity);
            if (!TryComp<BorgHypoComponent>(uid, out var component))
            {
                _sawmill.Warning($"Failed to get BorgHypoComponent for entity {uid}");
                return;
            }

            _sawmill.Info($"OnReagentChange: ReagentId={msg.ReagentId}, Entity={msg.Entity}");

            if (msg.ReagentId == null)
                return;

            if (_prototypeManager.TryIndex(msg.ReagentId, out ReagentPrototype? reagent))
            {
                SwitchReagent(uid, component, reagent);
            }
            else
            {
                _sawmill.Warning($"Failed to get ReagentPrototype for {msg.ReagentId}");
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

            if (!TryComp<SolutionRegenerationComponent>(uid, out var solutionRegenerationComponent))
            {
                return;
            }

            if (!_solutionSystem.TryGetSolution(uid, solutionRegenerationComponent.SolutionName, out var solution))
            {
                _sawmill.Warning($"Failed to get solution {solutionRegenerationComponent.SolutionName} for {uid}");
                return;
            }

            _sawmill.Info($"Found solution {solutionRegenerationComponent.SolutionName} for {uid}");

            if (reagent != null)
            {
                var index = component.Solutions.FindIndex(x => x.GetPrimaryReagentId() == reagent.ID);
                if (index == -1)
                {
                    _sawmill.Warning($"Reagent {reagent.ID} not found in solutions");
                    return;
                }

                _sawmill.Info($"Found reagent at index {index}");
                component.CurrentIndex = index;
            }
            else
            {
                component.CurrentIndex = (component.CurrentIndex + 1) % component.Solutions.Count;
                _sawmill.Info($"Cycling to next reagent at index {component.CurrentIndex}");
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

            // Очищаем текущий раствор
            _sawmill.Info($"Clearing current solution");
            solution.Value.Comp.Solution.RemoveAllSolution();

            var generated = solutionRegenerationComponent.Generated;
            var chemSolution = newSolution.ToChemSolution();

            generated.RemoveAllSolution();
            foreach (var reagentQuantity in newSolution.Reagents)
            {
                generated.AddReagent(reagentQuantity.ReagentId, reagentQuantity.Quantity);
            }

            component.CurrentReagentName = proto.LocalizedName;
            component.UiUpdateNeeded = true;
            Dirty(uid, component);
        }
    }
}
