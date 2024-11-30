using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool hasLantern = false;
    public bool hasNote = false;

    [Header("Books")]
    public int booksCollected = 0;
    public int totalBooks = 5;
    [SerializeField] private GameObject numberOfBooksText;
    [SerializeField] private GameObject[] books;

    private void Awake() {
        if (instance == null) {
            instance = this;

            numberOfBooksText.SetActive(false);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        
    }

    public void CollectLantern() {
        hasLantern = true;
    }

    public void CollectNote() {
        hasNote = true;
    }

    public IEnumerator CollectBookCoroutine() {
        booksCollected++;
        numberOfBooksText.SetActive(true);

        if (booksCollected >= totalBooks) {
            numberOfBooksText.GetComponent<TMPro.TextMeshProUGUI>().text = "Sacrifice them!";
        }
        else {
            numberOfBooksText.GetComponent<TMPro.TextMeshProUGUI>().text = booksCollected + " / " + totalBooks;    
        }
        yield return new WaitForSeconds(3f);
        numberOfBooksText.SetActive(false);
    }

    public void ResetBooks() {
        booksCollected = 0;
        numberOfBooksText.GetComponent<TMPro.TextMeshProUGUI>().text = booksCollected + " / " + totalBooks;

        foreach (GameObject book in books) {
            book.GetComponent<BookController>().bookGrabbed = false;
            foreach (Transform child in book.transform) {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void ResetGame() {
        hasLantern = false;
        hasNote = false;
        ResetBooks();
    }

    public void QuitGame() {
        Application.Quit();
    }
}
