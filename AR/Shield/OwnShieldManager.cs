using UnityEngine;
using System.Collections;

public class OwnShieldManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to shield object
    public GameObject shieldObject;

    // Material of the shield
    private Material shieldMaterial;

    // Track if the grow animation has been played
    private bool hasPlayedGrowAnimation = false;
    // Track if the break animation has been played
    private bool hasPlayedBreakAnimation = false;

    // Variables to store original material properties
    private Color originalColor;
    private Color originalEmissionColor;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Get the material of the shield object
        shieldMaterial = shieldObject.GetComponent<Renderer>().material;

        // Store the original material properties using local values
        originalColor = shieldMaterial.color;
        originalEmissionColor = shieldMaterial.GetColor("_EmissionColor");
        originalScale = shieldObject.transform.localScale;
        originalPosition = shieldObject.transform.localPosition; // Use local position reference

        // Subscribe to the PlayerShieldHit event
        gameState.PlayerShieldHitEvent += OnPlayerShieldHit;
        shieldObject.SetActive(false);
        UpdateOwnShield();
    }

    private void Update()
    {
        UpdateOwnShield();
    }

    private void UpdateOwnShield()
    {
        int ownShieldValue = gameState.ShieldValue;

        if (ownShieldValue != 0)
        {
            shieldObject.SetActive(true);
            if (ownShieldValue == 30 && !hasPlayedGrowAnimation)
            {
                StartCoroutine(ShieldGrowAnimation());
            }
            else if (ownShieldValue != 30)
            {
                hasPlayedGrowAnimation = false;
            }
            hasPlayedBreakAnimation = false;
        }
        else
        {
            hasPlayedGrowAnimation = false;
            if (!hasPlayedBreakAnimation)
            {
                BreakShield();
            }
            else
            {
                shieldObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.PlayerShieldHitEvent -= OnPlayerShieldHit;
        }
    }

    private void OnPlayerShieldHit()
    {
        FlashOwnShield();
    }

    private void FlashOwnShield()
    {
        // Start the coroutine to flash the shield
        StartCoroutine(FlashShieldCoroutine());
    }

    private IEnumerator FlashShieldCoroutine()
    {
        // Set the flash color (reddish color for the flash)
        Color flashColor = Color.red;
        Color flashEmission = flashColor * 2.0f; // Increase intensity for emission

        // Enable emission property and set the shield to the flash color
        for (int i = 0; i < 2; i++)
        {
            shieldMaterial.EnableKeyword("_EMISSION");
            shieldMaterial.color = flashColor;
            shieldMaterial.SetColor("_EmissionColor", flashEmission);

            // Wait for a short duration
            yield return new WaitForSeconds(0.1f);

            // Revert back to the original color and emission
            shieldMaterial.color = originalColor;
            shieldMaterial.SetColor("_EmissionColor", originalEmissionColor);
            shieldMaterial.DisableKeyword("_EMISSION");

            // Wait for a short duration before the next flash
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void BreakShield()
    {
        // Start the coroutine to break the shield with an animation
        StartCoroutine(BreakShieldCoroutine());
    }

    private IEnumerator BreakShieldCoroutine()
    {
        // Optionally add particle effects or play sound here

        // Animate the shield breaking (scale down, fade out, flash red, and vibrate)
        float breakDuration = 1.0f;

        // Store the original local position (relative to the player)
        Vector3 originalLocalPosition = shieldObject.transform.localPosition;

        for (float t = 0; t < breakDuration; t += Time.deltaTime)
        {
            float progress = t / breakDuration;

            // Scale down the shield
            shieldObject.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);

            // Fade out the shield color
            Color fadeColor = originalColor;
            fadeColor.a = Mathf.Lerp(1.0f, 0.0f, progress);
            shieldMaterial.color = fadeColor;

            // Flash red color
            shieldMaterial.EnableKeyword("_EMISSION");
            Color flashEmission = Color.red * Mathf.PingPong(t * 10.0f, 1.5f);
            shieldMaterial.SetColor("_EmissionColor", flashEmission);

            // Vibrate shield slightly around the original local position
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f),
                Random.Range(-0.05f, 0.05f));
            shieldObject.transform.localPosition = originalLocalPosition + randomOffset;

            yield return null;
        }

        shieldObject.SetActive(false);

        // Ensure the shield is back to its original state
        ResetShieldProperties();
        hasPlayedBreakAnimation = true;
    }

    private IEnumerator ShieldGrowAnimation()
    {
        // Animate the shield growing and oscillating before settling at original size
        float growDuration = 1.5f;
        float easeBackDuration = 0.5f; // Additional easing duration
        Vector3 enlargedScale = originalScale * 1.5f;

        // Growth and oscillation phase
        for (float t = 0; t < growDuration; t += Time.deltaTime)
        {
            float progress = t / growDuration;
            float oscillation = Mathf.Sin(progress * Mathf.PI * 4) * 0.1f; // Oscillate 4 times

            // Calculate the scale with oscillation
            Vector3 currentScale = Vector3.Lerp(originalScale * 0.5f, enlargedScale, progress) + Vector3.one * oscillation;
            shieldObject.transform.localScale = currentScale;

            yield return null;
        }

        // Easing back to the original size phase
        for (float t = 0; t < easeBackDuration; t += Time.deltaTime)
        {
            float progress = t / easeBackDuration;

            // Smoothly ease back to the original size
            Vector3 currentScale = Vector3.Lerp(enlargedScale, originalScale, progress);
            shieldObject.transform.localScale = currentScale;

            yield return null;
        }

        // Ensure the shield is set to its original scale
        shieldObject.transform.localScale = originalScale;
        hasPlayedGrowAnimation = true;
    }

    private void ResetShieldProperties()
    {
        // Reset material properties to their original values
        shieldMaterial.color = originalColor;
        shieldMaterial.SetColor("_EmissionColor", originalEmissionColor);
        shieldMaterial.DisableKeyword("_EMISSION");

        // Reset shield transform properties using local values
        shieldObject.transform.localScale = originalScale;
        shieldObject.transform.localPosition = originalPosition; // Now using local position
    }
}
