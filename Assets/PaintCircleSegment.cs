using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PaintCircleSegment : MonoBehaviour
{
    public Image circleImage; // Ссылка на изображение круга
    public Transform arrow; // Ссылка на стрелку
    public Transform startIndicator; // Объект для отображения начала сектора
    public Transform endIndicator; // Объект для отображения конца сектора
    public float interval = 1000f; // Интервал изменения углов в секундах
    public float radius = 180f; // Радиус окружности
    public float rotationSpeed = 10f; // Начальная скорость вращения

    public float startAngle;
    public float endAngle;
    public float currentAngle;
    public bool isInSector;
    private Animator ggAnimator;

    public void Start()
    {
        StartCoroutine(ChangeAnglesRoutine());
        StartCoroutine(LogCurrentAngleRoutine());
    }

    public void Update()
    {
        // Вращаем объект
        arrow.RotateAround(circleImage.transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);

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

        // Обновляем закрашивание сектора и индикаторы
        PaintSegment(startAngle, endAngle);
        UpdateIndicators();
    }

    public IEnumerator ChangeAnglesRoutine()
    {
        while (true)
        {
            SetRandomAngles();
            yield return new WaitForSeconds(interval);
        }
    }

    public void SetRandomAngles()
    {
        startAngle = Random.Range(0f, 360f);
        endAngle = (startAngle + 30f) % 360f;
    }

    public bool IsAngleInSector(float angle, float start, float end)
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

    public void PaintSegment(float start, float end)
    {
        float fillAmount = (end - start + 360) % 360 / 360f;
        circleImage.fillAmount = fillAmount;
        circleImage.fillClockwise = false;
        circleImage.fillOrigin = (int)Image.Origin360.Right; // Устанавливаем начало заполнения на верхний центр

        // Поворачиваем изображение круга, чтобы оно соответствовало началу заполнения
        circleImage.transform.localEulerAngles = new Vector3(0, 0, start);
    }

    public void UpdateIndicators()
    {
        // Рассчитываем позиции индикаторов
        Vector3 startPos = CalculatePositionOnCircle(startAngle);
        Vector3 endPos = CalculatePositionOnCircle(endAngle);

        // Устанавливаем позиции индикаторов
        startIndicator.position = circleImage.transform.position + startPos;
        endIndicator.position = circleImage.transform.position + endPos;
    }

    public Vector3 CalculatePositionOnCircle(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radian) * radius;
        float y = Mathf.Sin(radian) * radius;
        return new Vector3(x, y, 0);
    }

    public IEnumerator LogCurrentAngleRoutine()
    {
        while (true)
        {
            // Debug.Log($"Current Angle: {currentAngle}, Sector: {startAngle} to {endAngle}");
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void IncreaseRotationSpeed(float amount)
    {
        rotationSpeed += amount;
    }
}
