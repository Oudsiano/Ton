using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using DG.Tweening; // Добавьте это пространство имен для использования DoTween

public class ButtonManager : MonoBehaviour
{
    public TMP_Text id;
    public TMP_Text balanceText;
    public TMP_Text ClickCountText;
    public TMP_Text RoundText;
    public Slider clickSlider;
    public GameObject floatingTextPrefab; // Префаб плавающего текста
    public PaintCircleSegment paintCircleSegment; // Ссылка на скрипт PaintCircleSegment
    public GameObject sled; // Ссылка на объект Sled

    public GameObject[] characters;

    private Animator outputFrameAnimator;
    private Animator ggAnimator; // Аниматор на объекте GG
    private Animator pletkAnimator; // Аниматор на объекте Pletk
    private Animator activeCharacterAnimator; // Аниматор на активном объекте
    private int activeCharacterIndex = 0; // Индекс текущего активного персонажа

    private int clickCount = 0;
    private int clickThreshold = 1;
    private int round = 1;
    private int coinsPerClick = 1;
    private string userId;
    private string apiUrl = "https://1aa0-195-10-205-80.ngrok-free.app/api/";

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

        // Получаем компоненты аниматора на объектах GG и Pletk
        ggAnimator = GameObject.Find("GG").GetComponent<Animator>();
        pletkAnimator = GameObject.Find("Pletk").GetComponent<Animator>();

        // Устанавливаем активного персонажа
        SetActiveCharacter(0);

        UpdateUI();
    }

    public void OnButtonClick()
    {
        // Проверяем, находится ли текущий угол в диапазоне углов
        if (paintCircleSegment.IsAngleInSector(paintCircleSegment.currentAngle, paintCircleSegment.startAngle, paintCircleSegment.endAngle))
        {
            // Запускаем анимацию атаки
            ggAnimator.SetBool("attack", true);
            activeCharacterAnimator.SetBool("attack", true);
            pletkAnimator.SetBool("attack", true);
            clickCount++;
            StartCoroutine(UpdateCoins(coinsPerClick)); // Добавляем монеты с каждым кликом
            StartCoroutine(UpdateCountBlows(clickCount)); // Обновляем количество тапов на сервере
            UpdateUI();

            // Создаем плавающий текст
            ShowFloatingText($"+{coinsPerClick}");

            if (clickCount >= clickThreshold)
            {
                clickCount = 0;
                round++;
                clickThreshold += round * 1; // Увеличиваем количество нажатий с каждым раундом
                StartCoroutine(UpdateLevelBoss(round)); // Обновляем номер босса на сервере
                UpdateUI();

                // Сменяем активного персонажа
                ChangeActiveCharacter();

                // Увеличиваем скорость вращения
                paintCircleSegment.IncreaseRotationSpeed(10f); // Увеличиваем скорость на 10 единиц (можно изменить на желаемое значение)
            }

            // Изменяем диапазон углов после успешного клика
            paintCircleSegment.SetRandomAngles();

            // Запускаем сброс параметра через одну секунду
            StartCoroutine(ResetAttackParameter());

            // Запускаем анимацию Sled
            AnimateSled();
        }
    }

    IEnumerator ResetAttackParameter()
    {
        yield return new WaitForSeconds(0.1f); // Ожидаем одну секунду (или другое время в зависимости от продолжительности анимации)
        ggAnimator.SetBool("attack", false); // Отключаем анимацию атаки
        activeCharacterAnimator.SetBool("attack", false); // Отключаем анимацию атаки
        pletkAnimator.SetBool("attack", false);
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

    private void AnimateSled()
    {
        if (sled == null)
        {
            Debug.LogError("Sled GameObject is not assigned in the inspector!");
            return;
        }

        // Активируем объект Sled
        sled.SetActive(true);

        // Получаем или добавляем CanvasGroup компонент
        CanvasGroup canvasGroup = sled.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = sled.AddComponent<CanvasGroup>();
        }

        // Устанавливаем начальную прозрачность и масштаб
        canvasGroup.alpha = 1;
        sled.transform.localScale = new Vector3(0.283898503f, 0.283898503f, 0.283898503f);

        // Запускаем анимацию увеличения, уменьшения и плавного исчезновения
        sled.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.2f) // Увеличиваем объект
            .OnComplete(() =>
            {
                Debug.Log("Увеличение завершено");
                sled.transform.DOScale(new Vector3(0.283898503f, 0.283898503f, 0.283898503f), 0.2f) // Уменьшаем объект
                    .OnComplete(() =>
                    {
                        Debug.Log("Уменьшение завершено");
                        canvasGroup.DOFade(0, 1f) // Плавно исчезаем
                            .OnComplete(() =>
                            {
                                Debug.Log("Исчезновение завершено");
                                sled.SetActive(false); // Деактивируем объект после анимации
                            });
                    });
            });
    }

    private void SetActiveCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("Character index out of range!");
            return;
        }

        // Деактивируем текущего активного персонажа
        if (activeCharacterAnimator != null)
        {
            characters[activeCharacterIndex].SetActive(false);
        }

        // Активируем нового персонажа
        activeCharacterIndex = index;
        characters[activeCharacterIndex].SetActive(true);
        activeCharacterAnimator = characters[activeCharacterIndex].GetComponent<Animator>();
    }

    private void ChangeActiveCharacter()
    {
        // Вычисляем индекс следующего персонажа
        int nextCharacterIndex = (activeCharacterIndex + 1) % characters.Length;
        SetActiveCharacter(nextCharacterIndex);
    }
}
