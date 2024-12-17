using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class DissolveController : MonoBehaviour
{
    [SerializeField] public SkinnedMeshRenderer[] skinnedMeshes;
    [SerializeField] public VisualEffect vfx;
    [SerializeField] public float dissolveRate = 0.0125f;
    [SerializeField] public float refreshRate = 0.025f;

    private List<Material> skinnedMaterials = new List<Material>();

    private void Awake() {
        if (skinnedMeshes.Length > 0) {
            for (int i = 0; i < skinnedMeshes.Length; i++) {
                // Get all materials from the skinned mesh renderer
                skinnedMaterials.AddRange(skinnedMeshes[i].materials.ToList());
            }
        }

        if (vfx != null) {
            vfx.Stop();
        }
    }

    public void StartDissolve() {
        StartCoroutine(DissolveCoroutine());
    }

    public IEnumerator DissolveCoroutine() {
        if (vfx != null) {
            vfx.Play();
        }

        if (skinnedMaterials.Count > 0) {
            float counter = 0;
            while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1) {
                counter += dissolveRate;

                for(int i = 0; i < skinnedMaterials.Count; i++) {
                    skinnedMaterials[i].SetFloat("_DissolveAmount", counter);
                }

                yield return new WaitForSeconds(refreshRate);
            }
        }
    }
}
