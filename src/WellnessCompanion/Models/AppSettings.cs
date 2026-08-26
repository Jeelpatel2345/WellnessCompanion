namespace WellnessCompanion.Models;

public sealed class AppSettings
{
    public int WaterIntervalMinutes { get; set; } = 60;
    public int WaterDurationSeconds { get; set; } = 30;
    public int WaterVerificationDelaySeconds { get; set; } = 30;
    public int WaterRetryDelaySeconds { get; set; } = 15;
    public int MaxWaterVerificationAttempts { get; set; } = 3;

    public int FoodIntervalMinutes { get; set; } = 180;
    public int FoodDurationMinutes { get; set; } = 30;

    public int IdleThresholdSeconds { get; set; } = 120;

    public bool StartWithWindows { get; set; } = false;
    public bool SoundEnabled { get; set; } = true;
}
