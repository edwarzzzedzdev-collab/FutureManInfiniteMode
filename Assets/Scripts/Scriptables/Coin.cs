using UnityEngine;

[CreateAssetMenu(fileName = "NuevaMoneda", menuName = "Configuracion Juego/Items/Moneda")]
public class CoinData : ScriptableObject
{
    [Header("Configuración Económica")]
    public int coinValue = 1;

    [Header("Configuración Estética")]
    public Material coinMaterial;

    [Header("Configuración de Animación")]
    public float rotationSpeed = 100f;
}