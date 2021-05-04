using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    static SceneController instance;    // Singleton instance

    Animator animator;  // The animator which is used to control the transition effects
    Image blackout;     // The blackout image used for fading

    [SerializeField]
    [Tooltip("The index of the loading screen.")]
    int loadScreenIndex;

    void Awake()
    {
        if (instance != null) Destroy(gameObject);  // If there is already a SceneController we destroy ourself
        instance = this;    // Set the instance to this instance of SceneController
        DontDestroyOnLoad(gameObject);  // Mark this object to not destroy on load
        animator = GetComponent<Animator>();    // Get reference to the animator
        blackout = GetComponentInChildren<Image>(); // Get reference to the blackout image
    }

    private void Update()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Asks the current SceneController instance to load a given scene.
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to load.</param>
    public static void LoadScene(int sceneBuildIndex)
    {
        // If there is not an instance of SceneController we throw an error.
        if (instance == null)
        {
            Debug.LogError("Tried to load scene with index " + sceneBuildIndex + " but no SceneController was found in the current scene.");
            return;
        }
        // Call the instance's GoToScene method
        instance.GoToScene(sceneBuildIndex);
    }
    /// <summary>
    /// Asks the current SceneController instance to load a given scene.
    /// </summary>
    /// <param name="sceneName">The name or path to the scene to load.</param>
    public static void LoadScene(string sceneName)
    {
        // If there is not an instance of SceneController we throw an error.
        if (instance == null)
        {
            Debug.LogError("Tried to load scene " + sceneName + " but no SceneController was found in the current scene.");
            return;
        }
        // Call the instance's GoToScene method
        instance.GoToScene(sceneName);
    }

    /// <summary>
    /// Starts the coroutine to load a scene.
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to load.</param>
    void GoToScene(int sceneBuildIndex)
    {
        StartCoroutine(GoToSceneCoroutine(sceneBuildIndex));
    }
    /// <summary>
    /// Starts the coroutine to load a scene.
    /// </summary>
    /// <param name="sceneName">The name or path to the scene to load.</param>
    void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneCoroutine(sceneName));
    }

    /// <summary>
    /// Loads a scene asynchronously and uses a loading scene to show the asynch progress.
    /// </summary>
    /// <param name="sceneBuildIndex">The build index of the scene to load.</param>
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
    /// <summary>
    /// Loads a scene asynchronously and uses a loading scene to show the asynch progress.
    /// </summary>
    /// <param name="sceneName">The name or path to the scene to load.</param>
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
