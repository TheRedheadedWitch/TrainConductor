using System;
using System.Collections.Generic;
using System.Globalization;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
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
    public static SeString ReplaceMapPlaceholder(string text, string zone, float x, float y, uint instanceId = 0) => text.Split("<location>", StringSplitOptions.None).Length switch {1 => new SeStringBuilder().AddText(text).Build(), 2 => new SeStringBuilder().AddText(text.Split("<location>", StringSplitOptions.None)[0]).Append(GenerateMapLinkSeString(zone, x, y, instanceId)).AddText(text.Split("<location>", StringSplitOptions.None)[1]).Build(), _ => ((Func<SeString>)(() => { LOG.Error($"Too many <map> placeholders in text: {text}, {text.Split("<location>", StringSplitOptions.None).Length - 1} detected."); return new SeStringBuilder().AddText(string.Empty).Build(); }))()};
    public static SeString GenerateMapLinkSeString(string zone, float x, float y, uint instanceId = 0) => !Maps.TryGetValue(zone, out (uint TerritoryId, uint MapId) mapInfo) ? new SeStringBuilder().Build() : new SeStringBuilder().Add(new MapLinkPayload(mapInfo.TerritoryId, mapInfo.MapId, x, y, 0.0f)).AddText($"{(char)SeIconChar.LinkMarker}{zone}{GenerateInstanceString(instanceId)} ({x:F1}, {y:F1})").Add(RawPayload.LinkTerminator).Build();
    public static string GenerateInstanceString(uint instanceId, string? i0Text = null) => instanceId switch {0 => i0Text ?? string.Empty, 1 => $"{(char)SeIconChar.Instance1}", 2 => $"{(char)SeIconChar.Instance2}", 3 => $"{(char)SeIconChar.Instance3}", 4 => $"{(char)SeIconChar.Instance4}", 5 => $"{(char)SeIconChar.Instance5}", 6 => $"{(char)SeIconChar.Instance6}", 7 => $"{(char)SeIconChar.Instance7}", 8 => $"{(char)SeIconChar.Instance8}", 9 => $"{(char)SeIconChar.Instance9}", _ => $"i{instanceId}"};
}
