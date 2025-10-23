using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using Serilog;
using System.Numerics;


namespace FFXIVTrainConductor;

internal class ConductorWindow : Window, IDisposable
{
    internal static StoredData storedData = StoredData.Load();
    private string newMob = string.Empty, newZone = string.Empty, newX = string.Empty, newY = string.Empty, newInstance = string.Empty;
    internal static readonly string[] Patches = { "ARR", "Heavensward", "Stormblood", "Shadowbringers", "Endwalker", "Dawntrail" };
    private static readonly Dictionary<string, string> ZoneToPatch = new(StringComparer.OrdinalIgnoreCase) { { "Middle La Noscea", "ARR" }, { "Lower La Noscea", "ARR" }, { "Eastern La Noscea", "ARR" }, { "Western La Noscea", "ARR" }, { "Upper La Noscea", "ARR" }, { "Outer La Noscea", "ARR" }, { "Central Shroud", "ARR" }, { "East Shroud", "ARR" }, { "South Shroud", "ARR" }, { "North Shroud", "ARR" }, { "Western Thanalan", "ARR" }, { "Central Thanalan", "ARR" }, { "Eastern Thanalan", "ARR" }, { "Southern Thanalan", "ARR" }, { "Northern Thanalan", "ARR" }, { "Coerthas Central Highlands", "ARR" }, { "Mor Dhona", "ARR" }, { "Coerthas Western Highlands", "Heavensward" }, { "The Sea of Clouds", "Heavensward" }, { "Azys Lla", "Heavensward" }, { "The Dravanian Forelands", "Heavensward" }, { "The Dravanian Hinterlands", "Heavensward" }, { "The Churning Mists", "Heavensward" }, { "The Fringes", "Stormblood" }, { "The Peaks", "Stormblood" }, { "The Lochs", "Stormblood" }, { "The Ruby Sea", "Stormblood" }, { "The Azim Steppe", "Stormblood" }, { "Yanxia", "Stormblood" }, { "Lakeland", "Shadowbringers" }, { "Kholusia", "Shadowbringers" }, { "Amh Araeng", "Shadowbringers" }, { "Il Mheg", "Shadowbringers" }, { "The Rak'tika Greatwood", "Shadowbringers" }, { "The Tempest", "Shadowbringers" }, { "Labyrinthos", "Endwalker" }, { "Thavnair", "Endwalker" }, { "Garlemald", "Endwalker" }, { "Mare Lamentorum", "Endwalker" }, { "Ultima Thule", "Endwalker" }, { "Elpis", "Endwalker" }, { "Urqopacha", "Dawntrail" }, { "Kozama'uka", "Dawntrail" }, { "Yak T'el", "Dawntrail" }, { "Shaaloani", "Dawntrail" }, { "Heritage Found", "Dawntrail" }, { "Living Memory", "Dawntrail" } };
    private static List<MobLocation> _cachedPatchMobs = new();
    private static string _currentCachedPatch = string.Empty;

    public ConductorWindow() : base("Train Conductor")
    {
        TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("Support Redheaded Witch on Ko-fi"), Icon = FontAwesomeIcon.Heart, IconOffset = new Vector2(1, 1), Click = _ => Util.OpenLink("https://ko-fi.com/theredheadedwitch") });
        TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("Open Settings"), Icon = FontAwesomeIcon.Cog, IconOffset = new Vector2(1, 1), Click = _ => TrainConductor.ConfigurationWin.IsOpen = true });
    }

    public override unsafe void Draw()
    {
        if (string.IsNullOrWhiteSpace(storedData.SelectedPatch)) storedData.SelectedPatch = "Dawntrail";
        ImGui.Columns(2, "split", true);
        ImGui.BeginChild("left_panel", new Vector2(-1, -1), false);
        string chatMessage = storedData.ChatMessage;
        if (ImGui.InputTextMultiline("##chatmsg", ref chatMessage, 1024, new Vector2(-1, 80)))
        {
            storedData.ChatMessage = chatMessage;
            storedData.Save();
        }
        ImGui.Separator(); ImGui.Text("Send To");
        bool yell = storedData.Yell, shout = storedData.Shout, echo = storedData.Echo, party = storedData.Party, say = storedData.Say;
        bool changed = false;
        changed |= ImGui.Checkbox("Yell", ref yell); ImGui.SameLine();
        changed |= ImGui.Checkbox("Shout", ref shout); ImGui.SameLine();
        changed |= ImGui.Checkbox("Echo", ref echo); ImGui.SameLine();
        changed |= ImGui.Checkbox("Party", ref party); ImGui.SameLine();
        changed |= ImGui.Checkbox("Say", ref say);
        if (changed)
        {
            storedData.Yell = yell;
            storedData.Shout = shout;
            storedData.Echo = echo;
            storedData.Party = party;
            storedData.Say = say;
            storedData.Save();
        }
        bool canSend = !string.IsNullOrWhiteSpace(storedData.ChatMessage);
        if (!canSend) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
        if (ImGui.Button("Send", new Vector2(-1, 28))) SendChat();
        if (!canSend) ImGui.PopStyleVar();
        ImGui.Separator();
        ImGui.Text("Add New Mob");
        ImGui.InputText("Mob", ref newMob, 64);
        ImGui.InputText("Zone", ref newZone, 64);
        ImGui.InputText("X", ref newX, 8);
        ImGui.InputText("Y", ref newY, 8);
        ImGui.InputText("Instance", ref newInstance, 16);
        if (ImGui.Button("Add", new Vector2(-1, 28)) && float.TryParse(newX, out float x) && float.TryParse(newY, out float y) && !string.IsNullOrWhiteSpace(newMob) && !string.IsNullOrWhiteSpace(newZone)) 
        {
            string zone = CleanZone(newZone);
            string patch = DeterminePatchForZone(zone);
            storedData.Mobs.Add(new() { Mob = CapitalizeEachWord(newMob.Trim()), Zone = zone, X = x, Y = y, Instance = ParseInstanceId(newInstance.Trim()), Patch = patch });
            newMob = newZone = newX = newY = newInstance = string.Empty; storedData.Save();
        }
        ImGui.EndChild();
        ImGui.NextColumn();
        ImGui.Text("Patch");
        ImGui.SameLine();
        if (ImGui.BeginCombo("##patchcombo", storedData.SelectedPatch))
        {
            foreach (string patch in Patches)
            {
                bool selected = patch.Equals(storedData.SelectedPatch, StringComparison.OrdinalIgnoreCase); 
                if (ImGui.Selectable(patch, selected)) storedData.SelectedPatch = patch;
                if (selected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo(); 
        }
        ImGui.Separator();
        Vector2 colAvail = ImGui.GetContentRegionAvail();
        ImGui.BeginChild("right_panel", new Vector2(-1, colAvail.Y - 50), true);
        List<MobLocation> displayList = storedData.Mobs.Where(m => m.Patch.Equals(storedData.SelectedPatch, StringComparison.OrdinalIgnoreCase)).ToList(); 
        if (ImGui.BeginTable("MobTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("Mob Info", ImGuiTableColumnFlags.WidthStretch); 
            ImGui.TableSetupColumn("Buttons", ImGuiTableColumnFlags.WidthFixed, 100f);
            string payloadType = "MOBLOC_DRAG";
            int moveSrc = -1, moveDst = -1;
            for (int i = 0; i < displayList.Count; i++)
            {
                MobLocation loc = displayList[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.BeginTable($"inner_{i}", 4, ImGuiTableFlags.SizingStretchProp))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Zone", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed, 40f);
                    ImGui.TableSetupColumn("Y", ImGuiTableColumnFlags.WidthFixed, 40f);
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    Vector2 rowStart = ImGui.GetCursorScreenPos();
                    float rowHeight = ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().CellPadding.Y * 2;
                    float rowWidth = ImGui.GetContentRegionAvail().X;
                    ImGui.InvisibleButton($"row_{i}", new Vector2(rowWidth, rowHeight));
                    bool itemHeld = ImGui.IsItemActive();
                    if (itemHeld) ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.TableRowBgAlt));
                    if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) 
                    {
                        byte[] src = BitConverter.GetBytes(i); 
                        ImGui.SetDragDropPayload(payloadType, new ReadOnlySpan<byte>(src));
                        ImGui.Text($"Moving {loc.Mob}"); ImGui.EndDragDropSource();
                    }
                    if (ImGui.BeginDragDropTarget())
                    {
                        ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload(payloadType);
                        if (payload.Data != null)
                        { 
                            int sourceIdx = BitConverter.ToInt32(new ReadOnlySpan<byte>(payload.Data, sizeof(int)));
                            if (sourceIdx != i && sourceIdx >= 0 && sourceIdx < displayList.Count) { moveSrc = sourceIdx; moveDst = i;
                            }
                        }
                        ImGui.EndDragDropTarget();
                    }
                    ImGui.SetCursorScreenPos(rowStart);
                    ImGui.Text(loc.Mob);
                    ImGui.TableNextColumn();
                    ImGui.Text(loc.Instance == 0 ? loc.Zone : $"{loc.Zone} {GetInstanceMarker(loc.Instance)}");
                    ImGui.TableNextColumn();
                    ImGui.Text(loc.X.ToString("0.0"));
                    ImGui.TableNextColumn();
                    ImGui.Text(loc.Y.ToString("0.0"));
                    ImGui.EndTable();
                } 
                ImGui.TableNextColumn();
                bool upEnabled = i != 0;
                bool downEnabled = i != displayList.Count - 1;
                if (!upEnabled)
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                if (ImGui.Button($"▲##up{i}")) 
                    if (upEnabled) MoveMobByDirection(displayList[i], -1);
                if (!upEnabled) ImGui.PopStyleVar();
                ImGui.SameLine(); 
                if (!downEnabled)
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                if (ImGui.Button($"▼##down{i}"))
                    if (downEnabled) MoveMobByDirection(displayList[i], 1);
                if (!downEnabled) ImGui.PopStyleVar();
                ImGui.SameLine();
                if (ImGui.Button($"X##del{i}"))
                { 
                    storedData.Mobs.Remove(displayList[i]);
                    storedData.Save();
                    break;
                }
            }
            if (moveSrc >= 0 && moveDst >= 0 && moveSrc != moveDst)
            {
                MobLocation moved = displayList[moveSrc];
                displayList.RemoveAt(moveSrc);
                displayList.Insert(moveDst, moved);
                storedData.Mobs = storedData.Mobs.Where(m => !m.Patch.Equals(storedData.SelectedPatch, StringComparison.OrdinalIgnoreCase)).Concat(displayList).ToList();
                storedData.Save();
            }
            ImGui.EndTable();
        } 
        ImGui.EndChild(); 
        ImGui.BeginChild("bottom_buttons", new Vector2(-1, 50), false);
        float bottomBtnW = 90f;
        if (ImGui.Button("Import", new Vector2(bottomBtnW, 36f))) ImportFromClipboard();
        ImGui.SameLine(); 
        if (ImGui.Button("Clear All", new Vector2(bottomBtnW, 36f))) 
        {
            storedData.Mobs.Clear();
            storedData.Save();
        } 
        ImGui.EndChild();
        ImGui.Columns(1); 
    }

    private static void ImportFromClipboard()
    {
        string? clipboard = Clipboard.GetClipboardText();
        if (string.IsNullOrWhiteSpace(clipboard)) return;
        storedData.Mobs.Clear();
        foreach (string line in clipboard.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        { 
            if (!line.Contains('@') || !line.Contains('(') || !line.Contains(')')) continue;
            int mobEnd = line.IndexOf('@'), coordStart = line.IndexOf('('), coordEnd = line.IndexOf(')');
            if (mobEnd <= 0 || coordStart <= mobEnd || coordEnd <= coordStart) continue;
            string mob = line[..mobEnd].Trim(), zone = CleanZone(line[(mobEnd + 1)..coordStart]);
            string[] coords = line[(coordStart + 1)..coordEnd].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length != 2 || !float.TryParse(coords[0], out float x) || !float.TryParse(coords[1], out float y)) continue;
            string instance = string.Empty;
            int idx = line.IndexOf("Instance", coordEnd + 1, StringComparison.OrdinalIgnoreCase);
            if (idx > -1) instance = line[(idx + 8)..].Trim(); string patch = DeterminePatchForZone(zone);
            storedData.Mobs.Add(new() { Mob = CapitalizeEachWord(mob), Zone = zone, X = x, Y = y, Instance = ParseInstanceId(instance), Patch = patch });
        }
        storedData.Save(); _currentCachedPatch = string.Empty;
    }

    private static void MoveMobByDirection(MobLocation mob, int direction) 
    { 
        int idx = storedData.Mobs.IndexOf(mob); 
        if (idx < 0) return; int newIdx = idx + direction; if (newIdx < 0 || newIdx >= storedData.Mobs.Count) return;
        storedData.Mobs.RemoveAt(idx);
        storedData.Mobs.Insert(newIdx, mob);
        storedData.Save(); _currentCachedPatch = string.Empty;
    }

    private unsafe void SendChat()
    {
        List<MobLocation> aliveMobs = storedData.Mobs.Where(m => m.Patch.Equals(storedData.SelectedPatch, StringComparison.OrdinalIgnoreCase)).ToList();
        if (aliveMobs.Count == 0) return;
        MobLocation targetMob = aliveMobs[0];
        string msg = storedData.ChatMessage.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        msg = msg.Replace("<target>", targetMob.Mob + GetInstanceMarker(targetMob.Instance));
        if (!MapLinks.Maps.TryGetValue(targetMob.Zone, out (uint TerritoryId, uint MapId) mapInfo))
        {
            LOG.Warning($"[MapLink] Could not find map info for zone: {targetMob.Zone}");
            return;
        }
        try { MapLinks.SetFlag(mapInfo.TerritoryId, mapInfo.MapId, targetMob.X, targetMob.Y); }
        catch (Exception ex) { LOG.Error("Map link error: Failed to calculate coordinates or set flag.", ex); }
        string instanceMarker = string.Empty;
        if (targetMob.Instance > 0)
        {
            instanceMarker = $" {GetInstanceMarker(targetMob.Instance)}";
            LOG.Debug($"[MapLink] Adding instance marker: '{instanceMarker}'");
        }
        msg = msg.Replace("<location>", $"<flag>{instanceMarker}");
        (bool Enabled, string Prefix, XivChatType ChatType)[] chatTargets = new[] {(storedData.Yell, "/y ", XivChatType.Yell), (storedData.Shout, "/sh ", XivChatType.Shout), (storedData.Echo, "/e ", XivChatType.Echo), (storedData.Party, "/p ", XivChatType.Party), (storedData.Say, "/s ", XivChatType.Say)};
        if (UIModule.Instance() == null) return;
        foreach ((bool enabled, string prefix, XivChatType chatType) in chatTargets)
        {
            if (!enabled) continue;
            SeString message = new SeStringBuilder().AddText(prefix).AddText(msg).Build();
            byte[] bytes = message.Encode();
            if (bytes == null || bytes.Length == 0) continue;
            fixed (byte* ptr = bytes)
            {
                Utf8String* utf8String =
                    Utf8String.FromSequence(ptr);
                UIModule.Instance()->ProcessChatBoxEntry(utf8String, 1);
                utf8String->Dtor(true);
            }
        }
    }

    private static string CapitalizeEachWord(string input) => string.IsNullOrWhiteSpace(input) ? string.Empty : string.Join(' ', input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w[1..].ToLowerInvariant() : string.Empty)));
    private static string CleanZone(string z) => new string(z.Where(c => c < 0xE000 || c > 0xF8FF).ToArray()).Trim();
    private static string DeterminePatchForZone(string z) => ZoneToPatch.FirstOrDefault(kv => CleanZone(z).IndexOf(kv.Key, StringComparison.OrdinalIgnoreCase) >= 0).Value ?? "ARR";
    private static uint ParseInstanceId(string i) => string.IsNullOrWhiteSpace(i) ? 0 : int.TryParse(i, out int v) ? (uint)v : i.Trim().ToUpperInvariant() switch { "ONE" => 1u, "TWO" => 2u, "THREE" => 3u, "FOUR" => 4u, "FIVE" => 5u, "SIX" => 6u, "SEVEN" => 7u, "EIGHT" => 8u, "NINE" => 9u, _ => 0u };
    private static string GetInstanceMarker(uint id) => id switch { 1u => $"{(char)SeIconChar.Instance1}", 2u => $"{(char)SeIconChar.Instance2}", 3u => $"{(char)SeIconChar.Instance3}", 4u => $"{(char)SeIconChar.Instance4}", 5u => $"{(char)SeIconChar.Instance5}", 6u => $"{(char)SeIconChar.Instance6}", 7u => $"{(char)SeIconChar.Instance7}", 8u => $"{(char)SeIconChar.Instance8}", 9u => $"{(char)SeIconChar.Instance9}", _ => string.Empty };
    public void Dispose() { }
}