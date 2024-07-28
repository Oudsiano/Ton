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

    public static double ServerTime
    {
        get => serverTime;
        set
        {
            serverTime = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ButtonManager buttonManager = FindObjectOfType<ButtonManager>();
        if (!buttonManager.GameStarted)
            return;


        ServerTime += Time.deltaTime;

        long energyT = ButtonManager.userData.energytime;

        energytimeDelta = (int)(energyT) - (int)(ServerTime);

        if (energytimeDelta < 0)
            buttonManager.GetData();

        TimeSpan timeSpan = TimeSpan.FromSeconds(energytimeDelta);

        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                             timeSpan.Hours,
                                             timeSpan.Minutes,
                                             timeSpan.Seconds);
        textTime.text = formattedTime;

        textEnergy.text = ButtonManager.userData.energy.ToString() + "/20";
    }
}
