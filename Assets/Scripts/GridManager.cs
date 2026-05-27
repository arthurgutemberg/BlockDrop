using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;   // prefab Cell
    public GameObject blockPrefab;  // prefab Block (será usado depois)
    public Transform[,] gridCells;

    void Start()
    {
        gridCells = new Transform[width, height];
        DrawGrid();
    }

    void DrawGrid()
    {
        GameObject cellsHolder = new GameObject("Cells");
        cellsHolder.transform.parent = transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                cell.name = $"Cell ({x},{y})";
                cell.transform.parent = cellsHolder.transform;

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if ((x + y) % 2 == 0)
                    sr.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                else
                    sr.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);

                gridCells[x, y] = cell.transform;
            }
        }

        // Posiciona a grade à esquerda (células vão de (0,0) a (7,7) localmente)
        transform.position = new Vector3(-5f, -3.5f, 0);
    }
}