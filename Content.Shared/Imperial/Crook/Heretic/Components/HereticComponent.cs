using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Imperial.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticComponent : Component
{
    /// <summary>
    /// Максимальное количество рун, которые можно создать
    /// </summary>
    [DataField]
    public int MaxRunes = 1;
}
