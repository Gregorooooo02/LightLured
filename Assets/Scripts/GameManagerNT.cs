using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerNT : MonoBehaviour
{
    public static GameManagerNT instance;

    public bool hasLantern = false;
    public bool hasNote = false;

    [Header("Time Variables")]
    [SerializeField] private float timeToFinal = 240f; // 4 minutes
    [SerializeField] private float timeToStart = 0f;

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

    [Header("Enemies")]
    [SerializeField] private GameObject[] enemies;

    public bool isPaused = false;
    public bool isGameOver = false;
    public bool isGameWon = false;

    [Header("Barrier")]
    [SerializeField] private GameObject barrierObject;

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

            nightLight.intensity = lightIntensityStart;

            heartbeatAudio.volume = heartbeatVolume;
            heartbeatAudio.loop = true;
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        if (hasLantern) {
            barrierObject.SetActive(false);

            // Activate every enemy
            foreach (GameObject enemy in enemies) {
                enemy.SetActive(true);
            }

            if (timeToStart < timeToFinal) {
                timeToStart += Time.deltaTime;
            }
            else {
                finishObject.SetActive(true);
            }
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

    public IEnumerator DisplayText(GameObject text) {
        text.GetComponent<Animator>().Play("FadeOut");
        yield return new WaitForSeconds(textDuration);
        text.GetComponent<Animator>().Play("FadeIn");
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
    }

    public void QuitGame() {
        Application.Quit();
    }
}
