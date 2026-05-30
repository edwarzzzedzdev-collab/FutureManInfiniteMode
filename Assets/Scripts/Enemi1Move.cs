using UnityEngine;

public class Enemi1Move : MonoBehaviour
{
    [Header("Componentes del Robot")]
    public GameObject tire;
    public GameObject EnemyBoddy; 
    public GameObject Pupil;

    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    public float TurboSpeed = 1.5f; 
    private float currentDirection = 1f; // 1 = Derecha, -1 = Izquierda
    private bool isChasing = false;

    [Header("Detector Único (Suelo y Paredes)")]
    [SerializeField] private LayerMask environmentLayer; 
    [Tooltip("Punto de origen del detector respecto al centro del robot")]
    public Vector3 checkOffset = new Vector3(0.5f, 0.2f, 0f);
    public float detectorRadius = 0.2f;
    [Tooltip("Qué tan lejos se lanza la esfera diagonalmente hacia abajo")]
    public float castDistance = 1.0f;

    [Header("Visión del Turbo")]
    public float playerDetectionRange = 6f;
    public float visionThickness = 0.5f; 

    [Header("Configuración de Animaciones")]
    public float MovEye = 0.004f;       
    public float bobFrequency = 5f;    
    public float bobAmplitude = 0.03f; 
    public Color chaseColor = Color.red; 

    [Header("Ajuste de Llanta")]
    public bool resetTireScale = false;

    // Variables de control interno
    private Vector3 bodyInitialLocalPos;
    private Vector3 pupilInitialLocalPos;
    private Vector3 tireWorldOffset; 
    private Material pupilMaterial;
    private Color pupilInitialColor;

    void Start()
    {
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
            if (resetTireScale) tire.transform.localScale = Vector3.one; 
        }
    }

    void Update()
    {
        HandlePatrolBoundaries();
        DetectPlayer();

        // Cálculo de velocidad y desplazamiento
        float finalSpeed = speed * (isChasing ? TurboSpeed : 1f);
        transform.Translate(Vector3.right * currentDirection * finalSpeed * Time.deltaTime, Space.World);

        // --- SISTEMA DE ANIMACIONES COMPACTO ---
        if (tire)
        {
            tire.transform.position = transform.position + tireWorldOffset;
            tire.transform.Rotate(Vector3.forward * -currentDirection * finalSpeed * 150f * Time.deltaTime, Space.Self);
        }

        if (EnemyBoddy)
        {
            float freq = isChasing ? bobFrequency * 1.6f : bobFrequency;
            float newY = bodyInitialLocalPos.y + Mathf.Sin(Time.time * freq) * bobAmplitude;
            EnemyBoddy.transform.localPosition = new Vector3(bodyInitialLocalPos.x, newY, bodyInitialLocalPos.z);
        }

        if (Pupil)
        {
            float targetY = pupilInitialLocalPos.y + (currentDirection * MovEye);
            Pupil.transform.localPosition = Vector3.Lerp(Pupil.transform.localPosition, new Vector3(pupilInitialLocalPos.x, targetY, pupilInitialLocalPos.z), Time.deltaTime * 5f);
            
            if (pupilMaterial) 
                pupilMaterial.color = Color.Lerp(pupilMaterial.color, isChasing ? chaseColor : pupilInitialColor, Time.deltaTime * 8f);
        }
    }

    void HandlePatrolBoundaries()
    {
        // Origen dinámico según la dirección y vector diagonal (Adelante + Abajo)
        Vector3 origin = transform.position + new Vector3(checkOffset.x * currentDirection, checkOffset.y, 0f);
        Vector3 direction = new Vector3(currentDirection, -1f, 0f).normalized;

        // Un solo disparo de esfera física para todo
        if (Physics.SphereCast(origin, detectorRadius, direction, out RaycastHit hit, castDistance, environmentLayer))
        {
            // Si el impacto tiene una inclinación lateral pronunciada (hit.normal.x), es una pared
            if (Mathf.Abs(hit.normal.x) > 0.6f) 
            {
                currentDirection *= -1f;
            }
        }
        else
        {
            // Si no detecta absolutamente nada, es un abismo/vacío
            currentDirection *= -1f;
        }
    }

    void DetectPlayer()
    {
        Vector3 moveDir = Vector3.right * currentDirection;
        Vector3 visionOrigin = transform.position + Vector3.up * 0.3f;
        
        if (Physics.SphereCast(visionOrigin, visionThickness, moveDir, out RaycastHit hit, playerDetectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isChasing = true;
                return;
            }
        }
        isChasing = false;
    }

    private void OnDrawGizmos()
    {
        // Dibujo del detector único diagonal (Color Cian)
        Vector3 origin = transform.position + new Vector3(checkOffset.x * currentDirection, checkOffset.y, 0f);
        Vector3 direction = new Vector3(currentDirection, -1f, 0f).normalized;
        Vector3 endPoint = origin + direction * castDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, detectorRadius);
        Gizmos.DrawWireSphere(endPoint, detectorRadius);
        Gizmos.DrawLine(origin, endPoint);

        // Visión del jugador (Color Amarillo)
        Gizmos.color = Color.yellow;
        Vector3 visionOrigin = transform.position + Vector3.up * 0.3f;
        Vector3 visionEnd = visionOrigin + (Vector3.right * currentDirection * playerDetectionRange);
        
        Gizmos.DrawWireSphere(visionOrigin, visionThickness);
        Gizmos.DrawWireSphere(visionEnd, visionThickness);
        Gizmos.DrawLine(visionOrigin, visionEnd);
    }
}