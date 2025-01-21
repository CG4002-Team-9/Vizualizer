using UnityEngine;

public class PlayerIDTextManager : MonoBehaviour
{
    public GameObject PlayerIDText;

    void Start()
    {
        if (PlayerIDText != null)
        {
            PlayerIDText.SetActive(false);
        }
    }

    public void Update()
    {
        if (GameState.Instance.EnemyActive)
        {
            if (PlayerIDText != null)
            {
                PlayerIDText.SetActive(true);
            }
        }
        else
        {
            if (PlayerIDText != null)
            {
                PlayerIDText.SetActive(false);
            }
        }
    }
}