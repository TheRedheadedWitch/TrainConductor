using Dalamud.Configuration;

namespace FFXIVTrainConductor;

public class StoredData : IPluginConfiguration
{
    public int Version { get; set; }
    public List<MobLocation> Mobs { get; set; } = new();
    public string SelectedPatch { get; set; } = string.Empty;
    public string ChatMessage { get; set; } = string.Empty;
    public bool Yell { get; set; }
    public bool Shout { get; set; }
    public bool Echo { get; set; }
    public bool Party { get; set; }
    public bool Say { get; set; }
    public string Discord { get; set; } = string.Empty;
    public int selectedAetheryteIndex = 0;
    public int selectedExpansionIndex = 0;
    public bool AnnounceNN { get; set; }
    public bool AnnounceYell { get; set; }
    public bool AnnounceShout { get; set; }
    public bool AnnounceParty { get; set; }
    public bool AnnounceLS1 { get; set; }
    public bool AnnounceLS2 { get; set; }
    public bool AnnounceLS3 { get; set; }
    public bool AnnounceLS4 { get; set; }
    public bool AnnounceLS5 { get; set; }
    public bool AnnounceLS6 { get; set; }
    public bool AnnounceLS7 { get; set; }
    public bool AnnounceLS8 { get; set; }
    public bool AnnounceCWLS1 { get; set; }
    public bool AnnounceCWLS2 { get; set; }
    public bool AnnounceCWLS3 { get; set; }
    public bool AnnounceCWLS4 { get; set; }
    public bool AnnounceCWLS5 { get; set; }
    public bool AnnounceCWLS6 { get; set; }
    public bool AnnounceCWLS7 { get; set; }
    public bool AnnounceCWLS8 { get; set; }
    public string CustomAnnounceMessage { get; set; } = string.Empty;

    public void Save() => SERVICES.Interface.SavePluginConfig(this);

    public static StoredData Load()
    {
        StoredData data = SERVICES.Interface.GetPluginConfig() as StoredData ?? new StoredData { Version = 1 };
        if (string.IsNullOrEmpty(data.SelectedPatch))
            data.SelectedPatch = "Dawntrail";
        return data;
    }

    public List<MobLocation> GetPatchMobs() => Mobs.Where(m => m.Patch == SelectedPatch).ToList();
}

public class MobLocation
{
    public string Mob { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public string Instance { get; set; } = string.Empty;
    public string Patch { get; set; } = "Uncategorized";
}