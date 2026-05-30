using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    I, O, T, L, J, S, Z, Dot, Domino, Trio
}

public static class PieceShape
{
    public static Dictionary<PieceType, List<Vector2Int[]>> Shapes = new Dictionary<PieceType, List<Vector2Int[]>>
    {
        { PieceType.I, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(2,0), new(3,0) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2), new(0,3) }
        }},
        { PieceType.O, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(0,1), new(1,1) }
        }},
        { PieceType.T, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(2,0), new(1,1) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2), new(1,1) },
            new Vector2Int[] { new(0,0), new(1,0), new(2,0), new(1,-1) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2), new(-1,1) }
        }},
        { PieceType.L, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(2,0), new(2,1) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2), new(1,0) },
            new Vector2Int[] { new(0,0), new(0,1), new(1,1), new(2,1) },
            new Vector2Int[] { new(0,0), new(1,0), new(1,-1), new(1,-2) }
        }},
        { PieceType.J, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(2,0), new(0,1) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2), new(1,2) },
            new Vector2Int[] { new(0,0), new(0,1), new(1,0), new(2,0) },
            new Vector2Int[] { new(0,0), new(1,0), new(1,1), new(1,2) }
        }},
        { PieceType.S, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(1,1), new(2,1) },
            new Vector2Int[] { new(0,0), new(0,1), new(-1,1), new(-1,2) }
        }},
        { PieceType.Z, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(0,1), new(-1,1) },
            new Vector2Int[] { new(0,0), new(0,1), new(1,1), new(1,2) }
        }},
        // Novas peças
        { PieceType.Dot, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0) }
        }},
        { PieceType.Domino, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0) },
            new Vector2Int[] { new(0,0), new(0,1) }
        }},
        { PieceType.Trio, new List<Vector2Int[]> {
            new Vector2Int[] { new(0,0), new(1,0), new(2,0) },
            new Vector2Int[] { new(0,0), new(0,1), new(0,2) }
        }}
    };

    public static Vector2Int[] GetRandomShape(PieceType type)
    {
        var variants = Shapes[type];
        return variants[Random.Range(0, variants.Count)];
    }

    // 🔹 Novo método: escolhe um tipo de peça aleatório com base no modo
    public static PieceType GetRandomPieceType(bool arcadeMode)
    {
        if (arcadeMode)
            return (PieceType)Random.Range(0, 10); // 0 a 9 (10 peças: I..Z + Dot, Domino, Trio)
        else
            return (PieceType)Random.Range(0, 7);  // apenas as 7 clássicas
    }
}