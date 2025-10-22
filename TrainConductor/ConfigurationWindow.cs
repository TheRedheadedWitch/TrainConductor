using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Data.Parsing;
using Lumina.Excel.Sheets;
using System.Numerics;


namespace FFXIVTrainConductor;

internal class ConfigurationWindow : Window, IDisposable
{
    private static List<string> AetheryteNames = SERVICES.Data.GetExcelSheet<Aetheryte>().Where(a => a.PlaceName.Value.RowId != 0).Select(a => a.PlaceName.Value.Name.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList();
    private static Dictionary<string, (string Zone, float X, float Y)> AetheryteMapData = new();
    private static readonly List<string> ServerList = new() { "Aegis", "Adamantoise", "Alexander", "Alpha", "Anima", "Asura", "Atomos", "Bahamut", "Balmung", "Belias", "Behemoth", "Bismarck", "Brynhildr", "Carbuncle", "Cactuar", "Cerberus", "Chocobo", "Coeurl", "Diabolos", "Durandal", "Exodus", "Famfrit", "Fenrir", "Garuda", "Gilgamesh", "Golem", "Gungnir", "Hades", "Halicarnassus", "Hyperion", "Ifrit", "Ixion", "Jenova", "Kraken", "Kujata", "Lamia", "Leviathan", "Louisoix", "Malboro", "Mandragora", "Marilith", "Masamune", "Mateus", "Midgardsormr", "Midnight", "Moogle", "Odin", "Omega", "Pandaemonium", "Phantom", "Phoenix", "Raiden", "Ramuh", "Rafflesia", "Ridill", "Sagittarius", "Sephirot", "Shinryu", "Shiva", "Siren", "Spriggan", "Tonberry", "Tiamat", "Titan", "Twintania", "Ultima", "Ultros", "Unicorn", "Valefor", "Yojimbo", "Zalera", "Zeromus", "Zodiark" };
    private string labelName = string.Empty;
    private string aetheryteFilter = string.Empty;
    private string serverFilter = string.Empty;
    private string boardingLocation = string.Empty;
    private int departureHour;
    private int departureMinute;
    private string customTag = string.Empty;
    private IPlayerCharacter? player = null;
    private bool AnnounceSay { get; set; }
    private bool AnnounceNN { get; set; }
    private bool AnnounceYell { get; set; }
    private bool AnnounceShout { get; set; }
    private bool AnnounceParty { get; set; }
    private bool AnnounceLS1 { get; set; }
    private bool AnnounceLS2 { get; set; }
    private bool AnnounceLS3 { get; set; }
    private bool AnnounceLS4 { get; set; }
    private bool AnnounceLS5 { get; set; }
    private bool AnnounceLS6 { get; set; }
    private bool AnnounceLS7 { get; set; }
    private bool AnnounceLS8 { get; set; }
    private bool AnnounceCWLS1 { get; set; }
    private bool AnnounceCWLS2 { get; set; }
    private bool AnnounceCWLS3 { get; set; }
    private bool AnnounceCWLS4 { get; set; }
    private bool AnnounceCWLS5 { get; set; }
    private bool AnnounceCWLS6 { get; set; }
    private bool AnnounceCWLS7 { get; set; }
    private bool AnnounceCWLS8 { get; set; }
    private uint Instance = 0;

    public ConfigurationWindow() : base("Train Conductor Configuration")
    {
        TitleBarButtons.Add(new()
        {
            ShowTooltip = () => ImGui.SetTooltip("Support Redheaded Witch on Ko-fi"),
            Icon = FontAwesomeIcon.Heart,
            IconOffset = new Vector2(1, 1),
            Click = _ => Util.OpenLink("https://ko-fi.com/theredheadedwitch")
        });

        Size = new Vector2(450, 400);
        SizeCondition = ImGuiCond.FirstUseEver;

        DateTime initialTime = DateTime.Now.AddMinutes(30);
        departureHour = initialTime.Hour;
        departureMinute = initialTime.Minute;

        SERVICES.Framework.RunOnTick(() =>
        {
            try { player = SERVICES.ClientState.LocalPlayer; }
            catch (Exception ex) { LOG.Error("player error", ex); }
        }).Wait();

        IEnumerable<Aetheryte> aetherytes = SERVICES.Data.GetExcelSheet<Aetheryte>().Where(a => a.PlaceName.Value.RowId != 0 && a.Territory.RowId != 0);

        foreach (Aetheryte a in aetherytes)
        {
            string aetheryteName = a.PlaceName.Value.Name.ToString();
            if (string.IsNullOrEmpty(aetheryteName) || AetheryteMapData.ContainsKey(aetheryteName))
                continue;
            TerritoryType territory = SERVICES.Data.GetExcelSheet<TerritoryType>().GetRow(a.Territory.RowId);
            if (territory.RowId == 0)
                continue;
            string zone = territory.PlaceName.Value.Name.ToString() ?? string.Empty;
            float worldX = 0f;
            float worldY = 0f;
            bool foundCoords = false;
            Level? level = a.Level[0].ValueNullable;
            if (level != null)
            {
                worldX = level.Value.X;
                worldY = level.Value.Z;
                foundCoords = true;
            }
            if (!foundCoords)
            {
                Lumina.Excel.SubrowExcelSheet<MapMarker> mapMarkerSheet = SERVICES.Data.GetSubrowExcelSheet<MapMarker>();
                if (mapMarkerSheet != null)
                {
                    MapMarker? marker = null;
                    foreach (Lumina.Excel.SubrowCollection<MapMarker> subrowCollection in mapMarkerSheet)
                    {
                        foreach (MapMarker m in subrowCollection)
                            if (m.DataType == 3 && m.DataKey.RowId == a.RowId)
                            {
                                marker = m;
                                break;
                            }
                        if (marker != null) break;
                    }
                    if (marker == null && a.AethernetName.RowId != 0)
                        foreach (Lumina.Excel.SubrowCollection<MapMarker> subrowCollection in mapMarkerSheet)
                        {
                            foreach (MapMarker m in subrowCollection)
                                if (m.DataType == 4 && m.DataKey.RowId == a.AethernetName.RowId)
                                {
                                    marker = m;
                                    break;
                                }
                            if (marker != null) break;
                        }
                    if (marker != null)
                    {
                        Map map = SERVICES.Data.GetExcelSheet<Map>().GetRow(a.Map.RowId);
                        if (map.RowId != 0)
                        {
                            float scale = map.SizeFactor / 100f;
                            worldX = PixelCoordToWorldCoord(marker.Value.X, scale, map.OffsetX);
                            worldY = PixelCoordToWorldCoord(marker.Value.Y, scale, map.OffsetY);
                            foundCoords = true;
                        }
                    }
                }
            }
            if (foundCoords)
                AetheryteMapData[aetheryteName] = (zone, worldX, worldY);
            else
                LOG.Warning($"Could not find coordinates for {aetheryteName}");
        }
    }

    private float PixelCoordToWorldCoord(int coord, float scale, short offset)
    {
        const float factor = 2048.0f / (50 * 41);
        return (coord * factor - 1024f) / scale - offset * 0.001f;
    }

    public override void Draw()
    {
        string selectedExpansion = ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex];
        ImGui.Text("Expansion");
        ImGui.SameLine();
        ImGui.SetCursorPosX(200);
        ImGui.PushItemWidth(250f);
        if (ImGui.BeginCombo("##expansionCombo", selectedExpansion))
        {
            for (int i = 0; i < ConductorWindow.Patches.Length; i++)
            {
                bool selected = i == ConductorWindow.storedData.selectedExpansionIndex;
                if (ImGui.Selectable(ConductorWindow.Patches[i], selected))
                    ConductorWindow.storedData.selectedExpansionIndex = i;
                if (selected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
        ImGui.Text("Gathering Location: ");
        ImGui.SameLine();
        ImGui.SetCursorPosX(200);
        ImGui.PushItemWidth(250f);
        DrawAetheryteDropdown();
        ImGui.PopItemWidth();

        ImGui.RadioButton("None", ref Instance, 0u); ImGui.SameLine();
        ImGui.RadioButton("Instance 1", ref Instance, 1u); ImGui.SameLine();
        ImGui.RadioButton("Instance 2", ref Instance, 2u); ImGui.SameLine();
        ImGui.RadioButton("Instance 3", ref Instance, 3u); ImGui.SameLine();
        ImGui.RadioButton("Instance 4", ref Instance, 4u); ImGui.SameLine();
        ImGui.RadioButton("Instance 5", ref Instance, 5u);

        ImGui.Text("Departure Time (HH:MM)");
        ImGui.SameLine();
        ImGui.SetCursorPosX(200);
        ImGui.PushItemWidth(60f);
        string hourStr = departureHour.ToString();
        if (ImGui.InputText("##hour", ref hourStr, 2, ImGuiInputTextFlags.CharsDecimal))
        {
            if (!int.TryParse(hourStr, out int h)) h = 0;
            h = Math.Clamp(h, 0, 23);
            departureHour = h;
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text(":");
        ImGui.SameLine();
        ImGui.PushItemWidth(60f);
        string minuteStr = departureMinute.ToString("00");
        if (ImGui.InputText("##minute", ref minuteStr, 2, ImGuiInputTextFlags.CharsDecimal))
        {
            if (!int.TryParse(minuteStr, out int m)) m = 0;
            m = Math.Clamp(m, 0, 59);
            departureMinute = m;
        }
        ImGui.PopItemWidth();
        ImGui.Separator();
        if (ImGui.BeginTable("Outputs", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableNextColumn();
            ImGui.PushItemWidth(-1);
            string discordText = ConductorWindow.storedData.Discord;
            if (ImGui.InputTextMultiline("##discordInput", ref discordText, 4096, new Vector2(-1, 200)))
            {
                ConductorWindow.storedData.Discord = discordText;
                ConductorWindow.storedData.Save();
            }
            ImGui.Separator();
            string parsedOutput = Parse();
            ImGui.Text("Discord Ready Output");
            ImGui.SameLine();
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Copy))
                ImGui.SetClipboardText(parsedOutput);
            ImGui.InputTextMultiline("##parsedOutput", ref parsedOutput, 4096, new Vector2(-1, 200), ImGuiInputTextFlags.ReadOnly);
            ImGui.TableNextColumn();
            bool nn = ConductorWindow.storedData.AnnounceNN;
            bool yell = ConductorWindow.storedData.AnnounceYell;
            bool shout = ConductorWindow.storedData.AnnounceShout;
            bool party = ConductorWindow.storedData.AnnounceParty;
            bool ls1 = ConductorWindow.storedData.AnnounceLS1;
            bool ls2 = ConductorWindow.storedData.AnnounceLS2;
            bool ls3 = ConductorWindow.storedData.AnnounceLS3;
            bool ls4 = ConductorWindow.storedData.AnnounceLS4;
            bool ls5 = ConductorWindow.storedData.AnnounceLS5;
            bool ls6 = ConductorWindow.storedData.AnnounceLS6;
            bool ls7 = ConductorWindow.storedData.AnnounceLS7;
            bool ls8 = ConductorWindow.storedData.AnnounceLS8;
            bool cwls1 = ConductorWindow.storedData.AnnounceCWLS1;
            bool cwls2 = ConductorWindow.storedData.AnnounceCWLS2;
            bool cwls3 = ConductorWindow.storedData.AnnounceCWLS3;
            bool cwls4 = ConductorWindow.storedData.AnnounceCWLS4;
            bool cwls5 = ConductorWindow.storedData.AnnounceCWLS5;
            bool cwls6 = ConductorWindow.storedData.AnnounceCWLS6;
            bool cwls7 = ConductorWindow.storedData.AnnounceCWLS7;
            bool cwls8 = ConductorWindow.storedData.AnnounceCWLS8;
            bool changed = false;
            ImGui.PushItemWidth(-1);
            string CustomAnnounceMessage = ConductorWindow.storedData.CustomAnnounceMessage;
            ImGui.Text("Announcement Message");
            if (ImGui.InputTextMultiline("##CustomAnnounceMessage", ref CustomAnnounceMessage, 400, new Vector2(-1, 50)))
            {
                ConductorWindow.storedData.CustomAnnounceMessage = CustomAnnounceMessage;
                ConductorWindow.storedData.Save();
            }
            ImGui.PopItemWidth();
            ImGui.Separator();
            ImGui.Text("Chat Channels:");
            float orig = ImGui.GetCursorPosX();
            changed |= ImGui.Checkbox("Yell", ref yell); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 80);
            changed |= ImGui.Checkbox("Shout", ref shout); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 160);
            changed |= ImGui.Checkbox("NN", ref nn); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 240);
            changed |= ImGui.Checkbox("Party", ref party);
            ImGui.Separator();
            ImGui.Text("Linkshells:");
            changed |= ImGui.Checkbox("LS1", ref ls1); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 80);
            changed |= ImGui.Checkbox("LS2", ref ls2); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 160);
            changed |= ImGui.Checkbox("LS3", ref ls3); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 240);
            changed |= ImGui.Checkbox("LS4", ref ls4);
            changed |= ImGui.Checkbox("LS5", ref ls5); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 80);
            changed |= ImGui.Checkbox("LS6", ref ls6); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 160);
            changed |= ImGui.Checkbox("LS7", ref ls7); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 240);
            changed |= ImGui.Checkbox("LS8", ref ls8);
            ImGui.Separator();
            ImGui.Text("Cross-World Linkshells:");
            changed |= ImGui.Checkbox("CWLS1", ref cwls1); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 80);
            changed |= ImGui.Checkbox("CWLS2", ref cwls2); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 160);
            changed |= ImGui.Checkbox("CWLS3", ref cwls3); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 240);
            changed |= ImGui.Checkbox("CWLS4", ref cwls4);
            changed |= ImGui.Checkbox("CWLS5", ref cwls5); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 80);
            changed |= ImGui.Checkbox("CWLS6", ref cwls6); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 160);
            changed |= ImGui.Checkbox("CWLS7", ref cwls7); ImGui.SameLine(); ImGui.SetCursorPosX(orig + 240);
            changed |= ImGui.Checkbox("CWLS8", ref cwls8);
            if (changed)
            {
                ConductorWindow.storedData.AnnounceNN = nn;
                ConductorWindow.storedData.AnnounceYell = yell;
                ConductorWindow.storedData.AnnounceShout = shout;
                ConductorWindow.storedData.AnnounceParty = party;
                ConductorWindow.storedData.AnnounceLS1 = ls1;
                ConductorWindow.storedData.AnnounceLS2 = ls2;
                ConductorWindow.storedData.AnnounceLS3 = ls3;
                ConductorWindow.storedData.AnnounceLS4 = ls4;
                ConductorWindow.storedData.AnnounceLS5 = ls5;
                ConductorWindow.storedData.AnnounceLS6 = ls6;
                ConductorWindow.storedData.AnnounceLS7 = ls7;
                ConductorWindow.storedData.AnnounceLS8 = ls8;
                ConductorWindow.storedData.AnnounceCWLS1 = cwls1;
                ConductorWindow.storedData.AnnounceCWLS2 = cwls2;
                ConductorWindow.storedData.AnnounceCWLS3 = cwls3;
                ConductorWindow.storedData.AnnounceCWLS4 = cwls4;
                ConductorWindow.storedData.AnnounceCWLS5 = cwls5;
                ConductorWindow.storedData.AnnounceCWLS6 = cwls6;
                ConductorWindow.storedData.AnnounceCWLS7 = cwls7;
                ConductorWindow.storedData.AnnounceCWLS8 = cwls8;
                ConductorWindow.storedData.Save();
            }
            ImGui.Separator();
            string parsedInGameOutput = ParseAnnouncementmessage().TextValue;
            ImGui.Text("In-Game Chat Preview");
            ImGui.InputTextMultiline("##parsedInGameOutput", ref parsedInGameOutput, 400, new Vector2(-1, 50), ImGuiInputTextFlags.ReadOnly);
            if (ImGui.Button("Send Chat Announcement", new Vector2(-1, 0)))
            {
                SendChat();
            }
            ImGui.EndTable();
        }
    }

    private unsafe void SendChat()
    {
        if (AetheryteMapData.TryGetValue(AetheryteNames[ConductorWindow.storedData.selectedAetheryteIndex], out (string Zone, float X, float Y) aethData))
        {
            Aetheryte aeth = SERVICES.Data.GetExcelSheet<Aetheryte>().FirstOrDefault(a => a.PlaceName.Value.Name.ToString() == AetheryteNames[ConductorWindow.storedData.selectedAetheryteIndex]);
            TerritoryType territory = SERVICES.Data.GetExcelSheet<TerritoryType>().GetRow(aeth.Territory.RowId);
            Map map = SERVICES.Data.GetExcelSheet<Map>().GetRow(aeth.Map.RowId);
            float mapX = ((41.0f / (map.SizeFactor / 100f)) * ((aethData.X + 1024.0f) / 2048.0f)) + 1.0f;
            float mapY = ((41.0f / (map.SizeFactor / 100f)) * ((aethData.Y + 1024.0f) / 2048.0f)) + 1.0f;
            LOG.Debug($"Map coords: {mapX}, {mapY}");
            MapLinks.SetFlag(territory.RowId, map.RowId, mapX, mapY);
        }
        SeString msg = ParseAnnouncementmessage();
        (bool Enabled, string Prefix)[] chatTargets = new[] {(ConductorWindow.storedData.AnnounceYell, "/y "), (ConductorWindow.storedData.AnnounceShout, "/sh "), (ConductorWindow.storedData.AnnounceNN, "/b "), (ConductorWindow.storedData.AnnounceParty, "/p "), (ConductorWindow.storedData.AnnounceLS1, "/l1 "), (ConductorWindow.storedData.AnnounceLS2, "/l2 "), (ConductorWindow.storedData.AnnounceLS3, "/l3 "), (ConductorWindow.storedData.AnnounceLS4, "/l4 "), (ConductorWindow.storedData.AnnounceLS5, "/l5 "), (ConductorWindow.storedData.AnnounceLS6, "/l6 "), (ConductorWindow.storedData.AnnounceLS7, "/l7 "), (ConductorWindow.storedData.AnnounceLS8, "/l8 "), (ConductorWindow.storedData.AnnounceCWLS1, "/cwl1 "), (ConductorWindow.storedData.AnnounceCWLS2, "/cwl2 "), (ConductorWindow.storedData.AnnounceCWLS3, "/cwl3 "), (ConductorWindow.storedData.AnnounceCWLS4, "/cwl4 "), (ConductorWindow.storedData.AnnounceCWLS5, "/cwl5 "), (ConductorWindow.storedData.AnnounceCWLS6, "/cwl6 "), (ConductorWindow.storedData.AnnounceCWLS7, "/cwl7 "), (ConductorWindow.storedData.AnnounceCWLS8, "/cwl8 ")};
        foreach ((bool enabled, string prefix) in chatTargets)
        {
            if (!enabled) continue;
            SeString chatMessage = new SeStringBuilder().AddText(prefix).Append(msg).Build();
            using Utf8String utf8Message = new(chatMessage.Encode());
            UIModule.Instance()->ProcessChatBoxEntry(&utf8Message);
        }
    }

    private string Parse() => ConductorWindow.storedData.Discord
        .Replace("<name>", player!.Name.ToString())
        .Replace("<server>", player!.CurrentWorld.Value.Name.ToString())
        .Replace("<expansion>", ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex])
        .Replace("<location>", AetheryteNames.Count > 0 ? AetheryteNames[ConductorWindow.storedData.selectedAetheryteIndex] : string.Empty)
        .Replace("<@>", GetExpansionTag(ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex]))
        .Replace("<time>", $"<t:{new DateTimeOffset((DateTime.Now.TimeOfDay.TotalMinutes > departureHour * 60 + departureMinute ? DateTime.Now.Date.AddDays(1).AddHours(departureHour).AddMinutes(departureMinute) : DateTime.Now.Date.AddHours(departureHour).AddMinutes(departureMinute))).ToUnixTimeSeconds()}:R>");

    private SeString ParseAnnouncementmessage()
    {
        TimeSpan timeUntilDeparture = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, departureHour, departureMinute, 0, DateTimeKind.Local) < DateTime.Now ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, departureHour, departureMinute, 0, DateTimeKind.Local).AddDays(1) - DateTime.Now : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, departureHour, departureMinute, 0, DateTimeKind.Local) - DateTime.Now;
        string processedMessage = ConductorWindow.storedData.CustomAnnounceMessage.Replace("<name>", player != null ? player.Name.ToString() : string.Empty).Replace("<server>", player!.CurrentWorld.Value.Name.ToString()).Replace("<expansion>", ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex]).Replace("<time>", $"{(int)Math.Round(timeUntilDeparture.TotalMinutes)} minute{((int)Math.Round(timeUntilDeparture.TotalMinutes) != 1 ? "s" : string.Empty)}");
        if (processedMessage.Contains("<location>"))
            processedMessage = processedMessage.Replace("<location>", "<flag>");
        return new SeStringBuilder().AddText(processedMessage).Build();
    }

    private string GetExpansionTag(string expansion)
    {
        return expansion switch
        {
            "Endwalker" => "@6.0A📢",
            "Shadowbringers" => "@5.0A📢",
            "ARR" or "Heavensward" or "Stormblood" => "@OldA📢",
            _ => "@7.0A📢"
        };
    }

    private void DrawAetheryteDropdown()
    {
        if (AetheryteNames.Count == 0) { ImGui.TextDisabled("No Aetherytes available."); return; }
        string selectedName = AetheryteNames[ConductorWindow.storedData.selectedAetheryteIndex];
        ImGui.PushItemWidth(250f);
        if (ImGui.BeginCombo("##aethcombo", selectedName, ImGuiComboFlags.HeightLarge))
        {
            ImGui.PushID("filter");
            ImGui.InputTextWithHint("##aetheryteFilter", "Search...", ref aetheryteFilter, 128);
            ImGui.PopID();
            string filterLower = aetheryteFilter.ToLower();
            List<string> filteredNames = AetheryteNames.Where(name => string.IsNullOrEmpty(filterLower) || name.ToLower().Contains(filterLower)).ToList();
            foreach (string name in filteredNames)
            {
                bool selected = name == selectedName;
                if (ImGui.Selectable(name, selected))
                {
                    ConductorWindow.storedData.selectedAetheryteIndex = AetheryteNames.IndexOf(name);
                    ConductorWindow.storedData.Save();
                    aetheryteFilter = string.Empty;
                    ImGui.CloseCurrentPopup();
                }
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();
    }

    public void Dispose() { }
}