using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Jugador")]
    public PlayerController player;   // arrastra aquí tu Player (con PlayerController)

    [Header("Panel 1")]
    public GameObject panelMensaje1;
    public Text textoMensaje1;
    public Button botonSiguiente;
    public Button botonGo;

    [Header("Panel 2")]
    public GameObject panelMensaje2;
    public Image imagenPersonaje;
    public Text textoMensaje2;
    public Button botonCerrar2;

    private void Start()
    {
        // asegurar que estén ocultos al inicio
        if (panelMensaje1 != null) panelMensaje1.SetActive(false);
        if (panelMensaje2 != null) panelMensaje2.SetActive(false);
    }

    // 🚩 LLAMADO POR LA LÍNEA 1
    public void MostrarPanel1()
    {
        if (player != null)
            player.canMove = false;   // bloquear jugador

        if (panelMensaje1 != null)
        {
            panelMensaje1.SetActive(true);

            // Primer mensaje
            textoMensaje1.text =
                "¡Bienvenido al segundo nivel!\n" +
                "Es hora de tocar el balón.";

            // Mostrar solo el botón Siguiente
            botonSiguiente.gameObject.SetActive(true);
            botonGo.gameObject.SetActive(false);
        }
    }

    // 👉 OnClick del botón "Siguiente"
    public void OnClickSiguiente()
    {
        // Segundo mensaje en el mismo panel
        textoMensaje1.text =
            "Estas son las indicaciones para jugar como todo un profesional:\n\n" +
            "W: avanza\n" +
            "S: retrocede\n" +
            "A: gira a la izquierda\n" +
            "D: gira a la derecha\n" +
            "Espacio: saltar\n" +
            "Click izquierdo del mouse: patear los balones\n\n" +
            "Si queda todo claro, continuemos.";

        // Ahora mostramos el botón GO
        botonSiguiente.gameObject.SetActive(false);
        botonGo.gameObject.SetActive(true);
    }

    // 👉 OnClick del botón "Go"
    public void OnClickGo()
    {
        if (panelMensaje1 != null)
            panelMensaje1.SetActive(false);

        if (player != null)
            player.canMove = true;   // volver a dejarlo moverse
    }

    // 🚩 LLAMADO POR LA LÍNEA 2
    public void MostrarPanel2()
    {
        if (player != null)
            player.canMove = false;   // bloquear jugador otra vez

        if (panelMensaje2 != null)
        {
            panelMensaje2.SetActive(true);

            // aquí solo controlamos el texto;
            // la imagen la diseñas tú en el panel
            textoMensaje2.text = "Comencemos a practicar, ven a la cancha.";
        }
    }

    // 👉 OnClick del botón de Panel 2 (OK / Go / Cerrar)
    public void OnClickCerrar2()
    {
        if (panelMensaje2 != null)
            panelMensaje2.SetActive(false);

        if (player != null)
            player.canMove = true;   // permitir que siga avanzando
    }
}
