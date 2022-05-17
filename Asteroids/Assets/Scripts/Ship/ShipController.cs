using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
{
    [SerializeField] private float moveForce;
    [SerializeField] private float angularForce;

    private Rigidbody2D rb2D;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = Vector3.zero;
        startRotation = Quaternion.identity;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        rb2D.MoveRotation(rb2D.rotation - input.x * angularForce * Time.deltaTime);

        if (input.y > 0.0f)
            rb2D.AddForce(input.y * transform.up * Time.deltaTime * moveForce, ForceMode2D.Force);
    }

    public void Restart()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}
