using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoatPathfinder : MonoBehaviour
{
    public Tilemap tilemap;
    public BoatController boat;  // assign in inspector

    private Vector3Int targetCell;

    public void SetTarget(Vector3Int target)
    {
        targetCell = target;
    }

    /// <summary>
    /// BFS pathfinding from boat's current cell to target
    /// Avoids occupied tiles
    /// </summary>
    public List<Vector3Int> FindPath()
    {
        if (tilemap == null || boat == null)
            return null;

        Vector3Int start = boat.currentCell;
        Vector3Int goal = targetCell;

        Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        frontier.Enqueue(start);

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if (current == goal)
                break;

            Vector3Int[] neighbors = boat.GetDirs(current.y);
            foreach (Vector3Int offset in neighbors)
            {
                Vector3Int next = current + offset;

                if (!tilemap.HasTile(next)) continue;
                if (cameFrom.ContainsKey(next)) continue;
                if (IsOccupied(next)) continue;

                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        // Reconstruct path
        List<Vector3Int> path = new List<Vector3Int>();
        if (!cameFrom.ContainsKey(goal)) return path;

        Vector3Int temp = goal;
        while (temp != start)
        {
            path.Insert(0, temp);
            temp = cameFrom[temp];
        }

        // Limit path to boat's maxCommands
        if (path.Count > boat.maxCommands)
            path = path.GetRange(0, boat.maxCommands);

        return path;
    }

    private bool IsOccupied(Vector3Int cell)
    {
        foreach (BoatController other in TurnManager.Instance.boats)
        {
            if (other != boat && other.currentCell == cell)
                return true;
        }
        return false;
    }
}

