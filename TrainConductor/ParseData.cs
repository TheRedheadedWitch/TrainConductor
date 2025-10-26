using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Lumina.Excel.Sheets;
using System.Text.RegularExpressions;


namespace FFXIVTrainConductor;

internal static class ParseData
{
    internal static IPlayerCharacter Player => SERVICES.ClientState.LocalPlayer!;
    private static List<string> AetheryteNames = SERVICES.Data.GetExcelSheet<Aetheryte>().Where(a => a.PlaceName.Value.RowId != 0).Select(a => a.PlaceName.Value.Name.ToString()).Where(n => !string.IsNullOrEmpty(n)).ToList();
    private static Dictionary<string, (string Zone, float X, float Y)> AetheryteMapData = new();

    internal static string Parse(string data, uint Instance = 0, int DepartureHour = 0, int DepartureMinute = 0, MobLocation? Monster = null)
     => ReplaceInsensitive(data, "<name>", Player!.Name.ToString())
         .Pipe(s => ReplaceInsensitive(s, "<server>", Player!.CurrentWorld.Value.Name.ToString()))
         .Pipe(s => ReplaceInsensitive(s, "<expansion>", ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex]))
         .Pipe(s => ReplaceInsensitive(s, "<target>", (Monster != null ? Monster.Mob : string.Empty)))
//         .Pipe(s => ReplaceInsensitive(s, "<location>", $"{(Monster != null ? Monster.Zone} ({(Monster ?? new MobLocation()).X:0.0}, {(Monster ?? new MobLocation()).Y:0.0})"))
         .Pipe(s => ReplaceInsensitive(s, "<startingarea>", AetheryteNames.Count > 0 ? AetheryteNames[ConductorWindow.storedData.selectedAetheryteIndex] : string.Empty))
         .Pipe(s => ReplaceInsensitive(s, "<instancecharacter>", GetInstanceMarker(Instance)))
         .Pipe(s => ReplaceInsensitive(s, "<instance>", Instance != 0 ? $"INSTANCE" : string.Empty))
         .Pipe(s => ReplaceInsensitive(s, "<instancenumber>", Instance != 0 ? $"{Instance.ToString()}" : string.Empty))
         .Pipe(s => ReplaceInsensitive(s, "<@>", GetExpansionTag(ConductorWindow.Patches[ConductorWindow.storedData.selectedExpansionIndex])))
         .Pipe(s => ReplaceInsensitive(s, "<timediscord>", $"<t:{new DateTimeOffset((DateTime.Now.TimeOfDay.TotalMinutes > DepartureHour * 60 + DepartureMinute ? DateTime.Now.Date.AddDays(1).AddHours(DepartureHour).AddMinutes(DepartureMinute) : DateTime.Now.Date.AddHours(DepartureHour).AddMinutes(DepartureMinute))).ToUnixTimeSeconds()}:R>"))
         .Pipe(s => ReplaceInsensitive(s, "<timecountdown>", $"{Math.Round((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local) < DateTime.Now ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local).AddDays(1) - DateTime.Now : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local) - DateTime.Now).TotalMinutes)} minute{(Math.Round((new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local) < DateTime.Now ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local).AddDays(1) - DateTime.Now : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DepartureHour, DepartureMinute, 0, DateTimeKind.Local) - DateTime.Now).TotalMinutes) != 1 ? "s" : string.Empty)}"));

    private static string ReplaceInsensitive(string input, string pattern, string replacement) => Regex.Replace(input, Regex.Escape(pattern), replacement, RegexOptions.IgnoreCase);
    private static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> func) => func(input);
    private static string GetExpansionTag(string expansion) => expansion switch {"Dawntrail" => "<@&1255412015757918240>", "Endwalker" => "<@&934264593470070784>", "Shadowbringers" => "<@&934264458069561354>", "ARR" or "Heavensward" or "Stormblood" => "<@&1091904124301353110>", _ => ""};
    private static string GetInstanceMarker(uint id) => id switch { 1u => $"{(char)SeIconChar.Instance1}", 2u => $"{(char)SeIconChar.Instance2}", 3u => $"{(char)SeIconChar.Instance3}", 4u => $"{(char)SeIconChar.Instance4}", 5u => $"{(char)SeIconChar.Instance5}", 6u => $"{(char)SeIconChar.Instance6}", 7u => $"{(char)SeIconChar.Instance7}", 8u => $"{(char)SeIconChar.Instance8}", 9u => $"{(char)SeIconChar.Instance9}", _ => string.Empty };
    internal static void Dispose() { }
}
