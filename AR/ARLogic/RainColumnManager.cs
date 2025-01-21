using UnityEngine;

public class RainColumnManager : MonoBehaviour
{
    private GameObject[] rainColumns;
    private GameObject[] enemyRainColumns;

    public static RainColumnManager Instance { get; private set; }

    private void Start()
    {
        //Singleton RainColumnManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void DestroyAllRainColumns()
    {
        rainColumns = GameObject.FindGameObjectsWithTag("Waterbomb");
        enemyRainColumns = GameObject.FindGameObjectsWithTag("EnemyWaterbomb");

        // Destroy all RainColumn GameObjects
        foreach (GameObject rainColumn in rainColumns)
        {
            Destroy(rainColumn);
        }

        // Destroy all EnemyRainColumn GameObjects
        foreach (GameObject enemyRainColumn in enemyRainColumns)
        {
            Destroy(enemyRainColumn);
        }
    }

}