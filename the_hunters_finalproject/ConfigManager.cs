using System;
using System.IO;
using System.Text.Json;

namespace the_hunters_finalproject;

public static class ConfigManager
{
    private static readonly string FilePath = Path.Combine("Content", "sim_config.json");
    private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };

    public static SimConfig Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<SimConfig>(json) ?? new SimConfig();
            }
        }
        catch { }

        return new SimConfig();
    }

    public static void Save(SimConfig config)
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(config, _writeOptions));
        }
        catch { }
    }
}
