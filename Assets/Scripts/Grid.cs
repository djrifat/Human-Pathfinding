using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    // Grid 
    public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
    int gridSizeX, gridSizeY;
    Node[,] grid;

    // Node 
    float nodeDiameter;

    // Terrain 
    public Terraintype[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();    // containing walkable regions <int key, int value>

    // Raycast
    int fireHeight = 50;
    float rayDistance = 100;

    // Movement Penalty
    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;
    public int obstacleProximityPenalty = 10;

    Unit unit;


    private void Awake() {

        unit = GetComponent<Unit>();

        // Set grid and node size
        // Devide by nodeDiameter to calculate how many nodes fit in the grid
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (Terraintype region in walkableRegions) {
            walkableMask.value |= region.terrainMask.value;    // Convert layer value to int and add to walkableMask value (using bitwise OR)
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2) ,region.terrainPenalty); // Dict key = Layer number
        }
        CreateGrid();   // Create grid
    }

    /// <summary>
    /// Return max grid size
    /// </summary>
    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }

    /// <summary>
    /// Create 2D grid
    /// </summary>
    void CreateGrid() {

        grid = new Node[gridSizeX, gridSizeY];  // 2D array of nodes
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;  // Get bottom left corner

        // Loop through all grid positions to see if they are walkable
        // As x increases, it increments along in node diameter. Untill it reaches the edge
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;   // Set movement penalty value to zero

                // Raycast
                Ray ray = new Ray(worldPoint + Vector3.up * fireHeight, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, rayDistance, walkableMask)) {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable) {
                    movementPenalty += obstacleProximityPenalty;
                }
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
        BlurPenaltyMap(3);
    }


    void BlurPenaltyMap(int blurSize) {
        int kernelSize = blurSize * 2 + 1;          // Must be uneven number
        int kernelExtents = (kernelSize - 1) / 2;   // Number of squares between center square and edge square

        // Horizontal and Vertical pass
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        // Horizontal pass
        for (int y = 0; y < gridSizeY; y++) {
            for (int x = -kernelExtents; x <= kernelExtents; x++) {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);                         // Clamp X to zero, when it's negative
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;      // Adds node to value
            }

            // Loop remaining nodes
            for (int x = 1; x < gridSizeX; x++) {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        // Vertical pass
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = -kernelExtents; y <= kernelExtents; y++) {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++) {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax) {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin) {
                    penaltyMin = blurredPenalty;
                }
            }
        }

    }

    /// <summary>
    /// Get neighbour nodes on the grid
    /// </summary>
    /// <param name="node"></param>
    /// <returns> Returns a list of neighbour nodes </returns>
    public List<Node> GetNeighbours(Node node) {
        // Initialize new neighbours node list
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;
       
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    /// <summary>
    /// Returns a node
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns> 2D grid with a node position </returns>
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // Substract one so its not outside the array
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    /// <summary>
    /// Draw Gizmo's
    /// </summary>
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        // Draw Gizmos is the grid is filled && Gizmos are allowed to be displayed
        if (grid != null && displayGridGizmos) {
            foreach (Node n in grid) {

                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));

                // '?' = "then", ':' = "else"
                Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
            }
        }
    }

    [System.Serializable]
    public class Terraintype {
        // Terrain variables
        public LayerMask terrainMask;
        public int terrainPenalty;
    }


}


