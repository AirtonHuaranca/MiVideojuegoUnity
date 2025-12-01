using UnityEngine;
using TMPro;

public class WelcomePanelManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelWelcome;

    private void Start()
    {
        Time.timeScale = 0f;

        if (panelWelcome != null)
            panelWelcome.SetActive(true);
        else
            Debug.LogError("panelWelcome NO está asignado en el inspector");
    }

    public void OnWelcomeButton()
    {
        if (panelWelcome != null)
            panelWelcome.SetActive(false);

        Time.timeScale = 1f;
    }
}
