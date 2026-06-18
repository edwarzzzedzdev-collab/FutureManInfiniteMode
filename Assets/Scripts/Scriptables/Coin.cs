using UnityEngine;

[CreateAssetMenu(fileName = "NuevaMoneda", menuName = "Game Settings/Moneda")]
public class CoinData : ScriptableObject
{
    [Header("Configuración Económica")]
    public int coinValue = 1;

    [Header("Configuración Estética")]
    public Material coinMaterial;

    [Header("Configuración de Animación")]
    public float rotationSpeed = 100f;
}