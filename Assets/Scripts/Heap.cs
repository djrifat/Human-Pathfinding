using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
                    
public class Heap<T> where T : IHeapItem<T> {

    T[] items;
    int currentItemCount;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="maxHeapSize"></param>
    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }

    /// <summary>
    /// Add items to items heap
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item) {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;         // Place item at the end of the array
        SortUp(item);
        currentItemCount++;
    }

    /// <summary>
    /// Remove first item from heap
    /// Takes item at the end of the heap and put it into first place
    /// </summary>
    /// <returns>Returns Type T (firstItem)</returns>
    public T RemoveFirst() {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    /// <summary>
    /// Change priority of item
    /// Update priority in the Heap
    /// </summary>
    /// <param name="item"></param>
    public void UpdateItem(T item) {
        SortUp(item);
    }

    /// <summary>
    /// Get number of items currently in the Heap
    /// </summary>
    /// <returns> currentItemCount </returns>
    public int Count {
        get {
            return currentItemCount;
        }
    }

    /// <summary>
    /// Check if heap contains specific item
    /// </summary>
    /// <param name="item"></param>
    /// <returns> TRUE if items is equal | otherwise FALSE </returns>
    public bool Contains(T item) {
        return Equals(items[item.HeapIndex], item);
    }

    /// <summary>
    /// Sortdown item priority
    /// </summary>
    /// <param name="item"></param>
    void SortDown(T item) {
        while (true) {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            // Check child values and swap where condition is met
            if (childIndexLeft < currentItemCount) {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount) {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) {
                        swapIndex = childIndexRight;
                    }
                }
                if (item.CompareTo(items[swapIndex]) < 0) {
                    Swap(item, items[swapIndex]);
                }
                else {
                    return;
                }
            }
            else {
                return;
            }
        }
    }


    /// <summary>
    /// Sortup item priority
    /// </summary>
    /// <param name="item"></param>
    void SortUp(T item) {
        int parentIndex = (item.HeapIndex - 1) / 2;

        // Return 1 if it's higher priority --> swap item
        // Return 0 if it's same priority
        // Return -1 if it's lower priority
        while (true) {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0) {
                Swap(item, parentItem);
            }
            else {      // break if no longer higher priorty then parent item
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    /// <summary>
    /// Swap nodes with eachother
    /// </summary>
    /// <param name="itemA"></param>
    /// <param name="itemB"></param>
    void Swap(T itemA, T itemB) {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }

}

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}   