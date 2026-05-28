using UnityEngine;

public class Ondestroy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Awake()
{
    DontDestroyOnLoad(gameObject); // Esto salvará a TODO el grupo al cambiar de escena
}
}
