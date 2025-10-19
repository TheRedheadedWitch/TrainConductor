using System.Diagnostics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;


namespace FFXIVTrainConductor;

public sealed class SERVICES
{
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] public static IBuddyList Buddies { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IFateTable Fates { get; private set; } = null!;
    [PluginService] public static IFlyTextGui FlyText { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IJobGauges Gauges { get; private set; } = null!;
    [PluginService] public static IKeyState KeyState { get; private set; } = null!;
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui PfGui { get; private set; } = null!;
    [PluginService] public static IPartyList Party { get; private set; } = null!;
    [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] public static ITargetManager Targets { get; private set; } = null!;
    [PluginService] public static IToastGui Toasts { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] public static IGameLifecycle GameLifecycle { get; private set; } = null!;
    [PluginService] public static IGamepadState GamepadState { get; private set; } = null!;
    [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider Hook { get; private set; } = null!;
    [PluginService] public static ITextureProvider Texture { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IAetheryteList AetheryteList { get; private set; } = null!;
    [PluginService] public static IAddonEventManager AddonEventManager { get; private set; } = null!;
    [PluginService] public static IGameInventory GameInventory { get; private set; } = null!;
    [PluginService] public static ITextureSubstitutionProvider TextureSubstitution { get; private set; } = null!;
    [PluginService] public static ITitleScreenMenu TitleScreenMenu { get; private set; } = null!;
    [PluginService] public static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] public static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] public static IMarketBoard MarketBoard { get; private set; } = null!;
    [PluginService] public static ISeStringEvaluator SeStringEvaluator { get; private set; } = null!;
}

public static class LOG
{
    [Conditional("DEBUG")] public static void Debug(string message) => SERVICES.Log.Debug(message);
    public static void Info(string message) => SERVICES.Log.Info(message);
    public static void Information(string message) => SERVICES.Log.Info(message);
    public static void Warn(string message) => SERVICES.Log.Warning(message);
    public static void Warn(string message, Exception ex) => SERVICES.Log.Warning(message, ex);
    public static void Warning(string message) => SERVICES.Log.Warning(message);
    public static void Warning(string message, Exception ex) => SERVICES.Log.Warning(message, ex);
    public static void Error(string message) => SERVICES.Log.Error(message);
    public static void Error(string message, Exception ex) => SERVICES.Log.Error(message, ex);
}