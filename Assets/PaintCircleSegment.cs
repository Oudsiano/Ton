using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PaintCircleSegment : MonoBehaviour
{
    public Image circleImage; // Ссылка на изображение круга
    public Transform arrow; // Ссылка на стрелку
    public Transform startIndicator; // Объект для отображения начала сектора
    public Transform endIndicator; // Объект для отображения конца сектора
    public float interval = 1f; // Интервал изменения углов в секундах
    public float radius = 180f; // Радиус окружности

    private float startAngle;
    private float endAngle;
    private float currentAngle;
    private bool isInSector;

    void Start()
    {
        StartCoroutine(ChangeAnglesRoutine());
        StartCoroutine(LogCurrentAngleRoutine());
    }

    void Update()
    {
        // Рассчитываем текущий угол на основе позиции стрелки
        Vector3 direction = arrow.position - circleImage.transform.position;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (currentAngle < 0)
        {
            currentAngle += 360;
        }

        // Проверяем, попадает ли текущий угол в заданный диапазон
        bool wasInSector = isInSector;
        isInSector = IsAngleInSector(currentAngle, startAngle, endAngle);

        if (isInSector && !wasInSector)
        {
            // Объект вошел в сектор
            Debug.Log($"Object entered the sector at angle: {currentAngle}, Sector: {startAngle} to {endAngle}");
        }
        else if (!isInSector && wasInSector)
        {
            // Объект вышел из сектора
            Debug.Log($"Object exited the sector at angle: {currentAngle}, Sector: {startAngle} to {endAngle}");
        }

        // Обновляем закрашивание сектора и индикаторы
        PaintSegment(startAngle, endAngle);
        UpdateIndicators();
    }

    IEnumerator ChangeAnglesRoutine()
    {
        while (true)
        {
            SetRandomAngles();
            yield return new WaitForSeconds(interval);
        }
    }

    void SetRandomAngles()
    {
        startAngle = Random.Range(0f, 360f);
        endAngle = (startAngle + 30f) % 360f;
        Debug.Log($"New Sector: Start = {startAngle}, End = {endAngle}");
    }

    bool IsAngleInSector(float angle, float start, float end)
    {
        if (start < end)
        {
            return angle >= start && angle <= end;
        }
        else
        {
            // Обрабатываем случай, когда сектор пересекает ноль
            return angle >= start || angle <= end;
        }
    }

    void PaintSegment(float start, float end)
    {
        float fillAmount = (end - start + 360) % 360 / 360f;
        circleImage.fillAmount = fillAmount;
        circleImage.fillClockwise = true;
        circleImage.fillOrigin = 2; // Устанавливаем начало заполнения на верхний центр

        // Поворачиваем изображение круга, чтобы оно соответствовало началу заполнения
        circleImage.transform.localEulerAngles = new Vector3(0, 0, -start);
    }

    void UpdateIndicators()
    {
        // Рассчитываем позиции индикаторов
        Vector3 startPos = CalculatePositionOnCircle(startAngle);
        Vector3 endPos = CalculatePositionOnCircle(endAngle);

        // Устанавливаем позиции индикаторов
        startIndicator.position = circleImage.transform.position + startPos;
        endIndicator.position = circleImage.transform.position + endPos;
    }

    Vector3 CalculatePositionOnCircle(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radian) * radius;
        float y = Mathf.Sin(radian) * radius;
        return new Vector3(x, y, 0);
    }

    IEnumerator LogCurrentAngleRoutine()
    {
        while (true)
        {
            Debug.Log($"Current Angle: {currentAngle}, Sector: {startAngle} to {endAngle}");
            yield return new WaitForSeconds(0.5f);
        }
    }
}
