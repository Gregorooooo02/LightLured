using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookController : MonoBehaviour
{
    public bool bookGrabbed = false;

    [SerializeField] private GameObject enemyToActivate;

    public void GrabBook() {
        StartCoroutine(GameManager.instance.CollectBookCoroutine());

        if (GameManager.instance.isFirstBook) {
            StartCoroutine(GameManager.instance.DisplayText(GameManager.instance.monsterText));
        }

        bookGrabbed = !bookGrabbed;

        if (bookGrabbed) {
            gameObject.GetComponent<BoxCollider>().enabled = false;
            foreach (Transform child in transform) {
                child.gameObject.SetActive(false);

                if (enemyToActivate != null) {
                    enemyToActivate.SetActive(true);
                }
            }
        }
    }
}
