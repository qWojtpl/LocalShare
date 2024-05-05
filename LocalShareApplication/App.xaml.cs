using Plugin.LocalNotification;

namespace LocalShareApplication;

public partial class App : Application
{

    private readonly int width = 380;
    private readonly int height = 660;

    public App()
    {
        InitializeComponent();

        LocalNotificationCenter.Current.Show(new NotificationRequest()
        {
            NotificationId = 0,
            Title = "LocalShare App",
            Subtitle = "LocalShare Application Worker",
            Description = "LocalShare is now listening on port 2780",
            BadgeNumber = 42,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(5),
            }
        });

        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {

        Window window = base.CreateWindow(activationState);

        window.Width = width;
        window.MinimumWidth = width;
        window.MaximumWidth = width;
        window.Height = height;
        window.MinimumHeight = height;
        window.MaximumHeight = height;
        window.Title = "LocalShare";

        return window;
    }

}
