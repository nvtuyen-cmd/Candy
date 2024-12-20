using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_IOS && !UNITY_EDITOR
    using Unity.Notifications.iOS;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
    using Unity.Notifications.Android;
#endif


public class NotificationManager : MonoBehaviour
{
    const string channelID = "channel_id";
    private static NotificationManager instance;
#if UNITY_ANDROID && !UNITY_EDITOR
    private bool initialized;
#endif

    public static NotificationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = "NotificationManager";
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<NotificationManager>();
            }
            return instance;
        }
    }


    public void Initialize(bool _cancelPendingNotifications)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (initialized == false)
        {
            initialized = true;
            var c = new AndroidNotificationChannel()
            {
                Id = channelID,
                Name = "Default Channel",
                Importance = Importance.High,
                Description = "Generic notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(c);
        }
        if (_cancelPendingNotifications == true)
        {
            AndroidNotificationCenter.CancelAllNotifications();
            //AndroidNotificationCenter.CancelAllDisplayedNotifications();
        }
#endif
#if UNITY_IOS && !UNITY_EDITOR
            if (cancelPendingNotifications == true)
            {
                iOSNotificationCenter.RemoveAllScheduledNotifications();
                iOSNotificationCenter.RemoveAllDeliveredNotifications();
            }
#endif
    }

    internal void SendNotification(string _title, string _text, DateTime _timeDelayFromNow, string _smallIcon, string _largeIcon, string _customData, TimeSpan? _repeatInterval)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var notification = new AndroidNotification();
        notification.Title = _title;
        notification.Text = _text;
        if (_repeatInterval != null)
        {
            notification.RepeatInterval = _repeatInterval;
        }
        if (_smallIcon != null)
        {
            notification.SmallIcon = _smallIcon;
        }
        if (_largeIcon != null)
        {
            notification.LargeIcon = _largeIcon;
        }
        if (_customData != null)
        {
            notification.IntentData = _customData;
        }
        notification.FireTime = _timeDelayFromNow;

        AndroidNotificationCenter.SendNotification(notification, channelID);
#endif

#if UNITY_IOS &&!UNITY_EDITOR
            iOSNotificationTimeIntervalTrigger timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = timeDelayFromNow,
                Repeats = false,
            };

            iOSNotification notification = new iOSNotification()
            {
                Title = title,
                Subtitle = "",
                Body = text,
                Data = customData,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
    }

    public string AppWasOpenFromNotification()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();

        if (notificationIntentData != null)
        {
            return notificationIntentData.Notification.IntentData;
        }
        else
        {
            return null;
        }
#elif UNITY_IOS && !UNITY_EDITOR
            iOSNotification notificationIntentData = iOSNotificationCenter.GetLastRespondedNotification();

            if (notificationIntentData != null)
            {
                return notificationIntentData.Data;
            }
            else
            {
                return null;
            }
#else
        return null;
#endif
    }
}
public static class ControlNotifications
{

    public static void Initialize(bool _cancelPending = true)
    {
        NotificationManager.Instance.Initialize(_cancelPending);
    }

    public static void SendNotification(string _title, string _text, TimeSpan _timeDelayFromNow, string _smallIcon = null, string _largeIcon = null, string _customData = "")
    {
        DateTime time = DateTime.Now.Add(_timeDelayFromNow);
        NotificationManager.Instance.SendNotification(_title, _text, time, _smallIcon, _largeIcon, _customData, null);
    }
    public static void SendNotificationForDaily(List<SettingNotification> _datas, TimeSpan _timeRepeat)
    {
        foreach (SettingNotification data in _datas)
        {
            SendNotificationForReapeat(data, _timeRepeat);
        }
    }
    public static void SendNotificationForReapeat(SettingNotification _data, TimeSpan _timeRepeat)
    {
        DateTime now = System.DateTime.Now;
        DateTime timeShow = new DateTime(now.Year, now.Month, now.Day, _data.hour, _data.minute, 0);
        if (now.Hour > _data.hour || (now.Hour == _data.hour && now.Minute > _data.minute))
        {
            timeShow = timeShow.AddDays(1);
        }
        NotificationManager.Instance.SendNotification(_data.title, _data.message, timeShow, _data.smallIcon, _data.largeIcon, _data.customData, _timeRepeat);
    }
    public static string AppWasOpenFromNotification()
    {
        return NotificationManager.Instance.AppWasOpenFromNotification();
    }
}
[System.Serializable]
public struct SettingNotification
{
    public string title;
    public string message;
    public byte hour;
    public byte minute;
    public string smallIcon;
    public string largeIcon;
    public string customData;
}

