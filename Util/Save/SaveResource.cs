using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SaveResource : Resource
{
    [Export]
    public Dictionary<string, Variant> save = new();

    private static string SaveDir = "user://saves";

    private static void SaveDirCheck()
    {
        if (!DirAccess.DirExistsAbsolute(SaveDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(SaveDir);
        }
    }

    public void SaveToFile(string name)
    {
        string thisAsJson = ToJson();
        GD.Print($"JSON Save: {thisAsJson}");

        var filePath = $"{SaveDir}/save_{name}.json";

        SaveDirCheck();

        using (var saveGame = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
        {
            saveGame.StoreString(thisAsJson);
        }
        GD.Print($"Game saved to {filePath}!");
    }

    public string ToJson()
    {
        return Json.Stringify(save);
    }

    public static SaveResource? FromName(string name)
    {
        return FromFileName($"save_{name}.json");
    }

        public static SaveResource? FromFileName(string fileName)
    {
        SaveDirCheck();
        var filePath = $"{SaveDir}/{fileName}";

        using (var saveGame = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
        {
            var jsonString = saveGame.GetLine();

            var json = new Json();
            var parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                GD.PrintErr($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at {json.GetErrorLine()}");

                return null;
            }

            var saveResource = new SaveResource
            {
                save = new Dictionary<string, Variant>((Dictionary)json.Data)
            };
            return saveResource;
        }
    }

    public static SaveResource FromCurrent(Node current)
    {
        return new SaveResource()
            .WriteCurrentDateAndTimeToResource(current)
            .WriteCurrentIslandToResource(current)
            .WriteCurrentPlayerToResource(current);
    }

    private SaveResource WriteCurrentPlayerToResource(Node current)
    {
        var player = current.GetNode<Player>("/root/MainGame/Player");

        save.Add("Player.Position", player.Position);

        return this;
    }

    private SaveResource WriteCurrentIslandToResource(Node current)
    {
        var island = current.GetNode<Island>("/root/MainGame/Island");

        save.Add($"Island.Radius", island.Radius);
        save.Add($"Island.BelowGroundFactor", island.BelowGroundFactor);
        save.Add($"Island.TerrainIndentFactor", island.TerrainIndentFactor);
        save.Add($"Island.SpawnPlatformVoxel", island.SpawnPlatformVoxel);
        save.Add($"Island.VoxelGeneration", island.VoxelGeneration);
        save.Add($"Island.WaterLevel", island.WaterLevel);
        save.Add($"Island.Changes", island.Changes);

        // Noises
        var terrainNoiseA = (FastNoiseLite)island.TerrainNoiseA.Noise;
        save.Add($"Island.TerrainNoiseA.Seed", terrainNoiseA.Seed);
        save.Add($"Island.TerrainNoiseA.NoiseType", (int)terrainNoiseA.NoiseType);
        save.Add($"Island.TerrainNoiseA.Frequency", terrainNoiseA.Frequency);
        save.Add($"Island.TerrainNoiseA.FractalType", (int)terrainNoiseA.FractalType);

        var terrainNoiseB = (FastNoiseLite)island.TerrainNoiseB.Noise;
        save.Add($"Island.TerrainNoiseB.Seed", terrainNoiseB.Seed);
        save.Add($"Island.TerrainNoiseB.NoiseType", (int)terrainNoiseB.NoiseType);
        save.Add($"Island.TerrainNoiseB.Frequency", terrainNoiseB.Frequency);
        save.Add($"Island.TerrainNoiseB.FractalType", (int)terrainNoiseB.FractalType);


        return this;
    }

    private SaveResource WriteCurrentDateAndTimeToResource(Node current)
    {
        var dateAndTime = current.GetNode<DateAndTime>("/root/MainGame/DateAndTime");

        save.Add("DateAndTime.HoursPerDay", dateAndTime.HoursPerDay);
        save.Add("DateAndTime.MinutesPerHour", dateAndTime.MinutesPerHour);
        save.Add("DateAndTime.DaysPerMonth", dateAndTime.DaysPerMonth);
        save.Add("DateAndTime.MonthsPerYear", dateAndTime.MonthsPerYear);
        save.Add("DateAndTime.IncrementFactor", dateAndTime.IncrementFactor);
        save.Add("DateAndTime.Time", dateAndTime.Time);
        save.Add("DateAndTime.Hour", dateAndTime.Hour);
        save.Add("DateAndTime.Minute", dateAndTime.Minute);
        save.Add("DateAndTime.Day", dateAndTime.Day);
        save.Add("DateAndTime.Month", dateAndTime.Month);
        save.Add("DateAndTime.Year", dateAndTime.Year);

        return this;
    }
}
