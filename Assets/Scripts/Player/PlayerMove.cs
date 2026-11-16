using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float movingSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _movment;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _movment.x = Input.GetAxisRaw("Horizontal");
        _movment.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _movment * movingSpeed;
    }
}
