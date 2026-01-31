using System.Collections;
using UnityEngine;

public class MainMenuBackgroundAnimScript : MonoBehaviour
{
    /*public GameObject b1,
                      b1_s,
                      b1_c,
                      b2,
                      b3,
                      b4,
                      b5;*/
    [SerializeField] private GameObject _menu;

    [SerializeField] private float _baseLerpDur = 2f;
    /*public float b1_multiplier, 
                 b1_s_multiplier, 
                 b1_c_multiplier, 
                 b2_multiplier, 
                 b3_multiplier, 
                 b4_multiplier, 
                 b5_multiplier;*/
    private int doneCounter = 0;

    private void Start()
    {
        StartBackgroundAnim();
    }
    private void StartBackgroundAnim()
    {
        for (int i = 0; i < transform.childCount; i++)
            StartCoroutine(ImageLerpCoro(transform.GetChild(i)));
    }
    private IEnumerator ImageLerpCoro(Transform image) 
    {
        Vector3 initPos = image.localPosition;
        //float m = GetMultiplier(image.name);
        float newDur = _baseLerpDur; //* m;

        for (float timer = 0; timer < newDur; timer += Time.deltaTime)
        {
            float r = timer / newDur;

            image.transform.localPosition = Vector3.Lerp(initPos, Vector3.zero, r);
            yield return null;
        }
        CoroDone();
    }
    private void CoroDone()
    {
        doneCounter++;
        if (doneCounter >= transform.childCount)
            _menu.gameObject.SetActive(true);
    }
    /*private float GetMultiplier(string name)
    {
        switch (name)
        {
            case "1":
                return b1_multiplier;
            case "1-sun":
                return b1_s_multiplier;
            case "1-clouds":
                return b1_c_multiplier;
            case "2":
                return b2_multiplier;
            case "3":
                return b3_multiplier;
            case "4":
                return b4_multiplier;
            case "5":
                return b5_multiplier;
            default:
                return 0;
        }
    }*/
}
