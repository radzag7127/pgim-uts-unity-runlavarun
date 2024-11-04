using UnityEngine;

public class RisingLava : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 1f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private LayerMask playerLayer;

    private float currentHeight;
    private GameSession gameSession;

    void Start()
    {
        currentHeight = transform.position.y;
        gameSession = FindObjectOfType<GameSession>();
    }

    void Update()
    {
        if (currentHeight < maxHeight)
        {
            transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
            currentHeight += riseSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log("Player entered lava trigger");
            // Notify GameSession to process player death
            gameSession.ProcessPlayerDeath();
        }
    }
}
