using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

[Serializable, NetSerializable]
public sealed partial class DrawRitualRuneDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity AnimationEntity;

    [DataField]
    public NetCoordinates Coordinates;

    [DataField]
    public string RuneProto = "HereticRuneRitual";

    [DataField]
    public string SoundPath = "/Audio/Imperial/Crook/Heretic/castsummon.ogg";

    private DrawRitualRuneDoAfterEvent()
    {
    }

    public DrawRitualRuneDoAfterEvent(NetEntity animationEntity, NetCoordinates coordinates)
    {
        AnimationEntity = animationEntity;
        Coordinates = coordinates;
    }

    public override DoAfterEvent Clone() => this;
}
