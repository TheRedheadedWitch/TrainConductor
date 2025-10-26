using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using SRankAssistant;
using System.Reflection;
using TrainConductor.Windows;

namespace FFXIVTrainConductor;

public sealed class TrainConductor : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new("Train Conductor");
    internal static ConductorWindow ConductorWin { get; set; } = null!;
    internal static ConfigurationWindow ConfigurationWin { get; set; } = null!;
    internal static HelpWindow HelpWin { get; set; } = null!;

    public TrainConductor(IDalamudPluginInterface pi)
    {
        pi.Create<SERVICES>();
        ConductorWin = new();
        ConfigurationWin = new();
        HelpWin = new();
        WindowSystem.AddWindow(ConductorWin);
        WindowSystem.AddWindow(ConfigurationWin);
        WindowSystem.AddWindow(HelpWin);
        SERVICES.CommandManager.AddHandler("/conductor", new CommandInfo(OnCommand) { HelpMessage = "Open Conductor Window!" });
        SERVICES.CommandManager.AddHandler("/conduct", new CommandInfo(OnCommand) { HelpMessage = "Open Conductor Window!" });
        SERVICES.Interface.UiBuilder.Draw += DrawUI;
        SERVICES.Interface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        SERVICES.Interface.UiBuilder.OpenMainUi += ToggleMainUI;
        MobDetector.Initialize();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        SERVICES.CommandManager.RemoveHandler("/conductor");
        SERVICES.CommandManager.RemoveHandler("/conduct");
        MobDetector.Dispose();
        ParseData.Dispose();
    }

    private void OnCommand(string command, string args) => ToggleMainUI();
    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigurationWin.Toggle();
    public void ToggleMainUI() => ConductorWin.Toggle();
}