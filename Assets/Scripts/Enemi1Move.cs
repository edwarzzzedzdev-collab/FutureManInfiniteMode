using UnityEngine;

public class Enemi1Move : MonoBehaviour
{
    [Header("Perfil de Datos")]
    [SerializeField] private Enemy1Data data; // Guardamos la referencia al ScriptableObject

    [Header("Componentes del Robot")]
    public GameObject tire;
    public GameObject EnemyBoddy; 
    public GameObject Pupil;

    // Los datos de ESTADO se quedan aquí (Pertenecen a la instancia individual)
    private float currentDirection = 1f; 
    private bool isChasing = false;
    private Vector3 bodyInitialLocalPos;
    private Vector3 pupilInitialLocalPos;
    private Vector3 tireWorldOffset; 
    private Material pupilMaterial;
    private Color pupilInitialColor;

    void Start()
    {
        if (data == null) { Debug.LogError($"¡Falta el ScriptableObject de datos en {gameObject.name}!"); return; }

        if (EnemyBoddy) bodyInitialLocalPos = EnemyBoddy.transform.localPosition;
        
        if (Pupil)
        {
            pupilInitialLocalPos = Pupil.transform.localPosition;
            if (Pupil.TryGetComponent<Renderer>(out var r))
            {
                pupilMaterial = r.material;
                pupilInitialColor = pupilMaterial.color;
            }
        }

        if (tire)
        {
            tireWorldOffset = tire.transform.position - transform.position;
            tire.transform.SetParent(null); 
            if (data.resetTireScale) tire.transform.localScale = Vector3.one; 
        }
    }

    void Update()
    {
        if (data == null) return;

        HandlePatrolBoundaries();
        DetectPlayer();

        float finalSpeed = data.speed * (isChasing ? data.turboSpeed : 1f);
        transform.Translate(Vector3.right * currentDirection * finalSpeed * Time.deltaTime, Space.World);

        if (tire)
        {
            tire.transform.position = transform.position + tireWorldOffset;
            tire.transform.Rotate(Vector3.forward * -currentDirection * finalSpeed * 150f * Time.deltaTime, Space.Self);
        }

        if (EnemyBoddy)
        {
            float freq = isChasing ? data.bobFrequency * 1.6f : data.bobFrequency;
            float newY = bodyInitialLocalPos.y + Mathf.Sin(Time.time * freq) * data.bobAmplitude;
            EnemyBoddy.transform.localPosition = new Vector3(bodyInitialLocalPos.x, newY, bodyInitialLocalPos.z);
        }

        if (Pupil)
        {
            float targetY = pupilInitialLocalPos.y + (currentDirection * data.movEye);
            Pupil.transform.localPosition = Vector3.Lerp(Pupil.transform.localPosition, new Vector3(pupilInitialLocalPos.x, targetY, pupilInitialLocalPos.z), Time.deltaTime * 5f);
            
            if (pupilMaterial) 
                pupilMaterial.color = Color.Lerp(pupilMaterial.color, isChasing ? data.chaseColor : pupilInitialColor, Time.deltaTime * 8f);
        }
    }

   void HandlePatrolBoundaries()
    {
        Vector3 origin = transform.position + new Vector3(data.checkOffset.x * currentDirection, data.checkOffset.y, 0f);
        Vector3 direction = new Vector3(currentDirection, -1f, 0f).normalized;

        // Ahora el SphereCast SOLO detectará las capas que tú le indiques en el ScriptableObject
        if (Physics.SphereCast(origin, data.detectorRadius, direction, out RaycastHit hit, data.castDistance, data.environmentLayer))
        {
            // SALVAGUARDA: Si el rayo choca con el jugador, ignoramos el impacto 
            // y salimos del método sin cambiar la dirección.
            if (hit.collider.CompareTag("Player")) return; 

            // Si es una pared legítima, cambia de dirección
            if (Mathf.Abs(hit.normal.x) > 0.6f) 
            {
                currentDirection *= -1f;
            }
        }
        else
        {
            // Si no detecta suelo (vacío), cambia de dirección
            currentDirection *= -1f;
        }
    }

    // Nota: Para mantener la limpieza del EnvironmentLayer unificada, el SphereCast ahora lee las capas del proyecto.
    // Si prefieres pasarle la LayerMask explícita por ScriptableObject, puedes añadir 'public LayerMask environmentLayer;' en Enemy1Data.

    void DetectPlayer()
    {
        Vector3 moveDir = Vector3.right * currentDirection;
        Vector3 visionOrigin = transform.position + Vector3.up * 0.3f;
        
        if (Physics.SphereCast(visionOrigin, data.visionThickness, moveDir, out RaycastHit hit, data.playerDetectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isChasing = true;
                return;
            }
        }
        isChasing = false;
    }

    private void OnDestroy()
    {
        if (tire != null) Destroy(tire);
    }

    private void OnDrawGizmos()
    {
        if (data == null) return;

        Vector3 origin = transform.position + new Vector3(data.checkOffset.x * currentDirection, data.checkOffset.y, 0f);
        Vector3 direction = new Vector3(currentDirection, -1f, 0f).normalized;
        Vector3 endPoint = origin + direction * data.castDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, data.detectorRadius);
        Gizmos.DrawWireSphere(endPoint, data.detectorRadius);
        Gizmos.DrawLine(origin, endPoint);

        Gizmos.color = Color.yellow;
        Vector3 visionOrigin = transform.position + Vector3.up * 0.3f;
        Vector3 visionEnd = visionOrigin + (Vector3.right * currentDirection * data.playerDetectionRange);
        
        Gizmos.DrawWireSphere(visionOrigin, data.visionThickness);
        Gizmos.DrawWireSphere(visionEnd, data.visionThickness);
        Gizmos.DrawLine(visionOrigin, visionEnd);
    }
}