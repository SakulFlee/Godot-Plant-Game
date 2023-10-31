using System.Reflection.Metadata.Ecma335;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SaveResource : Resource
{
    #region Island Settings
    [ExportCategory("Island Settings")]
    [Export]
    public uint Radius = 0;

    [Export]
    public float BelowGroundFactor = 0;

    [Export]
    public float TerrainIndentFactor = 0;

    [Export]
    public string SpawnPlatformVoxel = "";

    [Export]
    public Dictionary<int, Array<string>> VoxelGeneration = new();

    [Export]
    public int WaterLevel = 0;
    #endregion

    #region Island Noise
    [ExportCategory("Island Noise")]
    [Export]
    public NoiseTexture2D TerrainNoiseA = new();

    [Export]
    public NoiseTexture2D TerrainNoiseB = new();
    #endregion

    #region Island Change
    [ExportCategory("Island Settings")]
    [Export]
    public Dictionary<Vector3I, int> Changes = new();
    #endregion

    #region Date and Time
    [ExportCategory("Date and Time")]
    [Export]
    public uint HoursPerDay { get; private set; }

    [Export]
    public uint MinutesPerHour { get; private set; }

    [Export]
    public uint DaysPerMonth { get; private set; }

    [Export]
    public uint MonthsPerYear { get; private set; }

    [Export]
    public float IncrementFactor { get; private set; }

    [Export]
    public float Time { get; private set; }

    [Export]
    public uint Hour { get; private set; }

    [Export]
    public uint Minute { get; private set; }

    [Export]
    public uint Day { get; private set; }

    [Export]
    public uint Month { get; private set; }

    [Export]
    public uint Year { get; private set; }
    #endregion

    #region Functions
    public void SaveToFile(string name)
    {
        var thisAsJson = ToJson();

        var saveGame = FileAccess.Open($"user://save_{name}.save", FileAccess.ModeFlags.Write);
        saveGame.StoreString(thisAsJson);
    }

    public string ToJson()
    {
        return Json.Stringify(this);
    }

    public static SaveResource FromCurrent(Node current)
    {
        var saveResource = new SaveResource();

        saveResource = WriteIslandToResource(current, saveResource);
        saveResource = WriteDateAndTimeToResource(current, saveResource);

        return saveResource;
    }

    private static SaveResource WriteIslandToResource(Node current, SaveResource saveResource)
    {
        var island = current.GetNode<Island>("/MainGame/Island");

        saveResource.Radius = island.Radius;
        saveResource.BelowGroundFactor = island.BelowGroundFactor;
        saveResource.TerrainIndentFactor = island.TerrainIndentFactor;
        saveResource.SpawnPlatformVoxel = island.SpawnPlatformVoxel;
        saveResource.VoxelGeneration = island.VoxelGeneration;
        saveResource.WaterLevel = island.WaterLevel;
        saveResource.TerrainNoiseA = island.TerrainNoiseA;
        saveResource.TerrainNoiseB = island.TerrainNoiseB;
        saveResource.Changes = island.Changes;

        return saveResource;
    }

    private static SaveResource WriteDateAndTimeToResource(Node current, SaveResource saveResource)
    {
        var dateAndTime = current.GetNode<DateAndTime>("/MainGame/DateAndTime");

        saveResource.HoursPerDay = dateAndTime.HoursPerDay;
        saveResource.MinutesPerHour = dateAndTime.MinutesPerHour;
        saveResource.DaysPerMonth = dateAndTime.DaysPerMonth;
        saveResource.MonthsPerYear = dateAndTime.MonthsPerYear;
        saveResource.IncrementFactor = dateAndTime.IncrementFactor;
        saveResource.Time = dateAndTime.Time;
        saveResource.Hour = dateAndTime.Hour;
        saveResource.Minute = dateAndTime.Minute;
        saveResource.Day = dateAndTime.Day;
        saveResource.Month = dateAndTime.Month;
        saveResource.Year = dateAndTime.Year;

        return saveResource;
    }
    #endregion
}
