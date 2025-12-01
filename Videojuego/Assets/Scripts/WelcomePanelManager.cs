using UnityEngine;
using TMPro;

public class WelcomePanelManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelWelcome;   // Panel de bienvenida

    private void Start()
    {
        // Siempre aseguramos que el tiempo esté pausado
        Time.timeScale = 0f;

        if (panelWelcome != null)
        {
            panelWelcome.SetActive(true);
            Debug.Log("✅ PanelWelcome activado desde Start");
        }
        else
        {
            Debug.LogError("❌ panelWelcome NO está asignado en el inspector");
        }
    }

    // Esta función la asignas al botón "Continuar / Go"
    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        Time.timeScale = 1f;
        Debug.Log("▶️ Juego reanudado desde OnWelcomeButton");
    }
}

