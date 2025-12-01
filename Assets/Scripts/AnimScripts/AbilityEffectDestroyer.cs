using UnityEngine;


// Very basic class used for identifying ability effects, such as bullets and smoke,
// starting coroutines, and deleting effect gomeobjects
public class AbilityEffectDestroyer : MonoBehaviour
{
    public void DeleteMe()
    {
        Destroy(gameObject);
    }
}
