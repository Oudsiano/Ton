using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class ButtonManager : MonoBehaviour
{
    public TMP_Text id;
    public TMP_Text balanceText;
    public TMP_Text ClickCountText;
    public TMP_Text RoundText;
    public Slider clickSlider;
    public GameObject floatingTextPrefab; // Префаб плавающего текста

    private Animator outputFrameAnimator;
    private int clickCount = 0;
    private int clickThreshold = 100;
    private int round = 1;
    private int coinsPerClick = 1;
    private string userId;
    private string apiUrl = "https://2b85-195-10-205-80.ngrok-free.app/api/";

    void Start()
    {
        // Получаем параметры из URL
        string url = Application.absoluteURL;
        userId = GetParameterFromUrl(url, "user_id");
        id.text = userId;
        Debug.Log("User ID: " + userId);

        if (!string.IsNullOrEmpty(userId))
        {
            StartCoroutine(InitializeUserData());
        }
        else
        {
            Debug.LogError("User ID not found in URL");
        }

        clickSlider.minValue = 0;
        clickSlider.maxValue = 1;

        // Получаем аниматор объекта output_frame_0001
        //GameObject outputFrame = GameObject.Find("output_frame_0001");
        //if (outputFrame != null)
        //{
        //    outputFrameAnimator = outputFrame.GetComponent<Animator>();
        //}
        //else
        //{
        //    Debug.LogError("output_frame_0001 not found");
        //}

        UpdateUI();
    }

    public void OnButtonClick()
    {
        clickCount++;
        StartCoroutine(UpdateCoins(coinsPerClick)); // Добавляем монеты с каждым кликом
        StartCoroutine(UpdateCountBlows(clickCount)); // Обновляем количество тапов на сервере
        UpdateUI();
        //outputFrameAnimator.SetBool("isAttacking", true);

        // Создаем плавающий текст
        ShowFloatingText($"+{coinsPerClick}");

        if (clickCount >= clickThreshold)
        {
            clickCount = 0;
            round++;
            clickThreshold += round * 10; // Увеличиваем количество нажатий с каждым раундом
            StartCoroutine(UpdateLevelBoss(round)); // Обновляем номер босса на сервере
            UpdateUI();
        }

        // Запускаем сброс параметра через одну секунду
        StartCoroutine(ResetAttackParameter());
    }

    IEnumerator ResetAttackParameter()
    {
        yield return null; // Ожидаем завершения анимации (время может быть настроено)
//        outputFrameAnimator.SetBool("isAttacking", false);
    }

    void UpdateUI()
    {
        ClickCountText.text = $"{clickCount} / {clickThreshold}";
        clickSlider.value = (float)clickCount / clickThreshold;
        RoundText.text = $"{round}";
    }

    public void OnGetBalanceButtonClicked()
    {
        StartCoroutine(GetCoins());
    }

    IEnumerator InitializeUserData()
    {
        yield return GetCoins();
        yield return GetLevelBoss();
        yield return GetCountBlows();
    }

    IEnumerator GetCoins()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl + "coins/" + userId))
        {
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                balanceText.text = "Error fetching balance";
            }
            else
            {
                // Получаем количество монет из ответа
                string jsonResponse = www.downloadHandler.text;
                CoinData coinData = JsonUtility.FromJson<CoinData>(jsonResponse);
                balanceText.text = "Balance: " + coinData.balance;
                Debug.Log("Balance: " + coinData.balance);
            }
        }
    }

    IEnumerator UpdateCoins(int coins)
    {
        CoinUpdateData data = new CoinUpdateData { coins = coins };
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest www = UnityWebRequest.Put(apiUrl + "coins/" + userId, jsonData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Обновляем отображение количества монет
                StartCoroutine(GetCoins());
            }
        }
    }

    IEnumerator GetLevelBoss()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl + "level_boss/" + userId))
        {
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                LevelBossData levelBossData = JsonUtility.FromJson<LevelBossData>(jsonResponse);
                round = levelBossData.level_boss;
                UpdateUI();
                Debug.Log("Level Boss: " + levelBossData.level_boss);
            }
        }
    }

    IEnumerator UpdateLevelBoss(int levelBoss)
    {
        LevelBossData data = new LevelBossData { level_boss = levelBoss };
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest www = UnityWebRequest.Put(apiUrl + "level_boss/" + userId, jsonData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
    }

    IEnumerator GetCountBlows()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl + "count_blows/" + userId))
        {
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                CountBlowsData countBlowsData = JsonUtility.FromJson<CountBlowsData>(jsonResponse);
                clickCount = countBlowsData.count_blows;
                UpdateUI();
                Debug.Log("Count Blows: " + countBlowsData.count_blows);
            }
        }
    }

    IEnumerator UpdateCountBlows(int countBlows)
    {
        CountBlowsData data = new CountBlowsData { count_blows = countBlows };
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest www = UnityWebRequest.Put(apiUrl + "count_blows/" + userId, jsonData))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
        }
    }

    [System.Serializable]
    private class CoinData
    {
        public float balance;
    }

    [System.Serializable]
    private class CoinUpdateData
    {
        public int coins;
    }

    [System.Serializable]
    private class LevelBossData
    {
        public int level_boss;
    }

    [System.Serializable]
    private class CountBlowsData
    {
        public int count_blows;
    }

    private string GetParameterFromUrl(string url, string parameterName)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(parameterName))
            return null;

        Uri uri = new Uri(url);
        string query = uri.Query;
        var queryParameters = System.Web.HttpUtility.ParseQueryString(query);
        return queryParameters.Get(parameterName);
    }

    private void ShowFloatingText(string message)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingText prefab is not assigned in the inspector!");
            return;
        }

        Debug.Log("Creating floating text");

        // Создаем плавающий текст
        GameObject floatingText = Instantiate(floatingTextPrefab, transform);

        // Устанавливаем позицию текста рядом с balanceText
        RectTransform balanceTextRect = balanceText.GetComponent<RectTransform>();
        RectTransform floatingTextRect = floatingText.GetComponent<RectTransform>();

        // Определяем позицию для плавающего текста
        floatingTextRect.position = new Vector3(balanceTextRect.position.x + balanceTextRect.rect.width + 500, balanceTextRect.position.y, balanceTextRect.position.z);

        FloatingText floatingTextScript = floatingText.GetComponent<FloatingText>();
        if (floatingTextScript != null)
        {
            floatingTextScript.SetText(message);
            Debug.Log("Floating text created with message: " + message);
        }
        else
        {
            Debug.LogError("FloatingText script not found on the prefab!");
        }
    }
}
