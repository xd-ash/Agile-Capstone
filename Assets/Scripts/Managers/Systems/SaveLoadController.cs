using UnityEngine;

public class SaveLoadController : MonoBehaviour
{
    public static SaveLoadController instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SaveLoadScript.SaveGame?.Invoke();

        if (Input.GetKeyDown(KeyCode.F8))
            SaveLoadScript.LoadGame?.Invoke();
    }
}
