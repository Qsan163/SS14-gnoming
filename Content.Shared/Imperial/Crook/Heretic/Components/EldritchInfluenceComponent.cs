using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Damage;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class EldritchInfluenceComponent : Component
{
    [DataField]
    public bool Spent = false;

    [DataField]
    public DamageSpecifier RejectionDamage = new()
    {
        DamageDict = new()
        {
            {"Slash", 50f}
        }
    };

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan BaseDrainTime = TimeSpan.FromSeconds(12);

    [DataField]
    public EntProtoId SpawnOnDrain = "EldritchInfluenceIntermediate";

    [DataField]
    public LocId RejectionMessage = "eldritch-influence-rejection";

    [DataField]
    public LocId StartDrainingMessage = "eldritch-influence-start-draining";
}
