using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVTrainConductor;
using Lumina.Excel.Sheets;
using TrainConductor.Windows;

namespace SRankAssistant;

internal struct TrackedNpcData
{
    public uint NameId;
    public string Name;
    public bool IsDeadAndCounted;
}

internal static class MobDetector
{
    private static readonly Dictionary<uint, TrackedNpcData> TrackedNpcs = new();
    private static readonly Dictionary<uint, string> AllMonsterNames = SERVICES.Data.GetExcelSheet<BNpcName>().ToDictionary(e => e.RowId, e => e.Singular.ToString());
    internal static void Initialize() => SERVICES.Framework.Update += OnUpdate;

    internal static void Dispose()
    {
        SERVICES.Framework.Update -= OnUpdate;
        TrackedNpcs.Clear();
    }

    private static void OnUpdate(IFramework framework)
    {
        HashSet<uint> currentNameIds = new();
        List<MobLocation> patchMobs = ConductorWindow.storedData.GetPatchMobs();

        foreach (IBattleNpc monster in SERVICES.Objects.OfType<IBattleNpc>())
        {
            if (monster.NameId == 0) continue;

            currentNameIds.Add(monster.NameId);
            if (!AllMonsterNames.TryGetValue(monster.NameId, out string? name)) name = monster.Name.TextValue;
            if (!TrackedNpcs.ContainsKey(monster.NameId))
                TrackedNpcs[monster.NameId] = new TrackedNpcData { NameId = monster.NameId, Name = name, IsDeadAndCounted = false };

            if (monster.CurrentHp == 0 && !TrackedNpcs[monster.NameId].IsDeadAndCounted)
            {
                TrackedNpcData npcData = TrackedNpcs[monster.NameId];
                LOG.Debug($"KILLED MONSTER - NameId: {npcData.NameId}, Name: {npcData.Name}");
                MobLocation? matchedMob = patchMobs.Find(m => string.Equals(m.Mob, npcData.Name, StringComparison.OrdinalIgnoreCase));
                if (matchedMob != null)
                {
                    List<MobLocation> toRemove = ConductorWindow.storedData.Mobs.Where(m => string.Equals(m.Mob, npcData.Name, StringComparison.OrdinalIgnoreCase) && m.Instance == SERVICES.ClientState.Instance).ToList();
                    foreach (MobLocation mob in toRemove)
                        ConductorWindow.storedData.Mobs.Remove(mob);
                    if (toRemove.Count > 0)
                        ConductorWindow.storedData.Save();
                }
                TrackedNpcs[monster.NameId] = new TrackedNpcData { NameId = npcData.NameId, Name = npcData.Name, IsDeadAndCounted = true };
            }
        }
        List<uint> missingIds = TrackedNpcs.Keys.Except(currentNameIds).ToList();
        foreach (uint id in missingIds)
            TrackedNpcs.Remove(id);
    }
}
