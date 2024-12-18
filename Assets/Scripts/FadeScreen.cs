using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen instance;
    public static bool FadeCompleted;

    [SerializeField] private float speed = 1;
    [SerializeField] private Material ScreenMaterial;
    [SerializeField] private AudioMixer audioMixer;

    private float FadeTime {
        set {
            ScreenMaterial.SetFloat("_FadeTime", value);
            audioMixer.SetFloat("_Ambience", Mathf.Lerp(0, -80, value));
            audioMixer.SetFloat("_Effects", Mathf.Lerp(0, -10, value));
        }
    }

    private void Awake() {
        instance = this;
        FadeIn();
    }

    public void FadeIn() => StartCoroutine(Interpolate(1, 0));
    public void FadeOut() => StartCoroutine(Interpolate(0, 1));

    private IEnumerator Interpolate(float from, float to) {
        float current = from;
        
        FadeCompleted = false;
        for (float t = 0; current != to; t += Time.deltaTime * speed) {
            current = Mathf.Clamp01(Mathf.SmoothStep(from, to, t));
            FadeTime = current;
            yield return null;
        }
        FadeCompleted = true;
    }
}
