using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UI_bottombuttons : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button btn1;
    [SerializeField] private Button btn2;
    [SerializeField] private Button btn3;

    [Header("Screen1")]
    [SerializeField] private GameObject RefLinkScreenUI;
    [SerializeField] private Button closeBtn;
    [SerializeField] private GameObject backBtn;
    [SerializeField] private TMPro.TMP_InputField selectableText;
    [SerializeField] private Button btnCopy;


    [Header("Other")]
    [SerializeField] private GameObject screen2;
    [SerializeField] private GameObject screen3;

    private int currentButtonIndex = -1;
    private ButtonManager buttonManager;

    private void Awake()
    {
        btn1.onClick.AddListener(OnClickButton1);
        btn2.onClick.AddListener(OnClickButton2);
        btn3.onClick.AddListener(OnClickButton3);
        closeBtn.onClick.AddListener(OnClickClose);
        btnCopy.onClick.AddListener(OnClickCopy);

        buttonManager = FindObjectOfType<ButtonManager>();

        SetState(0);
    }

    private void OnClickCopy()
    {
        GUIUtility.systemCopyBuffer = buttonManager.RefLink;
    }

    private void OnClickButton1()
    {
        SetState(1);
        if (buttonManager != null)
        {
            selectableText.text = buttonManager.RefLink;
        }
    }

    private void OnClickButton2()
    {
        SetState(2);
    }

    private void OnClickButton3()
    {
        SetState(3);
    }

    private void OnClickClose()
    {
        SetState(0);
    }

    public void SetState(int state)
    {
        // Сначала деактивируем все элементы
        if (RefLinkScreenUI != null) RefLinkScreenUI.SetActive(false);
        if (screen2 != null) screen2.SetActive(false);
        if (screen3 != null) screen3.SetActive(false);
        if (backBtn != null) backBtn.SetActive(false);

        switch (state)
        {
            case 0:
                // Все элементы уже деактивированы выше
                break;
            case 1:
                if (RefLinkScreenUI != null) RefLinkScreenUI.SetActive(true);
                if (backBtn != null) MoveBackButton(btn1.transform.position);
                break;
            case 2:
                if (screen2 != null) screen2.SetActive(true);
                if (backBtn != null) MoveBackButton(btn2.transform.position);
                break;
            case 3:
                if (screen3 != null) screen3.SetActive(true);
                if (backBtn != null) MoveBackButton(btn3.transform.position);
                break;
        }
    }


    private void MoveBackButton(Vector3 targetPosition)
    {
        backBtn.SetActive(true);
        backBtn.transform.DOMoveX(targetPosition.x, 0.5f).SetEase(Ease.OutCubic);
    }
}
