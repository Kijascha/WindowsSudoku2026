namespace WindowsSudoku2026.Services;

public interface ITimerService
{
    TimeSpan ElapsedTime { get; set; }
    bool IsRunning { get; set; }
    void Start(TimeSpan startTime);
    void Pause();
    void Reset();
}