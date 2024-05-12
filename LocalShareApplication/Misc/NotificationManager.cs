
using Plugin.LocalNotification;

namespace LocalShareApplication.Misc;

public static class NotificationManager
{

    public static void SendNotification(string title, string subtitle, string description)
    {
#if ANDROID
        LocalNotificationCenter.Current.Show(new NotificationRequest()
        {
            NotificationId = 0,
            Title = title,
            Subtitle = subtitle,
            Description = description,
            BadgeNumber = 42
        });
#endif
    }
        
}
