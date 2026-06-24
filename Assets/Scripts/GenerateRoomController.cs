using System.Collections.Generic;
using UnityEngine;

public class GenerateRoomController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject BlockPrefab;       // ID 1
    public GameObject SpawnEnemyPrefab;  // ID 2
    public GameObject Spikes;            // ID 3
    public GameObject SpawnItemPrefab;   // ID 4
    public GameObject SpringPrefab;      // ID 5 (Muelle/Rebotador)

    [Header("Player Target")]
    public string playerTag = "Player";
    private Transform playerTransform;

    [Header("Room Templates")]
    public RoomData spawnRoom;
    public List<RoomData> flatRooms;

    [Header("Size & Scale Settings")]
    [Tooltip("The actual size of each tile unit. If your grid feels small, increase this (e.g., 1.5)")]
    public float tileSizeMultiplier = 1.5f;

    [Header("Memory Optimization Settings")]
    [Tooltip("Distance before the end of the last chunk to trigger a new spawn")]
    public float spawnThresholdDistance = 15f;
    [Tooltip("Maximum number of chunks allowed in the scene simultaneously")]
    public int maxActiveChunks = 3;

    // Internal state tracking
    private float currentXOffset = 0f;
    private Queue<GameObject> activeChunkContainers = new Queue<GameObject>();

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"GenerateRoomController: No GameObject found with tag '{playerTag}'.");
        }

        SpawnNewChunk(spawnRoom);
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.x > currentXOffset - spawnThresholdDistance)
        {
            GenerateRandomFlatChunk();
        }
    }

    void GenerateRandomFlatChunk()
    {
        if (flatRooms == null || flatRooms.Count == 0) return;

        int randomIndex = Random.Range(0, flatRooms.Count);
        RoomData selectedRoom = flatRooms[randomIndex];

        SpawnNewChunk(selectedRoom);
    }

    void SpawnNewChunk(RoomData room)
    {
        GameObject chunkContainer = new GameObject($"Chunk_{room.roomName}_{currentXOffset}");
        chunkContainer.transform.SetParent(this.transform);

        for (int y = 0; y < room.height; y++)
        {
            for (int x = 0; x < room.width; x++)
            {
                int index = y * room.width + x;
                if (index >= room.layout.Length) continue;

                int tileID = room.layout[index];

                // Si es 0, es espacio vacío, saltamos al siguiente tile sin instanciar nada
                if (tileID == 0) continue;

                // Calcular posiciones escaladas en base al multiplicador (ej. 1.5)
                float posX = currentXOffset + (x * tileSizeMultiplier);
                float posY = y * tileSizeMultiplier;
                Vector3 spawnPosition = new Vector3(posX, posY, 0f);

                GameObject spawnedObject = null;

                // Switch case para evaluar cada ID de tu tabla de Excel
                switch (tileID)
                {
                    case 1: // Bloque Sólido
                        if (BlockPrefab != null) spawnedObject = Instantiate(BlockPrefab, spawnPosition, Quaternion.identity);
                        break;

                    case 2: // Enemigo
                        if (SpawnEnemyPrefab != null) spawnedObject = Instantiate(SpawnEnemyPrefab, spawnPosition, Quaternion.identity);
                        break;

                    case 3: // Espinas
                        if (Spikes != null) spawnedObject = Instantiate(Spikes, spawnPosition, Quaternion.identity);
                        break;

                    case 4: // Ítems
                        if (SpawnItemPrefab != null) spawnedObject = Instantiate(SpawnItemPrefab, spawnPosition, Quaternion.identity);
                        break;

                    case 5: // Muelle / Rebotador
                        if (SpringPrefab != null) spawnedObject = Instantiate(SpringPrefab, spawnPosition, Quaternion.identity);
                        break;
                }

                // Si se logró crear el objeto, lo escalamos y lo metemos en el contenedor para la limpieza de memoria
                if (spawnedObject != null)
                {
                    spawnedObject.transform.localScale = new Vector3(tileSizeMultiplier, tileSizeMultiplier, 1f);
                    spawnedObject.transform.SetParent(chunkContainer.transform);
                }
            }
        }

        // Desplazar el offset usando el ancho real escalado del cuarto
        currentXOffset += (room.width * tileSizeMultiplier);

        activeChunkContainers.Enqueue(chunkContainer);

        // Limpieza de memoria automática al superar el límite de chunks activos
        if (activeChunkContainers.Count > maxActiveChunks)
        {
            GameObject oldestChunk = activeChunkContainers.Dequeue();
            Destroy(oldestChunk);
        }
    }
}