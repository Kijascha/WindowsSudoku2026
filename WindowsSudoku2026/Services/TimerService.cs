using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Threading;

namespace WindowsSudoku2026.Services
{
    public partial class TimerService : ObservableObject, ITimerService
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        [ObservableProperty] private TimeSpan _elapsedTime;
        [ObservableProperty] private bool _isRunning;

        public TimerService()
        {
            _timer.Tick += (s, e) => ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
        }

        public void Start(TimeSpan startTime)
        {
            ElapsedTime = startTime;
            _timer.Start();
            IsRunning = true;
        }

        public void Pause()
        {
            _timer.Stop();
            IsRunning = false;
        }

        public void Reset()
        {
            _timer.Stop();
            ElapsedTime = TimeSpan.Zero;
            IsRunning = false;
        }
    }
}
