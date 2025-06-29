using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class RuneScribingComponent : Component
{
    [DataField]
    public EntProtoId AnimationProto = "HereticRuneRitualDrawAnimationEffect";

    [DataField]
    public TimeSpan ScribingDuration = TimeSpan.FromSeconds(13.625);

    [DataField]
    public EntProtoId RuneProto = "HereticRuneRitual";

    [DataField]
    public ResPath SoundPath = new("/Audio/Imperial/Crook/Heretic/castsummon.ogg");

    [DataField]
    public float MaxDistance = 2f;

    [DataField]
    public bool NeedHand = true;
}
