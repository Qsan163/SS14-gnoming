using Robust.Shared.GameStates;

namespace Content.Shared.Imperial.Heretic.Components;

[RegisterComponent]
public sealed partial class EldritchInfluenceDrainerComponent : Component
{
    [DataField]
    public TimeSpan TimeModifier = TimeSpan.FromSeconds(0.5);

    [DataField]
    public bool Hidden;
}
