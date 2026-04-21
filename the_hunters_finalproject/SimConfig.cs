namespace the_hunters_finalproject;

public class SimConfig
{
    public int InitialFoxCount { get; set; } = 3;
    public int InitialRabbitCount { get; set; } = 10;
    public float DefaultSpeed { get; set; } = 1.0f;
    public int LifetimeKills { get; set; } = 0;
    public int BestSessionKills { get; set; } = 0;
    public float BestSurvivalSeconds { get; set; } = 0f;
}
