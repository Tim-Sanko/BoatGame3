using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public BoatController enemyBoat;   // assign in inspector
    public BoatController playerBoat;  // assign in inspector
    public BoatPathfinder pathfinder;  // assign in inspector

    /// <summary>
    /// Call this during enemy turn to plan moves
    /// </summary>
    public void TakeTurn()
    {
        if (enemyBoat == null || pathfinder == null || playerBoat == null)
            return;

        // Pick a target near the player (not the exact cell)
        Vector3Int[] dirs = enemyBoat.GetDirs(playerBoat.currentCell.y);
        Vector3Int target = playerBoat.currentCell + dirs[Random.Range(0, dirs.Length)];

        pathfinder.SetTarget(target);
        List<Vector3Int> path = pathfinder.FindPath();

        if (path == null || path.Count == 0)
            return;

        ConvertPathToCommands(path);
    }

    private void ConvertPathToCommands(List<Vector3Int> path)
    {
        int facing = enemyBoat.GetFacing();
        Vector3Int current = enemyBoat.currentCell;

        enemyBoat.commandQueue.Clear();
        enemyBoat.fireQueue.Clear();

        foreach (var step in path)
        {
            Vector3Int offset = step - current;
            Vector3Int[] dirs = enemyBoat.GetDirs(current.y);

            int dirIndex = -1;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (dirs[i] == offset)
                {
                    dirIndex = i;
                    break;
                }
            }

            if (dirIndex != -1)
            {
                // Rotate to face the direction
                int rotationSteps = (dirIndex - facing + 6) % 6;
                if (rotationSteps == 1)
                    enemyBoat.AddCommand(new BoatCommand(BoatCommandType.RotateRight));
                else if (rotationSteps == 5)
                    enemyBoat.AddCommand(new BoatCommand(BoatCommandType.RotateLeft));

                // Move forward
                enemyBoat.AddCommand(new BoatCommand(BoatCommandType.Forward));
                facing = dirIndex;
            }

            current = step;

            // Add random fire command using your FireCommand class
            FireCommandType fireType = (FireCommandType)Random.Range(0, 5);
            enemyBoat.AddFireCommand(new FireCommand(fireType));
        }

        // Pad commands to max
        while (enemyBoat.commandQueue.Count < enemyBoat.maxCommands)
            enemyBoat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
        while (enemyBoat.fireQueue.Count < enemyBoat.maxFireCommands)
            enemyBoat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
    }
}
