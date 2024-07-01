using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PaintCircleSegment : MonoBehaviour
{
    public Image circleImage; // Ссылка на изображение круга
    public Transform arrow; // Ссылка на стрелку
    public float interval = 1f; // Интервал изменения углов в секундах

    private float startAngle;
    private float endAngle;
    private float currentAngle;
    private bool isInSector;

    void Start()
    {
        StartCoroutine(ChangeAnglesRoutine());
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
        isInSector = currentAngle >= startAngle && currentAngle <= endAngle;

        if (isInSector && !wasInSector)
        {
            // Объект вошел в сектор
            Debug.Log("Object entered the sector");
        }
        else if (!isInSector && wasInSector)
        {
            // Объект вышел из сектора
            Debug.Log("Object exited the sector");
        }

        // Обновляем закрашивание сектора
        PaintSegment(startAngle, endAngle);
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
        startAngle = Random.Range(0f, 330f); // Убедимся, что конечный угол не выходит за 360 градусов
        endAngle = startAngle + 30f;
    }

    void PaintSegment(float start, float end)
    {
        float fillAmount = (end - start) / 360f;
        circleImage.fillAmount = fillAmount;
        circleImage.fillClockwise = true;
        circleImage.fillOrigin = 2; // Устанавливаем начало заполнения на верхний центр

        // Поворачиваем изображение круга, чтобы оно соответствовало началу заполнения
        circleImage.transform.localEulerAngles = new Vector3(0, 0, -start);
    }
}
