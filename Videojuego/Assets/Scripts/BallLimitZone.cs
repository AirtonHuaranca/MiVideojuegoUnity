using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BallLimitZone : MonoBehaviour
{
    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        // Nos aseguramos de que sea trigger siempre
        boxCollider.isTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        // Solo nos interesa la pelota
        if (!other.CompareTag("Ball")) return;

        // 1. Tomamos la posición actual de la pelota
        Vector3 pos = other.transform.position;

        // 2. Obtenemos los límites (min y max) del área
        Bounds bounds = boxCollider.bounds;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;
        float minZ = bounds.min.z;
        float maxZ = bounds.max.z;

        // 3. "Clampeamos" (limitamos) la posición para que quede dentro
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        // Ajustamos la altura de la pelota (Y) para que no quede enterrada
        pos.y = Mathf.Max(pos.y, bounds.min.y + 0.5f);

        // 4. Colocamos de nuevo la pelota dentro del área
        other.transform.position = pos;

        // 5. Opcional: frenar la pelota un poco
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
