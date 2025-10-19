using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVTrainConductor;
using Lumina.Excel.Sheets;

namespace SRankAssistant;

internal struct TrackedNpcData
{
    public uint DataId;
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
        uint currentInstance = SERVICES.ClientState.Instance;
        HashSet<uint> currentObjects = new();
        List<MobLocation> patchMobs = ConductorWindow.storedData.GetPatchMobs();
        foreach (IGameObject obj in SERVICES.Objects)
        {
            if (obj is not IBattleNpc monster) continue;
            if (monster.BaseId == 0 || obj.EntityId == 0) continue;
            currentObjects.Add(obj.EntityId);
            string monsterName = AllMonsterNames.TryGetValue(monster.NameId, out string? name) ? name : monster.Name.TextValue;
            if (!TrackedNpcs.ContainsKey(obj.EntityId))
                TrackedNpcs[obj.EntityId] = new TrackedNpcData { DataId = monster.BaseId, Name = monsterName, IsDeadAndCounted = false };
            if (monster.CurrentHp == 0 && !TrackedNpcs[obj.EntityId].IsDeadAndCounted)
            {
                TrackedNpcData npcData = TrackedNpcs[obj.EntityId];
                LOG.Debug($"KILLED MONSTER - ID: {npcData.DataId}, Name: {npcData.Name}");
                MobLocation? matchedMob = patchMobs.Find(m => string.Equals(m.Mob, npcData.Name, StringComparison.OrdinalIgnoreCase));
                if (matchedMob != null)
                {
                    ConductorWindow.storedData.Mobs.Remove(matchedMob);
                    ConductorWindow.storedData.Save();
                }
                TrackedNpcs.Remove(obj.EntityId);
            }
        }
        List<uint> deadObjects = TrackedNpcs.Keys.Except(currentObjects).ToList();
        foreach (uint deadId in deadObjects)
            TrackedNpcs.Remove(deadId);
    }
}