using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TransmutationRuneScriberComponent : Component
{
    [DataField]
    public float Time = 4.905f;

    [DataField]
    public EntProtoId RuneDrawingEntity = "HereticRuneRitualDrawAnimationCicatrixEffect";
}
