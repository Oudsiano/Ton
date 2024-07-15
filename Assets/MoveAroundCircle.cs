using UnityEngine;

public class MoveAroundCircle : MonoBehaviour
{
    public Transform center; // Центр круга
    public float radius = 5.0f; // Радиус круга
    public float speed = 1.0f; // Скорость движения

    private float angle = 0.0f;

    void Update()
    {
        // Увеличиваем угол в зависимости от времени и скорости
        angle += speed * Time.deltaTime;

        // Рассчитываем координаты объекта по окружности
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        // Обновляем позицию объекта
        transform.position = new Vector3(x, y, 0) + center.position;

        // Переводим угол в градусы и выводим в консоль
        float angleInDegrees = angle * Mathf.Rad2Deg;
        //Debug.Log("Current Angle: " + angleInDegrees % 360);
    }

    public void IncreaseSpeed(float increment)
    {
        speed += increment;
        Debug.Log("Скорость увеличена на " + increment + ". Новая скорость: " + speed);
    }
}
