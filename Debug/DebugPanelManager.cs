using UnityEngine;

public class DebugPanelManager : MonoBehaviour
{
    public GameObject DebugPanel;

    void Start()
    {
        if (DebugPanel != null)
        {
            DebugPanel.SetActive(false);
            Debug.Log("Debug Panel is now " + (DebugPanel.activeSelf ? "visible" : "hidden"));
        }
    }

    public void ShowDebugPanel()
    {
        if (DebugPanel != null)
        {
            DebugPanel.SetActive(true);
        }
    }

    public void HideDebugPanel()
    {
        if (DebugPanel != null)
        {
            DebugPanel.SetActive(false);
        }
    }

    public void ToggleDebugPanel()
    {
        if (DebugPanel != null)
        {
            DebugPanel.SetActive(!DebugPanel.activeSelf);
            Debug.Log("Debug Panel is now " + (DebugPanel.activeSelf ? "visible" : "hidden"));
        }
    }
}