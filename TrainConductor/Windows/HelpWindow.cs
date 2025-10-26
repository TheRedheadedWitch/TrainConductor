using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVTrainConductor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Dalamud.Interface.Windowing.Window;


namespace TrainConductor.Windows;

internal class HelpWindow : Window, IDisposable
{
    public HelpWindow() : base("Help") => TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("Support Redheaded Witch on Ko-fi"), Icon = FontAwesomeIcon.Heart, IconOffset = new Vector2(1, 1), Click = _ => Util.OpenLink("https://ko-fi.com/theredheadedwitch") });

    public override unsafe void Draw()
    {
        ImGui.TextWrapped("Train Conductor is developed by The Redheaded Witch.");
        ImGui.TextWrapped("If you find this plugin useful, please consider supporting its development on ");
        ImGui.SameLine();
        if (ImGui.Button("Ko-fi"))
            Util.OpenLink("https://ko-fi.com/theredheadedwitch");
        ImGui.Separator();
        ImGui.TextWrapped("For help and support or to offer suggestions, please visit ");
        ImGui.SameLine();
        if (ImGui.Button("Github"))
            Util.OpenLink("https://github.com/TheRedheadedWitch/TrainConductor");
        ImGui.Separator();
        ImGui.TextWrapped("Train Conductor is open source! I made this to help Train Conductors save a lot of time preparing!");
        ImGui.Separator();
        ImGui.TextWrapped("Train Conductor allows you to create your own custom messages that will make your trains unique for you. Just like in FFXIV, Train Conductor uses placeholders to replace data for you. Here is a list of current placeholders.");
        ImGui.NewLine();
        ImGui.Indent();
        ImGui.TextWrapped("<name> - Your character's name");
        ImGui.TextWrapped("<target> - The selected target mob (if any)");
        ImGui.TextWrapped("<server> - Your character's current server they are in");
        ImGui.TextWrapped("<expansion> - The selected expansion");
        ImGui.TextWrapped("<startingarea> - The selected area you want people to meet to start");
        ImGui.TextWrapped("<instancecharacter> - The instance Symbol in game for the selected instance (won't appear if there is no instance)");
        ImGui.TextWrapped("<instancestring> - The word instance (won't appear if there is no instance)");
        ImGui.TextWrapped("<instancenumber> - The instance number for the selected instance (won't appear if there are no instance)");
        ImGui.TextWrapped("<@> - The Discord role tag for the selected expansion (used in Faloop for starting an A Train)");
        ImGui.TextWrapped("<timediscord> - The time of departure formatted for Discord timestamp (Displays the amount of time until start time in Discord as a countdown)");
        ImGui.TextWrapped("<timecountdown> - The time remaining until departure in minutes");
        ImGui.Unindent();
        ImGui.NewLine();
        ImGui.TextWrapped("You can visually see how the placeholders will look in your message in the preview sections for each option. There is also an Echo chat option you can utilize to see how your messages will look in-game before you use them.");
        ImGui.TextWrapped("Import comes straight from https://scout.wobbuffet.net/, click the Export button at the top right and then click import on the plugin window. This will populate the plugin with the data including instances.");
        ImGui.TextWrapped("If you have any suggestions for additional placeholders or features, please open an issue on the GitHub page. Thank you for using Train Conductor!");
        ImGui.NewLine();
        ImGui.TextWrapped("*Note - placeholders are not case sensitive unlike FFXIV placeholders.");
        ImGui.TextWrapped("*Note - A flag is created for the locations prior to sending, so using the FFXIV <flag> placeholder will display this flag (it won't open your map, but it is placed for you automatically). Remember that <flag> is an FFXIV placeholder so you must use all lowercase to use it properly.");
    }

    public void Dispose() { }
}