using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    // Class objects
    PathRequestManager requestManager;
    Grid grid;

    public float nrOfSecondsToWait = 1;

    private void Awake() {
        // Initialize new objects
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }

    /// <summary>
    /// Start FindPath coroutine
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="targetPos"></param>
    public void StartFindPath (Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }


    /// <summary>
    /// Find path from current node to the target node
    /// </summary>
    /// <param name="startpos"></param>
    /// <param name="targetPos"></param>
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

        // FinishedProcessingPath variables
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        // Set starting and target node
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        // Start pathfinding if both nodes are walkable
        if (startNode.walkable && targetNode.walkable) {
            // List containing OPEN and CLOSED set
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            // Loop through nodes
            // Set node with lowest f_cost in OPEN set
            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                // Path has been found, Return
                if (currentNode == targetNode) {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;  // Exit pathfinding loop
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                        continue;
                    }

                    // Check if new path to neighbour is shorter or neighbour is not in OPEN
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        // Set f_cost of neighbour and set parent of neighbour to current
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        // Add parent to OPEN set
                        if (!openSet.Contains(neighbour)) {
                            openSet.Add(neighbour);
                        }
                        else {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(nrOfSecondsToWait);  // Wait one second before returning
        //yield return null;
        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

    }

    /// <summary>
    /// Retrace Node path
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns> Returns List of waypoints </returns>
    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);   // Retracing is done backwards. waypoints has to be reversed back
        return waypoints;
        
    }

    
    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i ++) {
            Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY); 
            if (directionNew != directionOld) {
                waypoints.Add(path[i].worldPosition);   // Add new waypoint to the List
            }
            directionOld = directionNew;   // Set OldDirection equal to NewDirection
        }
        return waypoints.ToArray();   // Return waypoints list as an Array
    }   

    
    /// <summary>
    /// Get distance between two nodes
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns> Returns the distance between two nodes </returns>
    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


}
