using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;
using DG.Tweening;
using System.Globalization;

[System.Serializable]
public class UserData
{
    public int energy = 20;
    public long energytime; // Assuming energytime and server_time are Unix timestamps
    public string referral_link;
    public float balance;
    public int level_boss;
    public int count_blows;
    public int weapon;
    public float soft_coins;
    public long server_time;
    public bool new_boss;
}


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

    public GameObject NotConnectedPanel;

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
    //private int activeWeaponIndex = 0; // Индекс текущего активного оружия
    //private int activeCharacterIndex = 0; // Индекс текущего активного персонажа
    //private int inGameCurrency = 0; // Количество внутриигровой валюты
    //private int weaponDamage = 1; // Урон текущего оружия
    //private int coinsPerClick = 1; // Количество монет, зарабатываемых за клик
    //private int inGameCoinsPerClick = 1; // Количество внутриигровых монет, зарабатываемых за клик
    //private int clickCount = 0;
    //private int clickThreshold = 10;
    //private int round = 1;
    public static string userId;
    public static string apiUrl = "https://3f22-195-10-205-80.ngrok-free.app/api/";

    public static UserData userData;
    private static bool gameStarted;

    // Upgrade Costs
    private int[] upgradeCosts = { 30, 100, 300, 500, 1000, 5000 };
    //private int currentUpgradeLevel = 0;

    // Reference to the MoveAroundCircle script
    public MoveAroundCircle moveAroundCircle; // Ссылка на скрипт MoveAroundCircle

    public bool GameStarted
    {
        get => gameStarted;
        set
        {
            gameStarted = value;
            if (!gameStarted)
                NotConnectedPanel.SetActive(true);
            else
                NotConnectedPanel.SetActive(false);
        }
    }

    void Start()
    {
        // Получаем параметры из URL
        string url = Application.absoluteURL;
        userId = GetParameterFromUrl(url, "user_id");
        id.text = userId;
        Debug.Log("User ID: " + userId);

#if DEBUG
        if (string.IsNullOrEmpty(userId))
            userId = "5004782446";
#endif

        if (!string.IsNullOrEmpty(userId))
        {
            StartCoroutine(GetPlayerData("data/"));
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
    }

    public void OnButtonClick()
    {
        // Проверяем, находится ли текущий угол в диапазоне углов
        if (paintCircleSegment.IsAngleInSector(paintCircleSegment.currentAngle, paintCircleSegment.startAngle, paintCircleSegment.endAngle))
        {
            StartCoroutine(GetPlayerData("click/"));


            // Запускаем анимацию атаки
            ggAnimator.SetBool("attack", true);
            activeCharacterAnimator.SetBool("attack", true);
            pletkAnimator.SetBool("attack", true);
            if (activeWeaponAnimator != null)
            {
                activeWeaponAnimator.SetBool("attack", true); // Запускаем анимацию атаки на активном оружии
            }

            //clickCount += weaponDamage; // Увеличиваем количество кликов на значение урона текущего оружия

            //StartCoroutine(UpdateCoins(coinsPerClick)); // Добавляем монеты с каждым кликом
            //StartCoroutine(UpdateCountBlows(clickCount)); // Обновляем количество тапов на сервере

            // Увеличиваем количество внутриигровой валюты
            //inGameCurrency += inGameCoinsPerClick;
            //Debug.Log($"Currency after click: {inGameCurrency}");
            ShowFloatingText($"+{userData.weapon}");

            ShowRandomMessage();

            paintCircleSegment.SetRandomAngles();

            StartCoroutine(ResetAttackParameter());

            AnimateSled();
        }
    }

    private void newBoss()
    {
        ChangeActiveCharacter();
        if (moveAroundCircle != null)
            moveAroundCircle.SetSpeed(1+userData.level_boss*0.5f); 
        paintCircleSegment.SetRotationSpeed((1 + userData.level_boss)*10); 
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
        ClickCountText.text = $"{userData.count_blows} / {(userData.level_boss + 1) * 10}";
        clickSlider.value = (float)userData.count_blows / (userData.level_boss + 1) * 10;
        RoundText.text = $"{userData.level_boss}";
        currencyText.text = $"Currency: {(int)(userData.soft_coins)}"; // Обновляем текст внутриигровой валюты
        Debug.Log($"Update UI called. Current currency: {(int)(userData.soft_coins)}");

        if (userData.weapon < upgradeCosts.Length)
            upgradeButtonText.text = $"Upgrade Weapon ({upgradeCosts[userData.weapon]} Coins)";
        else
            upgradeButtonText.text = "Max Level";


        balanceText.text = "Balance: " + userData.balance;

        // Устанавливаем активного персонажа
        SetActiveCharacter();

    }

    public void OnGetBalanceButtonClicked()
    {
        StartCoroutine(GetPlayerData("data/"));
    }

    public void GetData()
    {
        StartCoroutine(GetPlayerData("data/"));
    }

    IEnumerator GetPlayerData(string apiKey)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl + apiKey + userId))
        {

            Debug.Log(apiUrl + apiKey + userId);
            www.SetRequestHeader("ngrok-skip-browser-warning", "true");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                GameStarted = false;
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                userData = JsonUtility.FromJson<UserData>(jsonResponse);
                EnergyUI.ServerTime = userData.server_time;
                GameStarted = true;

                if (userData.new_boss)
                    newBoss();

                UpdateUI();
            }
        }
    }
    //не нужно
    /*
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
    }*/



    //Это делает сервер теперь
    /*IEnumerator UpdateCoins(int coins)
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
    }*/

    /*
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
    }*/

    /*
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
    }*/

    //Нет потребноси в таком запросе больше
    /*
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
    }*/

    //Это делает сервер теперь
    /*
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
    }*/
    //не нужно
    /*
    [System.Serializable]
    private class LevelBossData
    {
        public int level_boss;
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
    private class CountBlowsData
    {
        public int count_blows;
    }*/

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

    private void SetActiveCharacter()
    {
        int index = userData.level_boss;
        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("Character index out of range!");
            return;
        }

        foreach (var item in characters)
            item.SetActive(false);

        // Активируем нового персонажа
        characters[index].SetActive(true);
        activeCharacterAnimator = characters[index].GetComponent<Animator>();
    }

    private void ChangeActiveCharacter()
    {
        if (userData.level_boss == 0)
            return;

        // Получаем имена старого и нового босса
        string oldBossName = characters[(userData.level_boss-1) % characters.Length].name;
        string newBossName = characters[(userData.level_boss) % characters.Length].name;

        // Обновляем текст в панели
        bossChangeText.text = $"Поздравляем! Босс с именем \"{oldBossName}\" повержен. Вам предстоит победить нового босса с именем \"{newBossName}\".";

        // Активируем панель
        bossChangePanel.SetActive(true);

        foreach (var item in characters)
            item.SetActive(false);

        // Активируем нового персонажа
        characters[userData.level_boss % characters.Length].SetActive(true);
        activeCharacterAnimator = characters[userData.level_boss % characters.Length].GetComponent<Animator>();

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

    private void SetActiveWeapon()
    {
        int index = userData.weapon;
        if (index < 0 || index >= weapons.Length)
        {
            Debug.LogError("Weapon index out of range!");
            return;
        }

        foreach (var item in weapons)
            item.SetActive(false);

        // Активируем новое оружие
        weapons[index].SetActive(true);
        activeWeaponAnimator = weapons[index].GetComponent<Animator>();

        // Обновляем характеристики оружия
        //weaponDamage = index + 1; // Урон соответствует индексу + 1 (первое оружие наносит 1 урон и т.д.)
        //coinsPerClick = index + 1; // Количество монет соответствует индексу + 1
        //inGameCoinsPerClick = index + 1; // Количество внутриигровых монет соответствует индексу + 1

        // Устанавливаем изображение для sled
        if (index < sledImages.Length)
        {
            sled.GetComponent<Image>().sprite = sledImages[index];
        }

        UpdateUI();

    }

    public void OnUpgradeButtonClicked()
    {
        StartCoroutine(GetPlayerData("update/"));
        SetActiveWeapon();
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
        userData.weapon = (userData.weapon + 1) % weapons.Length;
                SetActiveWeapon();
    }
}
