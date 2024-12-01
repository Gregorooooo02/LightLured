using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class BookController : MonoBehaviour
{
    public bool bookGrabbed = false;

    [SerializeField] private GameObject enemyToActivate;

    public void GrabBook() {
        StartCoroutine(GameManager.instance.CollectBookCoroutine());
        bookGrabbed = !bookGrabbed;

        if (bookGrabbed) {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);

                if (enemyToActivate != null) {
                    enemyToActivate.SetActive(true);
                }
            }
        }
    }


}
