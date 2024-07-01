using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingText : MonoBehaviour
{
    public float floatDistance = 50f; // Расстояние, на которое поднимается текст
    public float horizontalDistance = 30f; // Максимальное расстояние по горизонтали
    public float duration = 1f; // Продолжительность анимации

    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        if (text == null)
        {
            Debug.LogError("TMP_Text component not found!");
            return;
        }
    }

    public void SetText(string message)
    {
        if (text == null)
        {
            Debug.LogError("TMP_Text component is not assigned!");
            return;
        }
        text.text = message;

        // Случайное значение для движения по горизонтали
        float randomHorizontalOffset = Random.Range(-horizontalDistance, horizontalDistance);

        // Анимация движения вверх и вправо/влево и исчезновения
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 endPosition = new Vector3(rectTransform.position.x + randomHorizontalOffset, rectTransform.position.y + floatDistance, rectTransform.position.z);

        rectTransform.DOMove(endPosition, duration).SetEase(Ease.OutQuad);
        text.DOFade(0, duration).SetEase(Ease.OutQuad).OnComplete(() => Destroy(gameObject));
    }
}
