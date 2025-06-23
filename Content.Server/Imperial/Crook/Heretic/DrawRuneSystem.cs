using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Imperial.Heretic.Events;
using Content.Shared.Imperial.Heretic.Components;
using Content.Shared.Tag;
using Content.Shared.Popups;
using Robust.Shared.Maths;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;

namespace Content.Server.Imperial.Heretic.EntitySystems;

public sealed partial class HereticRuneSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DrawRitualRuneDoAfterEvent>(OnRitualDoAfter);
    }

    private void OnAfterInteract(Entity<TagComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.ClickLocation.IsValid(_entMan))
            return;

        if (!args.CanReach || !_entMan.HasComponent<HereticComponent>(args.User))
            return;

        var (animProto, duration) = GetRuneDrawingParameters(ent);

        var animEnt = Spawn(animProto, args.ClickLocation);
        _transform.AttachToGridOrMap(animEnt);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            duration,
            new DrawRitualRuneDoAfterEvent(
                _entMan.GetNetEntity(animEnt),
                new NetCoordinates(
                    _entMan.GetNetEntity(args.ClickLocation.EntityId),
                    args.ClickLocation.Position)),
            ent,
            used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            DistanceThreshold = 2f,
            Broadcast = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            QueueDel(animEnt);
    }

    private (string animProto, TimeSpan duration) GetRuneDrawingParameters(EntityUid ent)
    {
        var animProto = "HereticRuneRitualDrawAnimationEffect";
        var duration = TimeSpan.FromSeconds(13.625f);

        if (_entMan.TryGetComponent<TransmutationRuneScriberComponent>(ent, out var scriber))
        {
            animProto = scriber.RuneDrawingEntity;
            duration = TimeSpan.FromSeconds(scriber.Time);
        }

        return (animProto, duration);
    }

    private void OnRitualDoAfter(DrawRitualRuneDoAfterEvent ev)
    {
        if (_entMan.GetEntity(ev.AnimationEntity) is { } animEnt)
            QueueDel(animEnt);

        if (ev.Cancelled || ev.Handled || !_entMan.TryGetEntity(ev.Coordinates.NetEntity, out var targetEntity))
            return;

        var spawnCoords = new EntityCoordinates(targetEntity.Value, ev.Coordinates.Position);
        var rune = Spawn("HereticRuneRitual", spawnCoords);
        _transform.AttachToGridOrMap(rune);

        var audioParams = AudioParams.Default
            .WithVolume(-5f)
            .WithMaxDistance(10f);
        _audio.PlayPvs("/Audio/Imperial/Crook/Heretic/castsummon.ogg", rune, audioParams);

        _popup.PopupEntity(Loc.GetString("Руна успешно создана"), rune, ev.User);
        ev.Handled = true;
    }
}
