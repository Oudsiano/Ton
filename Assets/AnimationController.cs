using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Запуск анимации при старте (если требуется)
        animator.Play("Two");
        Debug.Log("Animation started in Start");
    }

    void Update()
    {
        // Пример: Запуск анимации при нажатии пробела
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.Play("Two");
            Debug.Log("Animation started with Space key");
        }
    }
}
