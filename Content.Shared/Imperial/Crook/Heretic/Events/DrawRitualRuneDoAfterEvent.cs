using Robust.Shared.Serialization;
using Robust.Shared.GameStates;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

[Serializable, NetSerializable]
public sealed partial class DrawRitualRuneDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity AnimationEntity;

    [DataField]
    public NetCoordinates Coordinates;

    [DataField]
    public EntProtoId RuneProto = "HereticRuneRitual";

    [DataField]
    public ResPath SoundPath = new("/Audio/Imperial/Crook/Heretic/castsummon.ogg");

    private DrawRitualRuneDoAfterEvent()
    {
    }

    public DrawRitualRuneDoAfterEvent(NetEntity animationEntity, NetCoordinates coordinates, EntProtoId runeProto, ResPath soundPath)
    {
        AnimationEntity = animationEntity;
        Coordinates = coordinates;
        RuneProto = runeProto;
        SoundPath = soundPath;
    }

    public override DoAfterEvent Clone() => this;
}
