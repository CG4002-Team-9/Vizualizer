using UnityEngine;
using Vuforia;

public class UserLogManager : MonoBehaviour
{
    public static UserLogManager Instance { get; private set; }
    public TargetSwitchManager TargetSwitchManager;
    public MainCommsManager MainCommsManager;
    public RainColumnManager RainColumnManager;
    public ARManager ARManager;
    public GameObject LoginScreen;
    private bool loggedIn = false;
    private void Awake()
    {
        // Singleton UserLogScreen Manager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        // Set login screen to active
        LoginScreen.SetActive(true);
        VuforiaBehaviour.Instance.enabled = loggedIn;
    }

    public void LogoutActionManager(string action)
    {
        if (action == "logout")
        {
            LogoutUser();
        }
    }

    public void LoginUser(int userId)
    {
        Debug.Log($"Logging in for player {userId}.");
        loggedIn = true;
        GameState.Instance.PlayerID = userId;
        MainCommsManager.SendPlayerLoginMessage();
        TargetSwitchManager.TargetSwitch();
        LoginScreen.SetActive(!loggedIn);
        VuforiaBehaviour.Instance.enabled = loggedIn;
        Debug.Log($"Player {userId} logged in.");
    }

    public void LogoutUser()
    {
        Debug.Log($"Logging out player {GameState.Instance.PlayerID}.");
        loggedIn = false;
        MainCommsManager.SendGameActionMessageNotHit("logout");
        LoginScreen.SetActive(!loggedIn);
        VuforiaBehaviour.Instance.enabled = loggedIn;
        ARManager.DeinitializeGoogleCardboardXR();
        Debug.Log($"Player {GameState.Instance.PlayerID} logged out.");
    }

}