using UnityEngine;
using System.Collections;
using System;

public class Node : IHeapItem<Node> {

    // Other Variables
    public bool walkable;
    public Vector3 worldPosition;
    public int movementPenalty;

    // Grid variables
    public int gridX;
    public int gridY;

    // A* parameters
    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_walkable"></param>
    /// <param name="_worldPos"></param>
    /// <param name="_gridX"></param>
    /// <param name="_gridY"></param>
    /// <param name="_penalty"></param>
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty) {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    /// <summary>
    /// Returns calculated f-cost value
    /// </summary>
    public int fCost {
        get { return gCost + hCost; }
    }

    /// <summary>
    /// HeapIndex, get heapindex and set value for it
    /// </summary>
    public int HeapIndex {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    /// <summary>
    /// Compare nodes with eachother
    /// </summary>
    /// <param name="nodeToCompare"></param>
    /// <returns> Returns compare variable (1 if it's lower)</returns>
    public int CompareTo(Node nodeToCompare) {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }


}