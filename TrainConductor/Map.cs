using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Globalization;
#pragma warning disable IDE1006

namespace FFXIVTrainConductor;

public static class MapLinks
{
    public static Dictionary<string, (uint TerritoryId, uint MapId)> Maps { get; set; } = new();

    static MapLinks()
    {
        ExcelSheet<TerritoryType> sheet = SERVICES.Data.GetExcelSheet<TerritoryType>();
        if (sheet == null) return;
        foreach (TerritoryType t in sheet)
            if (t.PlaceName.RowId > 0 && t.Map.RowId > 0)
            {
                string n = t.PlaceName.Value.Name.ToString();
                if (!string.IsNullOrEmpty(n) && !Maps.ContainsKey(n))
                    Maps[n] = (t.RowId, t.Map.RowId);
            }
    }
    public static unsafe void SetFlag(uint territoryId, uint mapId, float x, float y)
    {
        ExcelSheet<TerritoryType> territorySheet = SERVICES.Data.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) return;
        TerritoryType territoryRow = territorySheet.GetRow(territoryId);
        if (territoryRow.RowId == 0) return;
        Map map = territoryRow.Map.Value;
        if (map.RowId != mapId) { LOG.Warning($"[MapLink] Provided MapId {mapId} does not match TerritoryType MapId {map.RowId}. Using TerritoryType's MapId."); }
        if (map.RowId == 0) return;
        if (AgentMap.Instance() == null) return;
        AgentMap.Instance()->FlagMarkerCount = 0;
        AgentMap.Instance()->SetFlagMapMarker(territoryId, map.RowId, ((x - 41.9f) * 50f * (map.SizeFactor / 100f)) + 1024f - map.OffsetX, ((y - 41.9f) * 50f * (map.SizeFactor / 100f)) + 1024f - map.OffsetY);
    }
    public static SeString ReplaceMapPlaceholder(string text, string zone, float x, float y, uint instanceId = 0) => text.Split("<location>", StringSplitOptions.None).Length switch {1 => new SeStringBuilder().AddText(text).Build(), 2 => new SeStringBuilder().AddText(text.Split("<location>", StringSplitOptions.None)[0]).Append(GenerateMapLinkSeString(zone, x, y, instanceId)).AddText(text.Split("<location>", StringSplitOptions.None)[1]).Build(), _ => ((Func<SeString>)(() => { LOG.Error($"Too many <map> placeholders in text: {text}, {text.Split("<location>", StringSplitOptions.None).Length - 1} detected."); return new SeStringBuilder().AddText(string.Empty).Build(); }))()};
    public static SeString GenerateMapLinkSeString(string zone, float x, float y, uint instanceId = 0) => !Maps.TryGetValue(zone, out (uint TerritoryId, uint MapId) mapInfo) ? new SeStringBuilder().Build() : new SeStringBuilder().Add(new MapLinkPayload(mapInfo.TerritoryId, mapInfo.MapId, x, y, 0.0f)).Add(new TextPayload($"{(char)SeIconChar.LinkMarker}{zone}{GenerateInstanceString(instanceId)} ({x:F1}, {y:F1})")).Add(RawPayload.LinkTerminator).Build();
    public static string GenerateInstanceString(uint instanceId, string? i0Text = null) => instanceId switch {0 => i0Text ?? string.Empty, 1 => $"{(char)SeIconChar.Instance1}", 2 => $"{(char)SeIconChar.Instance2}", 3 => $"{(char)SeIconChar.Instance3}", 4 => $"{(char)SeIconChar.Instance4}", 5 => $"{(char)SeIconChar.Instance5}", 6 => $"{(char)SeIconChar.Instance6}", 7 => $"{(char)SeIconChar.Instance7}", 8 => $"{(char)SeIconChar.Instance8}", 9 => $"{(char)SeIconChar.Instance9}", _ => $"i{instanceId}"};
}