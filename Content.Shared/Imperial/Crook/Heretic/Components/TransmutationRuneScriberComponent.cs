using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class TransmutationRuneScriberComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Time = TimeSpan.FromSeconds(4.905);

    [DataField]
    public EntProtoId RuneDrawingEntity = "HereticRuneRitualDrawAnimationCicatrixEffect";
}
