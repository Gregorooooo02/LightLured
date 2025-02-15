using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool hasLantern = false;
    public bool hasNote = false;

    [Header("Global Variables")]
    [SerializeField] private FirstPersonController player;
    [SerializeField] private LanternController lanternController;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TextMeshProUGUI sensitivityValue;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private GameObject finishObject;
    [SerializeField] private Light nightLight;
    [SerializeField] private float lightIntensityStart = 0.35f;
    [SerializeField] private float lightIntensityEnd = 0.05f;

    public bool isPaused = false;
    public bool isGameOver = false;
    public bool isGameWon = false;

    [Header("Barrier")]
    [SerializeField] private GameObject barrierObject;

    [Header("Books")]
    public bool shouldBooksWork = false;
    public int booksCollected = 0;
    public int totalBooks = 5;
    [SerializeField] private GameObject numberOfBooksText;
    [SerializeField] private GameObject[] books;
    public bool isFirstBook = true;

    [Header("UI")]
    [SerializeField] public GameObject obtainedLanternText;
    [SerializeField] public GameObject monsterText;
    public float textDuration = 3f;
    public float transparencySpeed = 0.1f;

    [Header("Heartbeat Audio")]
    [SerializeField] private AudioSource heartbeatAudio;
    [SerializeField] private float heartbeatVolume = 0.2f;
    [SerializeField] private AudioClip[] heartbeats = default;

    private void Awake() {
        if (instance == null) {
            instance = this;

            numberOfBooksText.SetActive(false);
            nightLight.intensity = lightIntensityStart;

            heartbeatAudio.volume = heartbeatVolume;
            heartbeatAudio.loop = true;
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        if (booksCollected >= 1) {
            isFirstBook = false;

            if (!heartbeatAudio.isPlaying) {
                heartbeatAudio.clip = heartbeats[booksCollected - 1];
                heartbeatAudio.Play();
            }
        }

        if (hasLantern) {
            barrierObject.SetActive(false);
        }

        if (booksCollected >= totalBooks) {
            finishObject.SetActive(true);
        }

        if (Input.GetKeyDown(pauseKey) && !isPaused) {
            PauseGame();
        }

        string sensitivity = player.lookSpeedX.ToString();
        // Cut the string to only show the first 3 characters
        sensitivity = sensitivity.Substring(0, 3);
        sensitivityValue.text = sensitivity;
    }

    public void CollectLantern() {
        hasLantern = true;
        shouldBooksWork = true;
        LampScare.instance.StartCoroutine(LampScare.instance.BreakLight());
    }

    public void CollectNote() {
        hasNote = true;
    }

    public IEnumerator ChangeLightIntensity() {
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime / 2;
            nightLight.intensity = Mathf.Lerp(lightIntensityStart, lightIntensityEnd, t);
            yield return null;
        }
    }

    public IEnumerator CollectBookCoroutine() {
        booksCollected++;
        numberOfBooksText.SetActive(true);

        if (booksCollected >= totalBooks) {
            numberOfBooksText.GetComponent<TMPro.TextMeshProUGUI>().text = "Burn them all!";
        }
        else {
            numberOfBooksText.GetComponent<TMPro.TextMeshProUGUI>().text = booksCollected + " / " + totalBooks;    
        }
        yield return new WaitForSeconds(3f);
        numberOfBooksText.SetActive(false);
    }

    public IEnumerator DisplayText(GameObject text) {
        text.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(textDuration);
        text.GetComponent<Animator>().Play("FadeIn");
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

    public void PauseGame() {
        isPaused = true;
        if (isPaused) {
            pauseMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.CanMove = false;
            lanternController.turnOnOffLantern = false;
            Time.timeScale = 0;
        }
    }

    public void ResumeGame() {
        isPaused = false;
        if (!isPaused) {
            pauseMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            player.CanMove = true;
            lanternController.turnOnOffLantern = true;
            Time.timeScale = 1;
        }
    }

    public void BackToMenu() {
        Time.timeScale = 1;
        LevelManager.instance.FadeIntoScene(0);
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
