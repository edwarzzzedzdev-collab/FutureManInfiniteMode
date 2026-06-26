using System.Collections.Generic;
using UnityEngine;

public class GenerateRoomController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject BlockPrefab;       // ID 1
    public GameObject SpawnEnemyPrefab;  // ID 2
    public GameObject Spikes;            // ID 3
    public GameObject SpawnItemPrefab;   // ID 4
    public GameObject SpringPrefab;      // ID 5

    [Header("Player Target")]
    public string playerTag = "Player";
    private Transform playerTransform;

    [Header("Room Templates")]
    public RoomData spawnRoom;
    
    [Tooltip("Arrastra aquí todas tus salas juntas (Flat, Up, Up2, Down, Down2).")]
    public List<RoomData> allRooms; 

    [Header("Size & Scale Settings")]
    public float tileSizeMultiplier = 1.5f;

    [Header("Memory Optimization Settings")]
    public float spawnThresholdDistance = 15f;
    public int maxActiveChunks = 3;

    // Listas dinámicas filtradas al iniciar
    private List<RoomData> flatRooms = new List<RoomData>();
    private List<RoomData> upRooms = new List<RoomData>();
    private List<RoomData> up2Rooms = new List<RoomData>();
    private List<RoomData> downRooms = new List<RoomData>();
    private List<RoomData> down2Rooms = new List<RoomData>();

    // Rastreadores de posición infinitos
    private float currentXOffset = 0f;
    private float currentYOffset = 0f; 
    private Queue<GameObject> activeChunkContainers = new Queue<GameObject>();
    private RoomData lastSpawnedRoom = null;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null) playerTransform = playerObj.transform;

        foreach (RoomData room in allRooms)
        {
            if (room == null) continue;
            switch (room.roomType)
            {
                case RoomType.Flat:  flatRooms.Add(room);  break;
                case RoomType.Up:    upRooms.Add(room);    break;
                case RoomType.Up2:   up2Rooms.Add(room);   break;
                case RoomType.Down:  downRooms.Add(room);  break;
                case RoomType.Down2: down2Rooms.Add(room); break;
            }
        }

        // Habitación inicial
        SpawnSingleChunk(spawnRoom, false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.x > currentXOffset - spawnThresholdDistance)
        {
            GenerateNextTerrain();
        }
    }

    void GenerateNextTerrain()
    {
        float roll = Random.value;

        if (upRooms.Count > 0 && up2Rooms.Count > 0 && roll < 0.20f)
        {
            RoomData upRoom = GetRandomRoomFromList(upRooms);
            RoomData up2Room = GetRandomRoomFromList(up2Rooms);

            if (upRoom != null && up2Room != null)
            {
                SpawnStackedUpChunks(upRoom, up2Room);
                return;
            }
        }
        else if (downRooms.Count > 0 && down2Rooms.Count > 0 && roll < 0.40f)
        {
            RoomData downRoom = GetRandomRoomFromList(downRooms);
            RoomData down2Room = GetRandomRoomFromList(down2Rooms);

            if (downRoom != null && down2Room != null)
            {
                SpawnStackedDownChunks(downRoom, down2Room);
                return;
            }
        }

        if (flatRooms.Count > 0)
        {
            RoomData flatRoom = GetRandomRoomFromList(flatRooms);
            SpawnSingleChunk(flatRoom, Random.value > 0.5f);
        }
    }

    RoomData GetRandomRoomFromList(List<RoomData> roomList)
    {
        if (roomList == null || roomList.Count == 0) return null;
        if (roomList.Count == 1) return roomList[0];

        RoomData chosen = null;
        int safetyNet = 0;
        do
        {
            chosen = roomList[Random.Range(0, roomList.Count)];
            safetyNet++;
        } while (chosen == lastSpawnedRoom && safetyNet < 10);

        lastSpawnedRoom = chosen;
        return chosen;
    }

    void SpawnSingleChunk(RoomData room, bool mirror)
    {
        string mirrorTag = mirror ? " [ESPEJO]" : "";
        GameObject chunkContainer = new GameObject($"Chunk_{room.roomName}_{currentXOffset}{mirrorTag}");
        chunkContainer.transform.SetParent(this.transform);

        GenerateTilesForRoom(room, chunkContainer, currentXOffset, currentYOffset, mirror);
        currentXOffset += (room.width * tileSizeMultiplier);

        FinalizeChunkContainer(chunkContainer);
    }

    void SpawnStackedUpChunks(RoomData upRoom, RoomData up2Room)
    {
        GameObject chunkContainer = new GameObject($"MegaSubida_{currentXOffset}");
        chunkContainer.transform.SetParent(this.transform);

        GenerateTilesForRoom(upRoom, chunkContainer, currentXOffset, currentYOffset, false);
        
        float up2YPosition = currentYOffset + (upRoom.height * tileSizeMultiplier);
        GenerateTilesForRoom(up2Room, chunkContainer, currentXOffset, up2YPosition, false);

        currentXOffset += (upRoom.width * tileSizeMultiplier);
        currentYOffset += (upRoom.height * tileSizeMultiplier);

        FinalizeChunkContainer(chunkContainer);
    }

    void SpawnStackedDownChunks(RoomData downRoom, RoomData down2Room)
    {
        GameObject chunkContainer = new GameObject($"MegaBajada_{currentXOffset}");
        chunkContainer.transform.SetParent(this.transform);

        float downYPosition = currentYOffset;
        GenerateTilesForRoom(downRoom, chunkContainer, currentXOffset, downYPosition, false);

        float down2YPosition = currentYOffset - (down2Room.height * tileSizeMultiplier);
        GenerateTilesForRoom(down2Room, chunkContainer, currentXOffset, down2YPosition, false);

        currentXOffset += (downRoom.width * tileSizeMultiplier);
        currentYOffset -= (down2Room.height * tileSizeMultiplier);

        FinalizeChunkContainer(chunkContainer);
    }

    void GenerateTilesForRoom(RoomData room, GameObject container, float baseX, float baseY, bool mirror)
    {
        // ====================================================================
        // FASE 1: GENERAR EL MUNDO FÍSICO (Bloques, pinchos, ítems, muelles)
        // ====================================================================
        for (int y = 0; y < room.height; y++)
        {
            for (int x = 0; x < room.width; x++)
            {
                int index = y * room.width + x;
                if (index >= room.layout.Length) continue;

                int tileID = room.layout[index];
                if (tileID == 0 || tileID == 2) continue; // Ignoramos vacíos y spawners de enemigos en esta fase

                int calculatedX = mirror ? (room.width - 1 - x) : x;

                float posX = baseX + (calculatedX * tileSizeMultiplier);
                float posY = baseY + (y * tileSizeMultiplier);
                Vector3 spawnPosition = new Vector3(posX, posY, 0f);

                GameObject spawnedObject = null;

                switch (tileID)
                {
                    case 1: if (BlockPrefab != null) spawnedObject = Instantiate(BlockPrefab, spawnPosition, Quaternion.identity); break;
                    case 3: if (Spikes != null) spawnedObject = Instantiate(Spikes, spawnPosition, Quaternion.identity); break;
                    case 4: if (SpawnItemPrefab != null) spawnedObject = Instantiate(SpawnItemPrefab, spawnPosition, Quaternion.identity); break;
                    case 5: if (SpringPrefab != null) spawnedObject = Instantiate(SpringPrefab, spawnPosition, Quaternion.identity); break;
                }

                if (spawnedObject != null)
                {
                    float finalScaleX = mirror ? -tileSizeMultiplier : tileSizeMultiplier;
                    spawnedObject.transform.localScale = new Vector3(finalScaleX, tileSizeMultiplier, 1f);
                    spawnedObject.transform.SetParent(container.transform);
                }
            }
        }

        // ¡EL TRUCO SÚPER CLAVE!: Forzamos a Unity a registrar físicamente la Fase 1 inmediatamente
        Physics2D.SyncTransforms();

        // ====================================================================
        // FASE 2: GENERAR ENTIDADES DINÁMICAS (Puntos de Spawn de Enemigos)
        // ====================================================================
        for (int y = 0; y < room.height; y++)
        {
            for (int x = 0; x < room.width; x++)
            {
                int index = y * room.width + x;
                if (index >= room.layout.Length) continue;

                int tileID = room.layout[index];
                if (tileID != 2) continue; // En esta fase SOLO procesamos los puntos de spawn de enemigos

                int calculatedX = mirror ? (room.width - 1 - x) : x;

                float posX = baseX + (calculatedX * tileSizeMultiplier);
                float posY = baseY + (y * tileSizeMultiplier);
                Vector3 spawnPosition = new Vector3(posX, posY, 0f);

                GameObject spawnedObject = null;

                if (SpawnEnemyPrefab != null)
                {
                    spawnedObject = Instantiate(SpawnEnemyPrefab, spawnPosition, Quaternion.identity);
                }

                if (spawnedObject != null)
                {
                    float finalScaleX = mirror ? -tileSizeMultiplier : tileSizeMultiplier;
                    spawnedObject.transform.localScale = new Vector3(finalScaleX, tileSizeMultiplier, 1f);
                    spawnedObject.transform.SetParent(container.transform);
                }
            }
        }
    }

    void FinalizeChunkContainer(GameObject chunkContainer)
    {
        activeChunkContainers.Enqueue(chunkContainer);

        if (activeChunkContainers.Count > maxActiveChunks)
        {
            GameObject oldestChunk = activeChunkContainers.Dequeue();
            Destroy(oldestChunk);
        }
    }
}