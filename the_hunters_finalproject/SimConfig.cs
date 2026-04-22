namespace the_hunters_finalproject;

public class SimConfig
{
    public int InitialFoxCount { get; set; } = 5;
    public int InitialRabbitCount { get; set; } = 15;
    public float DefaultSpeed { get; set; } = 1.0f;
    public int LifetimeKills { get; set; } = 0;
    public int BestSessionKills { get; set; } = 0;
    public float BestSurvivalSeconds { get; set; } = 0f;

    // simulation tuning
    public float FoxHungerLimit { get; set; } = 20f;
    public float FoxReproInterval { get; set; } = 15f;
    public float RabbitReproInterval { get; set; } = 12f;
    public float RabbitLifespan { get; set; } = 60f;
    public int   GrassZoneCount { get; set; } = 5;
}
