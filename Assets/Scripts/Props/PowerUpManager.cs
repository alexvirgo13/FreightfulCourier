using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] private PowerUpScriptableObject powerUpData;

    MovementController movementController;
    SpriteRenderer powerUpSpriteRenderer;
    float timeLeft;

    private void Start()
    {
        movementController = FindObjectsByType<MovementController>()[0];
        powerUpSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        timeLeft = powerUpData.TimeLimit;
    }

    private void Update()
    {
        CountDownPowerUpTime();
    }

    /// <summary>
    /// Count down the time left for the power-up effect and reset it when it reaches zero.
    /// </summary>
    private void CountDownPowerUpTime()
    {
        if (!powerUpSpriteRenderer.enabled)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                if (timeLeft <= 0) movementController.DeactivatePowerUp(powerUpData);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && powerUpSpriteRenderer.enabled)
        {
            powerUpSpriteRenderer.enabled = false; // Hide the power-up sprite
            movementController.ApplyPowerUp(powerUpData); // Apply the power-up effect to the player
        }
    }
}
