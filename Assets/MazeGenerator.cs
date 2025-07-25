using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Génère un labyrinthe en plaçant des cellules sur une grille et en supprimant des murs pour créer des chemins.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private MazeCell _mazeCellPrefab; // Préfabriqué pour les cellules du labyrinthe

    [SerializeField]
    private int _mazeWidth; // Largeur du labyrinthe

    [SerializeField]
    private int _mazeDepth; // Profondeur du labyrinthe

    [SerializeField]
    private float _scaleFactorX = 2f; // Facteur d'échelle pour l'axe X
    [SerializeField]
    private float _scaleFactorY = 1f; // Facteur d'échelle pour l'axe Y
    [SerializeField]
    private float _scaleFactorZ = 2f; // Facteur d'échelle pour l'axe Z

    [SerializeField]
    private GameObject startCubePrefab; // Préfabriqué pour le cube de début

    [SerializeField]
    private GameObject middleCubePrefab; // Préfabriqué pour le cube du milieu

    [SerializeField]
    private GameObject endCubePrefab; // Préfabriqué pour le cube de fin

    private MazeCell[,] _mazeGrid; // Grille contenant les cellules du labyrinthe

    /// <summary>
    /// Initialise la génération du labyrinthe et configure les cellules de départ et de fin.
    /// </summary>
    IEnumerator Start()
    {
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                // Positionne les cellules en fonction des facteurs d'échelle
                Vector3 position = new Vector3(x * _scaleFactorX, 0, z * _scaleFactorZ);

                // Instancie une cellule et ajuste son échelle pour la représentation visuelle
                MazeCell mazeCell = Instantiate(_mazeCellPrefab, position, Quaternion.identity);
                mazeCell.transform.localScale = new Vector3(_scaleFactorX, _scaleFactorY, _scaleFactorZ);
                mazeCell._currentWallType = UserSessionManager.Instance.WallType;
                _mazeGrid[x, z] = mazeCell;
            }
        }

        yield return GenerateMaze(null, _mazeGrid[0, 0]);

        SetStartAndEnd();
    }

    /// <summary>
    /// Configure les cellules de départ et de fin, et place les objets associés.
    /// </summary>
    private void SetStartAndEnd()
    {
        MazeCell startCell = _mazeGrid[0, 0];
        MazeCell endCell = _mazeGrid[_mazeWidth - 1, _mazeDepth - 1];

        startCell.SetCellType(MazeCell.CellType.Start);
        startCell.RemoveWall(MazeCell.Direction.Left);
        SpawnCubeAboveCell(startCell, startCubePrefab);

        endCell.SetCellType(MazeCell.CellType.End);
        endCell.RemoveWall(MazeCell.Direction.Right);
        SpawnCubeAboveCell(endCell, endCubePrefab);

        // Détermine le chemin principal et place un cube au milieu
        List<MazeCell> path = FindPath(startCell, endCell);
        if (path != null && path.Count > 2)
        {
            MazeCell middleCell = path[path.Count / 2];
            SpawnCubeAboveCell(middleCell, middleCubePrefab);
        }
    }

    /// <summary>
    /// Place un cube au-dessus d'une cellule spécifique.
    /// </summary>
    private GameObject SpawnCubeAboveCell(MazeCell cell, GameObject cubePrefab)
    {
        if (cubePrefab == null || cell == null)
            return null;

        // Gère le spawn XYZ
        Vector3 spawnPosition = cell.transform.position + new Vector3(0, 0.5f, 0); // 1.5 unités au-dessus de la cellule
        GameObject cube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
        
        return cube;
    }

    /// <summary>
    /// Génère le labyrinthe en supprimant les murs entre les cellules connectées.
    /// L'algorithme utilisé est une variante de l'algorithme de Prim.
    /// </summary>
    private IEnumerator GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        while (true)
        {
            MazeCell nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell == null)
                yield break;

            yield return GenerateMaze(currentCell, nextCell);
        }
    }

    /// <summary>
    /// Récupère une cellule voisine non visitée.
    /// </summary>
    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        List<MazeCell> unvisitedCells = GetUnvisitedCells(currentCell);

        if (unvisitedCells.Count == 0)
            return null;

        return unvisitedCells.OrderBy(_ => Random.Range(0, 100)).First();
    }

    /// <summary>
    /// Récupère toutes les cellules voisines non visitées.
    /// </summary>
    private List<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        List<MazeCell> unvisitedCells = new List<MazeCell>();

        int x = Mathf.FloorToInt(currentCell.transform.position.x / _scaleFactorX);
        int z = Mathf.FloorToInt(currentCell.transform.position.z / _scaleFactorZ);

        if (x + 1 < _mazeWidth && !_mazeGrid[x + 1, z].IsVisited)
            unvisitedCells.Add(_mazeGrid[x + 1, z]);

        if (x - 1 >= 0 && !_mazeGrid[x - 1, z].IsVisited)
            unvisitedCells.Add(_mazeGrid[x - 1, z]);

        if (z + 1 < _mazeDepth && !_mazeGrid[x, z + 1].IsVisited)
            unvisitedCells.Add(_mazeGrid[x, z + 1]);

        if (z - 1 >= 0 && !_mazeGrid[x, z - 1].IsVisited)
            unvisitedCells.Add(_mazeGrid[x, z - 1]);

        return unvisitedCells;
    }

    /// <summary>
    /// Supprime les murs entre deux cellules connectées.
    /// </summary>
    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
            return;

        int prevX = Mathf.FloorToInt(previousCell.transform.position.x / _scaleFactorX);
        int prevZ = Mathf.FloorToInt(previousCell.transform.position.z / _scaleFactorZ);
        int currX = Mathf.FloorToInt(currentCell.transform.position.x / _scaleFactorX);
        int currZ = Mathf.FloorToInt(currentCell.transform.position.z / _scaleFactorZ);

        if (prevX < currX)
        {
            previousCell.RemoveWall(MazeCell.Direction.Right);
            currentCell.RemoveWall(MazeCell.Direction.Left);
            return;
        }

        if (prevX > currX)
        {
            previousCell.RemoveWall(MazeCell.Direction.Left);
            currentCell.RemoveWall(MazeCell.Direction.Right);
            return;
        }

        if (prevZ < currZ)
        {
            previousCell.RemoveWall(MazeCell.Direction.Front);
            currentCell.RemoveWall(MazeCell.Direction.Back);
            return;
        }

        if (prevZ > currZ)
        {
            previousCell.RemoveWall(MazeCell.Direction.Back);
            currentCell.RemoveWall(MazeCell.Direction.Front);
            return;
        }
    }

    /// <summary>
    /// Trouve un chemin entre deux cellules en utilisant une recherche en largeur.
    /// </summary>
    private List<MazeCell> FindPath(MazeCell start, MazeCell end)
    {
        Dictionary<MazeCell, MazeCell> cameFrom = new Dictionary<MazeCell, MazeCell>();
        Queue<MazeCell> queue = new Queue<MazeCell>();
        HashSet<MazeCell> visited = new HashSet<MazeCell>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            MazeCell current = queue.Dequeue();
            if (current == end)
                break;

            foreach (MazeCell neighbor in GetConnectedNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        List<MazeCell> path = new List<MazeCell>();
        MazeCell step = end;

        while (step != start)
        {
            path.Add(step);
            if (!cameFrom.ContainsKey(step)) return new List<MazeCell>();
            step = cameFrom[step];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Récupère les voisins connectés d'une cellule.
    /// </summary>
    private List<MazeCell> GetConnectedNeighbors(MazeCell cell)
    {
        List<MazeCell> neighbors = new List<MazeCell>();
        int x = Mathf.FloorToInt(cell.transform.position.x / _scaleFactorX);
        int z = Mathf.FloorToInt(cell.transform.position.z / _scaleFactorZ);

        if (!cell.HasWall(MazeCell.Direction.Right) && x + 1 < _mazeWidth)
            neighbors.Add(_mazeGrid[x + 1, z]);
        if (!cell.HasWall(MazeCell.Direction.Left) && x - 1 >= 0)
            neighbors.Add(_mazeGrid[x - 1, z]);
        if (!cell.HasWall(MazeCell.Direction.Front) && z + 1 < _mazeDepth)
            neighbors.Add(_mazeGrid[x, z + 1]);
        if (!cell.HasWall(MazeCell.Direction.Back) && z - 1 >= 0)
            neighbors.Add(_mazeGrid[x, z - 1]);

        return neighbors;
    }
}