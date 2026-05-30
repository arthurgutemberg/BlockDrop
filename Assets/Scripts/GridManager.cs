using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public GameObject blockPrefab;
    public Transform[,] gridCells;
    private Transform[,] occupiedBy;   // guarda qual peça está em cada célula
    private GameObject[,] occupiedBlock; // guarda o GameObject do bloco em cada célula
    public float GridStartX { get; private set; }
    public float GridStartY { get; private set; }
    void Awake()
    {
        occupiedBy = new Transform[width, height];
        gridCells = new Transform[width, height];
        occupiedBlock = new GameObject[width, height];
        DrawGrid();
    }

    void DrawGrid()
    {
        // Cria um objeto para organizar as células como filho do Grid
        GameObject cellsHolder = new GameObject("Cells");
        cellsHolder.transform.parent = transform;
        cellsHolder.transform.localPosition = Vector3.zero; // posição local zero em relação ao Grid

        float offsetX = -width / 2f + 0.5f;
        float offsetY = -height / 2f + 0.5f;
        
        GridStartX = offsetX;
        GridStartY = offsetY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Instancia a célula como filha do cellsHolder
                GameObject cell = Instantiate(cellPrefab, cellsHolder.transform);
                // Define a posição local em relação ao cellsHolder
                cell.transform.localPosition = new Vector3(x + offsetX, y + offsetY, 0);
                cell.name = $"Cell ({x},{y})";

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if ((x + y) % 2 == 0)
                    sr.color = new Color(0.9f, 0.9f, 0.9f, 0.3f);
                else
                    sr.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);

                gridCells[x, y] = cell.transform;
            }
        }
        // Não precisa definir transform.position = ... – a posição do Grid é controlada manualmente
    }

    // Métodos de ocupação
    public bool IsCellOccupied(int x, int y)
    {
        return occupiedBy[x, y] != null;
    }

    public void SetCellOccupied(int x, int y, Transform piece, GameObject block)
    {
        occupiedBy[x, y] = piece;
        occupiedBlock[x, y] = block;
    }

    public void ClearCell(int x, int y)
    {
        occupiedBy[x, y] = null;
        occupiedBlock[x, y] = null;
    }
    // Limpa visualmente os blocos de uma linha e libera as células
    public void ClearLine(int row)
    {
        for (int x = 0; x < width; x++)
        {
            if (occupiedBlock[x, row] != null)
            {
                StartCoroutine(FadeAndDestroy(occupiedBlock[x, row], 0.3f));
            ClearCell(x, row);
            }
        }
    }

    // Verifica todas as linhas, limpa as completas e retorna quantas foram removidas
    public int CheckAndClearLines()
    {
        int cleared = 0;
        for (int y = height - 1; y >= 0; y--)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
            {
                if (!IsCellOccupied(x, y))
                {
                    full = false;
                    break;
                }
            }
            if (full)
            {
                ClearLine(y);
                cleared++;
            }
        }
        return cleared;
    }
    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float gridStartX = transform.position.x + GridStartX;
        float gridStartY = transform.position.y + GridStartY;
        int x = Mathf.RoundToInt(worldPos.x - gridStartX);
        int y = Mathf.RoundToInt(worldPos.y - gridStartY);
        return new Vector2Int(x, y);
    }

    public int CheckAndClearColumns()
    {
        int cleared = 0;
        for (int x = 0; x < width; x++)
        {
            bool full = true;
            for (int y = 0; y < height; y++)
            {
                if (!IsCellOccupied(x, y))
                {
                    full = false;
                    break;
                }
            }
            if (full)
            {
                ClearColumn(x);
                cleared++;
            }
        }
        return cleared;
    }

    private void ClearColumn(int col)
        {
            for (int y = 0; y < height; y++)
            {
                if (occupiedBlock[col, y] != null)
                {
                    StartCoroutine(FadeAndDestroy(occupiedBlock[col, y], 0.3f)); // mesmo fade das linhas
                    ClearCell(col, y);
                }
            }
        }
        IEnumerator FadeAndDestroy(GameObject block, float duration)
        {
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Destroy(block);
                yield break;
            }

            Color startColor = sr.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                sr.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }

            Destroy(block);
        }
}

