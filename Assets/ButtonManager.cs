using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using DG.Tweening;

public class ButtonManager : MonoBehaviour
{
    // UI Elements
    [Header("UI Elements")]
    public TMP_Text id;
    public TMP_Text balanceText;
    public TMP_Text ClickCountText;
    public TMP_Text RoundText;
    public TMP_Text currencyText; // Текст для отображения количества валюты
    public Slider clickSlider;
    public TMP_Text attackerText; // Текст для сообщения атакующего
    public TMP_Text targetText; // Текст для сообщения атакуемого
    public TMP_Text bossChangeText; // Текст для отображения сообщений
    public TMP_Text upgradeButtonText; // Текст для кнопки апгрейда оружия
    public Button upgradeButton; // Кнопка апгрейда оружия

    // Game Objects
    [Header("Game Objects")]
    public GameObject floatingTextPrefab; // Префаб плавающего текста
    public PaintCircleSegment paintCircleSegment; // Ссылка на скрипт PaintCircleSegment
    public GameObject sled; // Ссылка на объект Sled
    public GameObject bossChangePanel; // Панель для отображения текста
    public GameObject attackerMessage; // Объект сообщения атакующего
    public GameObject targetMessage; // Объект сообщения атакуемого

    // Characters and Weapons
    [Header("Characters and Weapons")]
    public GameObject[] characters;
    public GameObject[] weapons; // Массив объектов оружия

    // Sled Images
    [Header("Sled Images")]
    public Sprite[] sledImages; // Массив изображений для объекта Sled

    // Animators
    [Header("Animators")]
    private Animator activeWeaponAnimator; // Аниматор на текущем активном оружии
    private Animator ggAnimator; // Аниматор на объекте GG
    private Animator pletkAnimator; // Аниматор на объекте Pletk
    private Animator activeCharacterAnimator; // Аниматор на активном объекте
    private Animator outputFrameAnimator;

    // Game Data
    [Header("Game Data")]
    public string[] AttackerSays = { "Лесбиянки — это женщины с родинками на шее, из которых растут волосы", "Мы не заблудились, мы просто не знаем, где находимся" };
    public string[] AttackindSays = { "Нельзя недооценивать силу халявы", "Для наркотиков, секса и алкоголя есть свое время и место. И это место — колледж.", "Ты что, не знаешь главный закон физики? Все прикольное стоит минимум восемь баксов." };

    // Other Variables
    [Header("Other Variables")]
    private int activeWeaponIndex = 0; // Индекс текущего активного оружия
    private int activeCharacterIndex = 0; // Индекс текущего активного персонажа
    private int inGameCurrency = 0; // Количество внутриигровой валюты
    private int weaponDamage = 1; // Урон текущего оружия
    private int coinsPerClick = 1; // Количество монет, зарабатываемых за клик
    private int inGameCoinsPerClick = 1; // Количество внутриигровых монет, зарабатываемых за клик
    private int clickCount = 0;
    private int clickThreshold = 10;
    private int round = 1;
    private string userId;
    private string apiUrl = "https://1aa0-195-10-205-80.ngrok-free.app/api/";

    public string RefLink="Refka";

    // Upgrade Costs
    private int[] upgradeCosts = { 30, 100, 300, 500, 1000, 5000 };
    private int currentUpgradeLevel = 0;

    // Reference to the MoveAroundCircle script
    public MoveAroundCircle moveAroundCircle; // Ссылка на скрипт MoveAroundCircle

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
        UpdateUpgradeButton();
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
            if (activeWeaponAnimator != null)
            {
                activeWeaponAnimator.SetBool("attack", true); // Запускаем анимацию атаки на активном оружии
            }

            clickCount += weaponDamage; // Увеличиваем количество кликов на значение урона текущего оружия

            StartCoroutine(UpdateCoins(coinsPerClick)); // Добавляем монеты с каждым кликом
            StartCoroutine(UpdateCountBlows(clickCount)); // Обновляем количество тапов на сервере

            // Увеличиваем количество внутриигровой валюты
            inGameCurrency += inGameCoinsPerClick;
            Debug.Log($"Currency after click: {inGameCurrency}");
            UpdateUI();

            // Создаем плавающий текст
            ShowFloatingText($"+{coinsPerClick}");

            // Показываем случайное сообщение
            ShowRandomMessage();

            if (clickCount >= clickThreshold)
            {
                clickCount = 0;
                round++;
                clickThreshold += round * 10; // Увеличиваем количество нажатий с каждым раундом
                StartCoroutine(UpdateLevelBoss(round)); // Обновляем номер босса на сервере
                UpdateUI();

                // Сменяем активного персонажа
                ChangeActiveCharacter();

                // Увеличиваем скорость вращения объекта
                if (moveAroundCircle != null)
                {
                    Debug.Log("Увеличение скорости вращения объекта");
                    moveAroundCircle.IncreaseSpeed(0.5f); // Увеличиваем скорость на 0.5 единиц (можно изменить на желаемое значение)
                }

                // Увеличиваем скорость вращения PaintCircleSegment (если необходимо)
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
        yield return new WaitForSeconds(0.1f); // Ожидаем 0.1 секунды (или другое время в зависимости от продолжительности анимации)
        ggAnimator.SetBool("attack", false); // Отключаем анимацию атаки
        activeCharacterAnimator.SetBool("attack", false); // Отключаем анимацию атаки
        pletkAnimator.SetBool("attack", false);

        if (activeWeaponAnimator != null)
        {
            activeWeaponAnimator.SetBool("attack", false); // Отключаем анимацию атаки на активном оружии
        }
    }

    void UpdateUI()
    {
        ClickCountText.text = $"{clickCount} / {clickThreshold}";
        clickSlider.value = (float)clickCount / clickThreshold;
        RoundText.text = $"{round}";
        currencyText.text = $"Currency: {inGameCurrency}"; // Обновляем текст внутриигровой валюты
        Debug.Log($"Update UI called. Current currency: {inGameCurrency}");
    }

    public void OnGetBalanceButtonClicked()
    {
        StartCoroutine(GetCoins());
    }

    IEnumerator InitializeUserData()
    {
        yield return GetCoins();
        yield return GetReferralLink();
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
    IEnumerator GetReferralLink()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl + "referral/" + userId))
        {
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                balanceText.text = "Error fetching referral link";
            }
            else
            {
                // Получаем реферальную ссылку из ответа
                RefLink = www.downloadHandler.text;
                Debug.Log("Referral Link: " + RefLink);
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

        // Получаем имена старого и нового босса
        string oldBossName = characters[activeCharacterIndex].name;
        string newBossName = characters[nextCharacterIndex].name;

        // Обновляем текст в панели
        bossChangeText.text = $"Поздравляем! Босс с именем \"{oldBossName}\" повержен. Вам предстоит победить нового босса с именем \"{newBossName}\".";

        // Активируем панель
        bossChangePanel.SetActive(true);

        // Деактивируем текущего активного персонажа
        if (activeCharacterAnimator != null)
        {
            characters[activeCharacterIndex].SetActive(false);
        }

        // Активируем нового персонажа
        activeCharacterIndex = nextCharacterIndex;
        characters[activeCharacterIndex].SetActive(true);
        activeCharacterAnimator = characters[activeCharacterIndex].GetComponent<Animator>();

        // Запускаем корутину для деактивации панели через 3 секунды
        StartCoroutine(HideBossChangePanel());
    }

    private IEnumerator HideBossChangePanel()
    {
        yield return new WaitForSeconds(3f); // Время отображения панели
        bossChangePanel.SetActive(false);
    }

    private void ShowRandomMessage()
    {
        // Деактивируем оба сообщения, чтобы начать с чистого листа
        attackerMessage.SetActive(false);
        targetMessage.SetActive(false);

        // Выбираем случайное сообщение для отображения
        int randomChoice = UnityEngine.Random.Range(0, 2);

        if (randomChoice == 0)
        {
            attackerMessage.SetActive(true);
            attackerText.text = AttackerSays[UnityEngine.Random.Range(0, AttackerSays.Length)];
        }
        else
        {
            targetMessage.SetActive(true);
            targetText.text = AttackindSays[UnityEngine.Random.Range(0, AttackindSays.Length)];
        }

        // Запускаем корутину для отключения сообщения через 3 секунды
        StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Время отображения сообщения
        attackerMessage.SetActive(false);
        targetMessage.SetActive(false);
    }

    private void SetActiveWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length)
        {
            Debug.LogError("Weapon index out of range!");
            return;
        }

        // Деактивируем текущее активное оружие
        if (activeWeaponIndex >= 0 && activeWeaponIndex < weapons.Length)
        {
            weapons[activeWeaponIndex].SetActive(false);
        }

        // Активируем новое оружие
        activeWeaponIndex = index;
        weapons[activeWeaponIndex].SetActive(true);
        activeWeaponAnimator = weapons[activeWeaponIndex].GetComponent<Animator>();

        // Обновляем характеристики оружия
        weaponDamage = index + 1; // Урон соответствует индексу + 1 (первое оружие наносит 1 урон и т.д.)
        coinsPerClick = index + 1; // Количество монет соответствует индексу + 1
        inGameCoinsPerClick = index + 1; // Количество внутриигровых монет соответствует индексу + 1

        // Устанавливаем изображение для sled
        if (index < sledImages.Length)
        {
            sled.GetComponent<Image>().sprite = sledImages[index];
        }
    }

    public void OnUpgradeButtonClicked()
    {
        if (currentUpgradeLevel < upgradeCosts.Length && inGameCurrency >= upgradeCosts[currentUpgradeLevel])
        {
            inGameCurrency -= upgradeCosts[currentUpgradeLevel];
            currentUpgradeLevel++;
            SetActiveWeapon(currentUpgradeLevel);
            UpdateUpgradeButton();
            UpdateUI();
        }
    }

    private void UpdateUpgradeButton()
    {
        if (currentUpgradeLevel < upgradeCosts.Length)
        {
            upgradeButtonText.text = $"Upgrade Weapon ({upgradeCosts[currentUpgradeLevel]} Coins)";
        }
        else
        {
            upgradeButtonText.text = "Max Level";
        }
    }

    void Update()
    {
        // Проверяем нажатие пробела для смены оружия
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeWeapon();
        }
    }

    private void ChangeWeapon()
    {
        // Вычисляем индекс следующего оружия
        int nextWeaponIndex = (activeWeaponIndex + 1) % weapons.Length;
        SetActiveWeapon(nextWeaponIndex);
    }
}
