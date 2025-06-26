using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Localizations;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class RuneScribingComponent : Component
{
    [DataField("animationProto")]
    public string AnimationProto = "HereticRuneRitualDrawAnimationEffect";

    [DataField("scribingDuration", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ScribingDuration = TimeSpan.FromSeconds(13.625);

    [DataField("runeProto")]
    public string RuneProto = "HereticRuneRitual";

    [DataField("soundPath")]
    public string SoundPath = "/Audio/Imperial/Crook/Heretic/castsummon.ogg";

    [DataField("successMessage")]
    public LocId SuccessMessage = "heretic-rune-scribing-success";

    [DataField("maxDistance")]
    public float MaxDistance = 2f;

    [DataField("needHand")]
    public bool NeedHand = true;
}
