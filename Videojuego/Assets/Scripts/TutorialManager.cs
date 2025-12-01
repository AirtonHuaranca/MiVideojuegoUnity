using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del tutorial")]
    public GameObject panelWelcome;
    public GameObject panelLinea1;
    public GameObject panelLinea2;

    [Header("Contador de balones")]
    public GameObject ballCounterRoot;
    public TMP_Text ballCounterText;
    public int totalBalls = 10;

    [Header("Panel Final")]
    public GameObject panelFinal;
    public string nextSceneName = "Nivel3";

    [Header("Sistemas")]
    public LockMouse mouseLocker;

    private int kickedCount = 0;

    private void Start()
    {
        kickedCount = 0;
        UpdateBallCounter();

        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(false);

        if (panelLinea1 != null) panelLinea1.SetActive(false);
        if (panelLinea2 != null) panelLinea2.SetActive(false);

        if (panelWelcome != null)
            panelWelcome.SetActive(true);

        if (panelFinal != null)
            panelFinal.SetActive(false);

        PauseGameAndUnlockMouse();
    }

    private void PauseGameAndUnlockMouse()
    {
        Time.timeScale = 0f;
        if (mouseLocker != null)
            mouseLocker.Unlock();
    }

    private void ResumeGameAndLockMouse()
    {
        Time.timeScale = 1f;
        if (mouseLocker != null)
            mouseLocker.Lock();
    }

    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        ResumeGameAndLockMouse();
    }

    public void ShowPanelLinea1()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(true);

        PauseGameAndUnlockMouse();
    }

    public void OnLinea1Button()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(false);

        ResumeGameAndLockMouse();
    }

    public void ShowPanelLinea2()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(true);

        ShowBallCounter();
        PauseGameAndUnlockMouse();
    }

    public void OnLinea2Button()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(false);

        ResumeGameAndLockMouse();
    }

    public void ShowPanelFinal()
    {
        if (panelFinal != null)
            panelFinal.SetActive(true);

        PauseGameAndUnlockMouse();
    }

    public void OnFinalButton()
    {
        ResumeGameAndLockMouse();
        SceneManager.LoadScene(nextSceneName);
    }

    public void ShowBallCounter()
    {
        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(true);

        UpdateBallCounter();
    }

    public void RegisterBallKicked()
    {
        kickedCount++;
        UpdateBallCounter();

        if (kickedCount >= totalBalls)
            ShowPanelFinal();
    }

    private void UpdateBallCounter()
    {
        if (ballCounterText != null)
            ballCounterText.text = $"Balones: {kickedCount} / {totalBalls}";
    }
}
