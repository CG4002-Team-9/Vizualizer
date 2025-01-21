using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class PredictionsTextManager : MonoBehaviour
{
    public TMP_Text PredictionsText;

    public float fadeDuration = 1.0f;

    public float displayDuration = 2.0f;

    public float predictionThreshold = 0.9f;

    private bool newPrediction = false;

    private void Start()
    {
        PredictionsText.gameObject.SetActive(false);
    }

    public void UpdatePredictions(string Action, float Confidence)
    {
       var validActions = new[] { "basket", "bomb", "volley", "soccer", "reload", "logout", "shield", "bowl" };

       if (validActions.Contains(Action))
        {
          PredictionsText.text = Action + " (" + Confidence.ToString("F3") + ") " + (Confidence >= predictionThreshold ? "OK" : "AGAIN");
          PredictionsText.color = Confidence >= predictionThreshold ? Color.green : Color.red;
          newPrediction = true;
          StartCoroutine(FadeInAndOut());
        }
        else 
        {
          PredictionsText.text = "Invalid Action: " + Action;
          PredictionsText.color = Color.white;
          newPrediction = true;
          StartCoroutine(FadeInAndOut());
        }
    }

    private IEnumerator FadeInAndOut()
    {
        // Fade in
        PredictionsText.gameObject.SetActive(true);
        Color originalColor = PredictionsText.color;
        for (float t = 0.01f; t < fadeDuration; t += Time.deltaTime)
        {
            PredictionsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0, 1, t / fadeDuration));
            yield return null;
        }
        PredictionsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1);

        yield return new WaitForSeconds(displayDuration);

        // Fade out
        while (true)
        {
            newPrediction = false;
            for (float t = 0.01f; t < fadeDuration; t += Time.deltaTime)
            {
                if (newPrediction)
                {
                    yield break;
                }
                PredictionsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1, 0, t / fadeDuration));
                yield return null;
            }
            PredictionsText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            PredictionsText.gameObject.SetActive(false);

            if (!newPrediction)
            {
                break;
            }
        }
    }
}