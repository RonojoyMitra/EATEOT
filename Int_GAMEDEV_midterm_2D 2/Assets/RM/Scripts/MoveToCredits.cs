using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToCredits : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(LoadCredits());
    }

    IEnumerator LoadCredits()
    {
        yield return new WaitForSeconds(8);
        SceneManager.LoadScene("CreditsScene");
    }
}
