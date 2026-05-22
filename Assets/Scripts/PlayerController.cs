using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;

    Vector2 movement;

    void Update()
    {
        // Используем GetAxisRaw для четкого отклика (0 или 1)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }

        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        // ПРАВКА ДЛЯ ПЛАВНОСТИ: используем velocity вместо MovePosition
        // Это позволит параметру Interpolate в Rigidbody2D работать на 100%
        rb.velocity = movement.normalized * moveSpeed;
    }
}