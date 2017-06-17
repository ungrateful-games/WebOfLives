using UnityEngine;
using System.Collections.Generic;



// Uses the min implementation of the priority queue.
public class PriorityQueue<T> where T: System.IComparable
{
    List<T> _heap;

    //Comparer<T> ValueComparer;
    public int Count
    {
        get { return this._heap.Count; }
    }
    
    public PriorityQueue()
    {
        this._heap = new List<T>();
    }

    public void Push( T value)
    {
        this._heap.Add(value);

    }
    public T Pop()
    {
        T retValue = default(T);
        //return this.Root.Pop();

        if ( this._heap.Count > 0 )
        {
            retValue = this._heap[0];
            this._heap.RemoveAt(0);

            if(this._heap.Count > 0)
            {
                this._heap[0] = this._heap[this._heap.Count - 1];
                this._heap.RemoveAt(this._heap.Count - 1);
            }
        }

        return retValue;
    }

    public void HeapRebuild(int index)
    {
        int smallestChild = 2 * index + 1;
        if (this._heap.Count > smallestChild)
        {
            if(this._heap.Count > smallestChild + 1 &&
               this._heap[smallestChild].CompareTo(this._heap[smallestChild + 1]) > 0)
            {
                smallestChild++;
            }

            if(this._heap[smallestChild].CompareTo( this._heap[index]) > 0)
            {
                T temp = this._heap[smallestChild];
                this._heap[smallestChild] = this._heap[index];
                this._heap[index] = this._heap[smallestChild];

                this.HeapRebuild(smallestChild);
            }
        }
    }

    public void TrickleUp(int index)
    {
        // Compute the parent of this node.
        int parentIndex = Mathf.FloorToInt((index - 1) / 2);
        int currentIndex = index;

        // The root of the heap is always good.
        if(currentIndex > 0)
        {
            // This implementation aliases the heap locations.
            T parentNode = this._heap[currentIndex];
            T currentNode;
            do
            {
                // Assign the nodes.
                currentNode = parentNode;
                parentNode = this._heap[parentIndex];

                // Check to see if current node deserves to be on top more.
                // In a max priority current node's value should be greater than parent node.
                if (currentNode.CompareTo(parentNode) > 0)
                {
                    this._heap[currentIndex] = parentNode;
                    this._heap[parentIndex] = currentNode;

                    currentIndex = parentIndex;
                    parentIndex  = Mathf.FloorToInt((currentIndex - 1) / 2); 
                }
                else
                {
                    currentIndex = -1;
                }

            } while (currentIndex > 0);

        }
    }
}