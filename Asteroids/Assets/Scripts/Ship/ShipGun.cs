using UnityEngine;

public class ShipGun : MonoBehaviour
{
    [SerializeField] private ShipMissleFactory missleFactory;
    [SerializeField] private ShipMissleObjectPool misslePool;
    [SerializeField] private float missleSpawnDelay;

    private float counter;

    private void Update()
    {
        counter += Time.deltaTime;
        if (counter >= missleSpawnDelay)
        {
            ShipMissle newMissle = missleFactory.GetNewInstance();

            newMissle.transform.position = transform.position;
            newMissle.transform.rotation = transform.rotation;

            newMissle.ResetVelocity(transform);
            newMissle.SetMisslePool(misslePool);

            counter = 0.0f;
        }
    }
}
