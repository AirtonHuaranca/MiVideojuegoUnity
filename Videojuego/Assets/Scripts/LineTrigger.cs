using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    [Tooltip("1 = primera línea, 2 = segunda línea")]
    public int lineNumber = 1;

    private bool activated = false;

    private void Awake()
    {
        Debug.Log($"[LineTrigger] Awake en {gameObject.name}, lineNumber = {lineNumber}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[LineTrigger] OnTriggerEnter en {gameObject.name} con {other.name}");

        if (activated) 
        {
            Debug.Log("[LineTrigger] Ya se activó antes, ignorando.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            activated = true;
            Debug.Log("[LineTrigger] El objeto que entró tiene tag Player.");

            TutorialManager tm = FindObjectOfType<TutorialManager>();
            if (tm == null)
            {
                Debug.LogError("[LineTrigger] ❌ NO encontré TutorialManager en la escena.");
                return;
            }

            if (lineNumber == 1)
            {
                Debug.Log("[LineTrigger] Mostrando PanelLinea1");
                tm.ShowPanelLinea1();
            }
            else if (lineNumber == 2)
            {
                Debug.Log("[LineTrigger] Mostrando PanelLinea2");
                tm.ShowPanelLinea2();
            }
            else
            {
                Debug.LogWarning($"[LineTrigger] lineNumber = {lineNumber} no está configurado (1 o 2).");
            }
        }
        else
        {
            Debug.Log($"[LineTrigger] El objeto que entró NO tiene tag Player, tiene tag: {other.tag}");
        }
    }
}
