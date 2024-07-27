using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;



public class EnergyUI : MonoBehaviour
{
    [SerializeField] public TMP_Text textEnergy;
    [SerializeField] public TMP_Text textTime;

    public float energytimeDelta = 0;

    private static double serverTime;
    public static int energyTime;

    public static double ServerTime { get => serverTime; 
        set 
        { 
            serverTime = value; 
        } 
    }

    // Update is called once per frame
    void Update()
    {
        ServerTime += Time.deltaTime;

        float energyT = float.Parse(ButtonManager.userData.energytime, CultureInfo.InvariantCulture);

        energytimeDelta = (int)(energyT) -  (int)(ServerTime);

        if (energytimeDelta<0)
        {
            ButtonManager buttonManager = FindObjectOfType<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.GetData();
            }
        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(energytimeDelta);

        // Форматируем TimeSpan в строку формата "HH:mm:ss"
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                             timeSpan.Hours,
                                             timeSpan.Minutes,
                                             timeSpan.Seconds);
        textTime.text = formattedTime;

        textEnergy.text = ButtonManager.userData.energy.ToString() + "/20";
    }
}
