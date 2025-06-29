using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Imperial.Heretic.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Server.Imperial.Heretic.Systems;

public sealed partial class HereticRuneSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RuneScribingComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DrawRitualRuneDoAfterEvent>(OnRitualDoAfter);
    }

    private (EntProtoId animProto, TimeSpan duration) GetRuneDrawingParameters(Entity<RuneScribingComponent> ent, EntityUid? tool)
    {
        var animProto = ent.Comp.AnimationProto;
        var duration = ent.Comp.ScribingDuration;

        if (tool != null && TryComp<TransmutationRuneScriberComponent>(tool.Value, out var scriber))
        {
            animProto = scriber.RuneDrawingEntity;
            duration = scriber.Time;
        }

        return (animProto, duration);
    }


private void OnAfterInteract(Entity<RuneScribingComponent> ent, ref AfterInteractEvent args)
{
    if (!args.ClickLocation.IsValid(EntityManager))
        return;

    if (!args.CanReach || !HasComp<HereticComponent>(args.User))
        return;

    var (animProto, duration) = GetRuneDrawingParameters(ent, args.Used);

    var animEnt = Spawn(animProto, args.ClickLocation);
    _transform.AttachToGridOrMap(animEnt);

    var doAfterArgs = new DoAfterArgs(
        EntityManager,
        args.User,
        duration,
        new DrawRitualRuneDoAfterEvent(
            GetNetEntity(animEnt),
            GetNetCoordinates(args.ClickLocation),
            ent.Comp.RuneProto,
            ent.Comp.SoundPath),
        ent,
        used: args.Used)
    {
        BreakOnDamage = true,
        BreakOnMove = true,
        NeedHand = ent.Comp.NeedHand,
        DistanceThreshold = ent.Comp.MaxDistance,
        Broadcast = true
    };

    if (!_doAfter.TryStartDoAfter(doAfterArgs))
        QueueDel(animEnt);
}

    private void OnRitualDoAfter(DrawRitualRuneDoAfterEvent ev)
    {
        if (GetEntity(ev.AnimationEntity) is { Valid: true } animEnt)
            QueueDel(animEnt);

        if (ev.Cancelled || ev.Handled)
            return;

        var coords = GetCoordinates(ev.Coordinates);

        var rune = Spawn(ev.RuneProto, coords);
        _transform.AttachToGridOrMap(rune);

        var audioParams = AudioParams.Default
            .WithVolume(-5f)
            .WithMaxDistance(10f);
        _audio.PlayPvs(ev.SoundPath, rune, audioParams);

        ev.Handled = true;
    }
}
