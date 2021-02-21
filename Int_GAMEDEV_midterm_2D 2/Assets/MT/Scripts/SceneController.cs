using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    static SceneController instance;

    Animator animator;
    Image blackout;

    [SerializeField]
    int loadScreenIndex;

    void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(gameObject);
        animator = GetComponent<Animator>();
        blackout = GetComponentInChildren<Image>();
    }

    public static void LoadScene(int sceneBuildIndex)
    {
        instance.GoToScene(sceneBuildIndex);
    }
    public static void LoadScene(string sceneName)
    {
        instance.GoToScene(sceneName);
    }

    void GoToScene(int sceneBuildIndex)
    {
        StartCoroutine(GoToSceneCoroutine(sceneBuildIndex));
    }
    void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneCoroutine(sceneName));
    }

    IEnumerator GoToSceneCoroutine(int sceneBuildIndex)
    {
        yield return new WaitForEndOfFrame();   // WaitForEndOfFrame to allow the coroutine to breath

        // Fade out and wait for the opaque animation to be playing
        animator.SetTrigger("Out");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Opaque"));

        // Load the loading screen scene
        SceneManager.LoadScene(loadScreenIndex);
        yield return new WaitForEndOfFrame();

        // Start loading the target scene
        AsyncOperation l = SceneManager.LoadSceneAsync(sceneBuildIndex);
        yield return new WaitForEndOfFrame();
        Image fill = GameObject.Find("Fill") != null ? GameObject.Find("Fill").GetComponent<Image>() : null;

        // Wait for the scene load to be done
        while (l.isDone == false)
        {
            fill.fillAmount= l.progress;
            yield return new WaitForEndOfFrame();
        }

        // Fade in
        animator.SetTrigger("In");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Transparent"));
    }
    IEnumerator GoToSceneCoroutine(string sceneName)
    {
        yield return new WaitForEndOfFrame();   // WaitForEndOfFrame to allow the coroutine to breath

        // Fade out and wait for the opaque animation to be playing
        animator.SetTrigger("Out");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Opaque"));

        // Load the loading screen scene
        SceneManager.LoadScene(loadScreenIndex);
        yield return new WaitForEndOfFrame();

        // Start loading the target scene
        AsyncOperation l = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitForEndOfFrame();
        Image fill = GameObject.Find("Fill") != null ? GameObject.Find("Fill").GetComponent<Image>() : null;

        // Wait for the scene load to be done
        while (l.isDone == false)
        {
            fill.fillAmount = l.progress;
            yield return new WaitForEndOfFrame();
        }

        // Fade in
        animator.SetTrigger("In");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Transparent"));
    }
}
