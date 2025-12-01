using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Paneles del tutorial")]
    public GameObject panelWelcome;     // Panel de bienvenida
    public GameObject panelLinea1;      // Panel al cruzar línea 1
    public GameObject panelLinea2;      // Panel al cruzar línea 2

    [Header("Contador de balones")]
    public GameObject ballCounterRoot;  // Panel/objeto que contiene el contador
    public TMP_Text ballCounterText;    // Texto TMP que muestra "Balones: X / Y"
    public int totalBalls = 10;         // Número total de balones del nivel

    [Header("Panel Final")]
public GameObject panelFinal;         // <-- Panel final al terminar
public string nextSceneName = "Nivel3";  // <-- Cambia esto por tu escena real


    [Header("Sistemas")]
    public LockMouse mouseLocker;       // Referencia al LockMouse

    private int kickedCount = 0;        // Balones pateados

    private void Start()
    {
        // Asegurar estado inicial
        kickedCount = 0;
        UpdateBallCounter();

        // Ocultar contador al inicio
        if (ballCounterRoot != null)
            ballCounterRoot.SetActive(false);

        // Ocultar paneles de líneas al inicio
        if (panelLinea1 != null) panelLinea1.SetActive(false);
        if (panelLinea2 != null) panelLinea2.SetActive(false);

        // Mostrar panel de bienvenida y pausar el juego
        if (panelWelcome != null)
        {
            panelWelcome.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ PanelWelcome NO asignado en TutorialManager");
        }

        if (panelFinal != null)panelFinal.SetActive(false);


        PauseGameAndUnlockMouse();
    }

    // ===================== CONTROL DE JUEGO / MOUSE =====================

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

    // ======================== PANEL BIENVENIDA =========================

    // Asignar al botón del PanelWelcome
    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        ResumeGameAndLockMouse();
    }

    // ======================== PANEL LINEA 1 ============================

    public void ShowPanelLinea1()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(true);

        PauseGameAndUnlockMouse();
    }

    // Asignar al botón del PanelLinea1
    public void OnLinea1Button()
    {
        if (panelLinea1 != null)
            panelLinea1.SetActive(false);

        ResumeGameAndLockMouse();
    }

    // ======================== PANEL LINEA 2 ============================

    public void ShowPanelLinea2()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(true);

        // Al mostrar este panel, activamos el contador
        ShowBallCounter();

        PauseGameAndUnlockMouse();
    }

    // Asignar al botón del PanelLinea2
    public void OnLinea2Button()
    {
        if (panelLinea2 != null)
            panelLinea2.SetActive(false);

        ResumeGameAndLockMouse();
    }
    // ========================== Panel Final ============================

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


    // ===================== CONTADOR DE BALONES =========================

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
    {
        ShowPanelFinal();
    }
}

    private void UpdateBallCounter()
    {
        if (ballCounterText != null)
        {
            ballCounterText.text = $"Balones: {kickedCount} / {totalBalls}";
        }
    }
}
