using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmManager : MonoBehaviour
{
    [SerializeField]
    private List<SettingNotification> dailyNotifications;
    private void OnApplicationFocus(bool _focus)
    {
        if (_focus == false)
        {
            ControlNotifications.SendNotificationForDaily(dailyNotifications, new System.TimeSpan(0, 24, 0, 0));
        }
        else
        {
            ControlNotifications.Initialize();
        }
    }
}

