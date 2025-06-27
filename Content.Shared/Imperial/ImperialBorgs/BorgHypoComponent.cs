using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Imperial.ImperialBorgs
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
    }

    [DataDefinition]
    [Serializable, NetSerializable]
    public sealed partial class BorgSolution
    {
        [DataField("reagents")]
        public List<ImperialBorgsReagent> Reagents = new();

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
    public sealed partial class ImperialBorgsReagent
    {
        [DataField("ReagentId")]
        public string ReagentId = null!;

        [DataField("Quantity")]
        public float Quantity = 1.0f;

        [DataField("Sprite")]
        public string? Sprite;
    }

    [Serializable, NetSerializable]
    public sealed class BorgHypoComponentState(bool uiUpdateNeeded, string currentReagenName) : ComponentState
    {
        public readonly bool UiUpdateNeeded = uiUpdateNeeded;
        public readonly string CurrentReagentName = currentReagenName;
    }
}
