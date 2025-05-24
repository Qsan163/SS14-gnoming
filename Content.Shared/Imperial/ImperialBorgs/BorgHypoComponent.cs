using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.UserInterface;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Network;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.Borgs
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class BorgHypoComponent : Component
    {
        [DataField("Solutions")]
        public List<BorgSolution> Solutions = new List<BorgSolution>();

        public int CurrentIndex = 0;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool UiUpdateNeeded;

        [ViewVariables(VVAccess.ReadWrite)]
        public string CurrentReagentName = "бикаридин";

        [DataField]
        public EntProtoId Action = "ChangeReagent";

        [DataField]
        public EntityUid? ActionEntity;

        /// <summary>
        /// The key used for the ActivatableUI.
        /// </summary>
        public const string UIKey = "borg-hypo";
    }

    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class BorgSolution
    {
        [DataField("reagents")]
        public List<Reagent> Reagents = new();

        public string? GetPrimaryReagentId()
        {
            return Reagents.Count > 0 ? Reagents[0].ReagentId : null;
        }

        public Solution ToChemSolution()
        {
            var solution = new Solution();
            foreach (var reagent in Reagents)
            {
                solution.AddReagent(reagent.ReagentId, reagent.Quantity);
            }
            return solution;
        }
    }

    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class Reagent
    {
        [DataField("ReagentId")]
        public string ReagentId = default!;

        [DataField("Quantity")]
        public float Quantity = 1.0f;

        [DataField("Sprite")]
        public string? Sprite = null;
    }

    [Serializable, NetSerializable]
    public sealed class BorgHypoComponentState : ComponentState
    {
        public readonly bool UiUpdateNeeded;
        public readonly string CurrentReagentName;

        public BorgHypoComponentState(bool uiUpdateNeeded, string currentReagenName)
        {
            UiUpdateNeeded = uiUpdateNeeded;
            CurrentReagentName = currentReagenName;
        }
    }

    public sealed partial class ChangeReagentAction : InstantActionEvent
    {
    }

    [Serializable, NetSerializable]
    public sealed class ChangeReagentEvent : EntityEventArgs
    {
        public string? ReagentId { get; }
        public NetEntity Entity { get; }

        public ChangeReagentEvent(string? reagentId, NetEntity entity)
        {
            ReagentId = reagentId;
            Entity = entity;
        }
    }

    [Serializable, NetSerializable]
    public sealed class OpenBorgHypoUIEvent : EntityEventArgs
    {
        public NetEntity Entity { get; }

        public OpenBorgHypoUIEvent(NetEntity entity)
        {
            Entity = entity;
        }
    }

    [Serializable, NetSerializable]
    public enum BorgHypoUiKey
    {
        Key
    }
}
