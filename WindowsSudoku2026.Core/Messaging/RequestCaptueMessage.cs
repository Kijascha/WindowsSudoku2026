using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Media.Imaging;

namespace WindowsSudoku2026.Core.Messaging;

public class RequestCaptureMessage : AsyncRequestMessage<BitmapSource> { }
