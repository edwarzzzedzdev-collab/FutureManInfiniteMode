using UnityEngine;

[CreateAssetMenu(fileName = "NuevoEnemy1Data", menuName = "Game Settings/Enemy1Data")]
public class Enemy1Data : ScriptableObject
{
    [Header("Estadísticas de Vida")]
    public int maxHealth = 3;
    public Material flashMaterial; 
    public float flashDuration = 0.1f;  
    public float stunDuration = 0.5f;   
    public GameObject explosionPrefab; 

    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    public float turboSpeed = 1.5f; 

    [Header("Detector Único (Suelo y Paredes)")]
    public LayerMask environmentLayer; // <-- NUEVA LÍNEA: Determina qué es suelo/pared
    public Vector3 checkOffset = new Vector3(0.5f, 0.2f, 0f);
    public float detectorRadius = 0.2f;
    public float castDistance = 1.0f;

    [Header("Visión del Turbo")]
    public float playerDetectionRange = 6f;
    public float visionThickness = 0.5f; 

    [Header("Configuración de Animaciones")]
    public float movEye = 0.004f;       
    public float bobFrequency = 5f;    
    public float bobAmplitude = 0.03f; 
    public Color chaseColor = Color.red; 

    [Header("Ajuste de Llanta")]
    public bool resetTireScale = false;
}