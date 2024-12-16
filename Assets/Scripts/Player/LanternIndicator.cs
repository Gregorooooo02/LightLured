using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternIndicator : MonoBehaviour
{
    [SerializeField] private Light lanternLight;
    [SerializeField] private Color lightBasicColor;
    [SerializeField] private Color lightDangerColor;

    [SerializeField] private float transitionSpeed = 2f;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            StartCoroutine(GradualColorChange(lanternLight, lightDangerColor));   
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy")) {
            StartCoroutine(GradualColorChange(lanternLight, lightBasicColor));
        }
    }

    private IEnumerator GradualColorChange(Light light, Color targetColor) {
        Color startColor = light.color;
        Debug.Log("Changing light");
        Vector4 startColorVec = new Vector4(startColor.r, startColor.g, startColor.b, startColor.a);
        Vector4 targetColorVec = new Vector4(targetColor.r, targetColor.g, targetColor.b, targetColor.a);

        float elapsedTime = 0f;

        while (elapsedTime < 1f) {
            elapsedTime += Time.deltaTime * transitionSpeed;
            light.color = Color.Lerp(
                new Color(startColorVec.x, startColorVec.y, startColorVec.z, startColorVec.w),
                new Color(targetColorVec.x, targetColorVec.y, targetColorVec.z, targetColorVec.w),
                elapsedTime    
            );
            yield return null;
        }
    }
}
