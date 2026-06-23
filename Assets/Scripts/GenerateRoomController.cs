using System.Collections.Generic;
using UnityEngine;

public class GenerateRoomController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject BlockPrefab;
    public GameObject SpawnItemPrefab; 
    public GameObject SpawnEnemyPrefab; 

    [Header("Player Target")]
    public string playerTag = "Player";
    private Transform playerTransform;

    [Header("Room Templates")]
    public RoomData spawnRoom;
    public List<RoomData> flatRooms;

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
        // Find the player automatically using their Tag
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"GenerateRoomController: No GameObject found with tag '{playerTag}'. Make sure your Player has the correct tag.");
        }

        // Initialize the first room (Spawn)
        SpawnNewChunk(spawnRoom);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // If the player gets close to the end of the generated world, spawn the next chunk
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
        // Create an empty GameObject to group all tiles of THIS specific chunk
        GameObject chunkContainer = new GameObject($"Chunk_{room.roomName}_{currentXOffset}");
        chunkContainer.transform.SetParent(this.transform);

        // Build the blocks inside the container
        for (int y = 0; y < room.height; y++)
        {
            for (int x = 0; x < room.width; x++)
            {
                int index = y * room.width + x;
                if (index >= room.layout.Length) continue;

                int tileID = room.layout[index];

                if (tileID == 1) // Block
                {
                    Vector3 spawnPosition = new Vector3(currentXOffset + x, y, 0f);
                    GameObject newBlock = Instantiate(BlockPrefab, spawnPosition, Quaternion.identity);
                    
                    // Parent it to the container instead of the main controller
                    newBlock.transform.SetParent(chunkContainer.transform);
                }
            }
        }

        // Move the offset forward for the NEXT chunk
        currentXOffset += room.width;

        // Add this new container to our memory tracking queue
        activeChunkContainers.Enqueue(chunkContainer);

        // Memory Cleanup: If we exceed the maximum active chunks, delete the oldest one
        if (activeChunkContainers.Count > maxActiveChunks)
        {
            GameObject oldestChunk = activeChunkContainers.Dequeue();
            Destroy(oldestChunk); // This deletes the container and ALL its children blocks instantly
        }
    }
}