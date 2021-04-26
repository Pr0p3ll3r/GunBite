using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public Animator transition;
    public float transitionTime = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void LoadScene(int index)
    {      
        StartCoroutine(Load(index));
    }

    IEnumerator Load(int index)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(index);
        Debug.Log("LevelLoader: Loaded level: " + SceneManager.GetSceneByBuildIndex(index).name.ToString());
    }
}
