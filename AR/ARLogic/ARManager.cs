using Google.XR.Cardboard;
using UnityEngine;
using TMPro;

public class ARManager : MonoBehaviour
{
    private Google.XR.Cardboard.XRLoader cardboardLoader;
    public GameObject[] ToggleObjects;
    public GameObject[] HideObjects;
    public TextMeshProUGUI CalibrationStatusText;

    private bool CalibrationStatus;

    public static ARManager Instance { get; private set; }

    private void Start()
    {
        //Singleton ARManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        
        cardboardLoader = ScriptableObject.CreateInstance<Google.XR.Cardboard.XRLoader>();
        CalibrationStatus = false;
        updateCalibrationText();
    }

    public void InitializeGoogleCardboardXR()
    {
        Debug.Log("Initializing Google Cardboard XR");
        if  (!CalibrationStatus)
        {
            CalibrationStatusText.text = "Please Calibrate First";
            return;
        }
        // Initialize the Google Cardboard XR if on Android / iOS
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // set screen brightness to max
            Screen.brightness = 1;

            Application.targetFrameRate = 120;
            // Hide the HideObjects
            foreach (GameObject hideObject in HideObjects)
            {
                hideObject?.SetActive(false);
            }
            
            // Disable the ToggleObjects
            foreach (GameObject toggleObject in ToggleObjects)
            {
                toggleObject?.SetActive(false);
            }

            cardboardLoader.Initialize();
            cardboardLoader.Start();
            Debug.Log("Google Cardboard XR initialized");
        }
        else
        {
            Debug.Log("Google Cardboard XR not initialized. Platform not supported.");
        }
    }

    public void DeinitializeGoogleCardboardXR()
    {
        Debug.Log("Deinitializing Google Cardboard XR");

        // Stop the Google Cardboard XR if on Android / iOS
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // set screen brightness to default
            Screen.brightness = 0.5f;
            
            Application.targetFrameRate = 30;
            // Re-enable the ToggleObjects
            foreach (GameObject toggleObject in ToggleObjects)
            {
                toggleObject?.SetActive(true);
            }
        
            cardboardLoader.Stop();
            cardboardLoader.Deinitialize();
            Debug.Log("Google Cardboard XR deinitialized");  
        }
    }

    private void updateCalibrationText()
    {
        CalibrationStatusText.text = CalibrationStatus ? "Calibrated" : "Not Calibrated";
        CalibrationStatusText.color = CalibrationStatus ? Color.green : Color.red;
    }

    public void updateCalibrationStatus(bool status)
    {
        CalibrationStatus = status;
        updateCalibrationText();
    }

    public void Update()
    {
        // Deinitialize Google Cardboard XR if the close button is pressed
        if (Google.XR.Cardboard.Api.IsCloseButtonPressed)
        {
          DeinitializeGoogleCardboardXR();
        }
    }
}