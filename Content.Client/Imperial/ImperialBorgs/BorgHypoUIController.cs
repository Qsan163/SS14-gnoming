using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.Borgs;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Network;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Client.GameObjects;
using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameStates;

namespace Content.Client.Imperial.ImperialBorgs;

[UsedImplicitly]
public sealed class BorgHypoUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    private SimpleRadialMenu? _menu;
    private EntityUid? _activeHypo;

    // Кэш для улучшения производительности
    private readonly Dictionary<string, RadialMenuOption> _cachedOptions = new();
    private readonly Dictionary<EntityUid, List<RadialMenuOption>> _entityMenuCache = new();

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("borg.hypo");
        SubscribeNetworkEvent<OpenBorgHypoUIEvent>(OnOpenUI);

        // Предварительное кэширование часто используемых прототипов
        PreloadCommonReagents();
    }

    private void PreloadCommonReagents()
    {
        var commonReagents = new[] { "Bicaridine", "Kelotane", "Dermaline", "Tricordrazine" };
        foreach (var reagentId in commonReagents)
        {
            if (_prototypeManager.TryIndex(reagentId, out ReagentPrototype? proto))
            {
                CacheRadialOption(proto);
            }
        }
    }

    private RadialMenuOption CacheRadialOption(ReagentPrototype proto)
    {
        if (_cachedOptions.TryGetValue(proto.ID, out var cached))
            return cached;

        var option = new RadialMenuActionOption<ReagentPrototype>(HandleRadialButtonClick, proto)
        {
            Sprite = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Misc/beakerlarge.png")),
            ToolTip = Loc.GetString(proto.LocalizedName)
        };

        _cachedOptions[proto.ID] = option;
        return option;
    }

    public void OnStateEntered(GameplayState state)
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenEmotesMenu,
                InputCmdHandler.FromDelegate(_ => ToggleMenu(false)))
            .Register<BorgHypoUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<BorgHypoUIController>();
        _entityMenuCache.Clear();
    }

    private void OnOpenUI(OpenBorgHypoUIEvent ev, EntitySessionEventArgs args)
    {
        var uid = _entityManager.GetEntity(ev.Entity);
        if (!_entityManager.TryGetComponent<BorgHypoComponent>(uid, out var hypo))
            return;

        _activeHypo = uid;
        OpenMenu(uid, hypo);
    }

    private void OpenMenu(EntityUid uid, BorgHypoComponent hypo)
    {
        CloseMenu();

        if (_entityMenuCache.TryGetValue(uid, out var cachedModels))
        {
            _menu = new SimpleRadialMenu();
            _menu.SetButtons(cachedModels);
            _menu.Open();
            _menu.OpenCentered();
            return;
        }

        var models = ConvertToButtons(hypo.Solutions).ToList();
        _entityMenuCache[uid] = models;

        _menu = new SimpleRadialMenu();
        _menu.SetButtons(models);
        _menu.Open();
        _menu.OpenCentered();
    }

    private void ToggleMenu(bool centered)
    {
        if (_menu == null)
        {
            var player = _playerManager.LocalSession?.AttachedEntity;
            if (player == null || !_entityManager.TryGetComponent<BorgHypoComponent>(player.Value, out var hypo))
                return;

            OpenMenu(player.Value, hypo);
        }
        else
        {
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        if (_menu == null)
            return;

        _menu.Close();
        _menu = null;
    }

    private IEnumerable<RadialMenuOption> ConvertToButtons(List<BorgSolution> solutions)
    {
        var models = new List<RadialMenuOption>();

        foreach (var solution in solutions)
        {
            var primaryId = solution.GetPrimaryReagentId();
            if (string.IsNullOrEmpty(primaryId) || !_prototypeManager.TryIndex(primaryId, out ReagentPrototype? proto))
                continue;

            if (proto == null)
                continue;

            var option = CacheRadialOption(proto);
            models.Add(option);
        }

        return models;
    }

    private void HandleRadialButtonClick(ReagentPrototype prototype)
    {
        if (_activeHypo == null || !_entityManager.TryGetComponent<BorgHypoComponent>(_activeHypo.Value, out var hypo))
        {
            _sawmill.Warning($"No active hypo or component when trying to switch to {prototype.ID}");
            return;
        }

        var netEntity = _entityManager.GetNetEntity(_activeHypo.Value);
        _sawmill.Info($"Sending ChangeReagentEvent: ReagentId={prototype.ID}, Entity={netEntity}");

        var ev = new ChangeReagentEvent(prototype.ID, netEntity);
        _entityManager.EventBus.RaiseEvent(EventSource.Local, ev);

        _sawmill.Info($"Event sent");
        CloseMenu();
    }
}
