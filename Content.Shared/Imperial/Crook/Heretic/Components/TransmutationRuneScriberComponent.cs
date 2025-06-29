using Robust.Shared.Prototypes;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class TransmutationRuneScriberComponent : Component
{
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(4.905);

    [DataField]
    public EntProtoId RuneDrawingEntity = "HereticRuneRitualDrawAnimationCicatrixEffect";
}
