using UnityEngine;

public enum RoomType { Flat, Up, Up2, Down, Down2 }

[CreateAssetMenu(fileName = "NewRoomData", menuName = "Procedural/RoomData")]
public class RoomData : ScriptableObject
{
    [Header("Room Information")]

    
    public string roomName;
    public RoomType roomType;

    [Header("Dimensions (Auto-detected)")]
    public int width = 20;
    public int height = 15;

    [Header("Layout Data (Flattened Matrix)")]
    public int[] layout;

    [Header("⚡ Excel Import Tool")]
    [TextArea(5, 10)]
    [Tooltip("1. Copy your cells from Excel.\n2. Paste them here.\n3. Right-click the component context menu (three dots) and select 'Process Excel Data'.")]
    public string pasteFromExcel;

    [ContextMenu("Process Excel Data")]
    public void ProcessExcelData()
    {
        if (string.IsNullOrEmpty(pasteFromExcel))
        {
            Debug.LogError("Please paste Excel data into the text area before processing.");
            return;
        }

        string[] rows = pasteFromExcel.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        
        height = rows.Length;
        string[] firstRowColumns = rows[0].Split(new[] { '\t', ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        width = firstRowColumns.Length;

        layout = new int[width * height];

        for (int y = 0; y < height; y++)
        {
            int unityRow = height - 1 - y; 
            string[] columns = rows[y].Split(new[] { '\t', ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            for (int x = 0; x < width; x++)
            {
                if (x < columns.Length)
                {
                    if (int.TryParse(columns[x], out int value))
                    {
                        layout[unityRow * width + x] = value;
                    }
                }
            }
        }

        Debug.Log($"Chunk '{name}' successfully processed! Size: {width}x{height}. Matrix loaded.");
    }
}