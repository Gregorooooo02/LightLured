using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void FadeIntoScene(int index) {
        StartCoroutine(FadeToSceneAsync(index));
    }

    private IEnumerator FadeToSceneAsync(int i) {
        yield return new WaitUntil(() => FadeScreen.FadeCompleted);
        FadeScreen.instance.FadeOut();
        yield return new WaitUntil(() => FadeScreen.FadeCompleted);
        
        var op = SceneManager.LoadSceneAsync(i);

        while (!op.isDone) {
            yield return null;
        }
    }
}
