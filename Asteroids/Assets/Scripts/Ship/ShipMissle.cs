using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipMissle : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    [SerializeField] private float speed;

    private ShipMissleObjectPool misslePool;
    private Rigidbody2D rb2D;
    private float counter;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        counter = 0.0f;
    }

    private void Update()
    {
        counter += Time.deltaTime;
        if (counter >= lifeTime)
            misslePool.ReturnToPool(this);
    }

    public void ResetVelocity(Transform other)
    {
        rb2D.velocity = other.up * speed;
    }

    public void SetMisslePool(ShipMissleObjectPool misslePoolIn)
    {
        misslePool = misslePoolIn;
    }
}
