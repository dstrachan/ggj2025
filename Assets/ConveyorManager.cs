using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [SerializeField] private GameObject glassPrefab;
    [SerializeField] private float initialDelayInSeconds = 1;
    [SerializeField] private float spawnDelayInSeconds = 10;
    [SerializeField] private float forceToApply = 20;

    private float _lastSpawnTime;

    private GameObject _prevGlass;

    private void Awake()
    {
        Debug.Assert(glassPrefab is not null);
    }

    private void Start()
    {
        _lastSpawnTime = Time.time - spawnDelayInSeconds + initialDelayInSeconds;
    }

    private void Update()
    {
        if (Time.time > _lastSpawnTime + spawnDelayInSeconds)
        {
            if (_prevGlass is not null) Destroy(_prevGlass);

            _prevGlass = SpawnGlass();
            _lastSpawnTime = Time.time;
        }
    }

    private GameObject SpawnGlass()
    {
        var glass = Instantiate(glassPrefab, transform);

        var rbody = glass.GetComponent<Rigidbody>();
        rbody.AddForce(glass.transform.up * forceToApply);

        return glass;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.01f);
    }
}