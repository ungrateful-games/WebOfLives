using UnityEngine;
using System.Collections.Generic;



// Uses the min implementation of the priority queue.
public class PriorityQueue<T> where T: System.IComparable
{
    List<T> Heap;
    Comparer<T> ValueComparer;

    public PriorityQueue()
    {
        this.Heap = new List<T>();
    }

    public void Push( T value)
    {
        this.Heap.Add(value);

    }
    public T Pop()
    {
        T retValue = default(T);
        //return this.Root.Pop();

        if ( this.Heap.Count > 0 )
        {
            retValue = this.Heap[0];
            this.Heap.RemoveAt(0);

            if(this.Heap.Count > 0)
            {
                this.Heap[0] = this.Heap[this.Heap.Count - 1];
                this.Heap.RemoveAt(this.Heap.Count - 1);
            }
        }

        return retValue;
    }

    public void HeapRebuild(int index)
    {
        int smallestChild = 2 * index + 1;
        if (this.Heap.Count > smallestChild)
        {
            if(this.Heap.Count > smallestChild + 1 &&
               this.ValueComparer.Compare(this.Heap[smallestChild], this.Heap[smallestChild]) > 1)
            {
                smallestChild++;
            }

            if(this.ValueComparer.Compare(this.Heap[smallestChild], this.Heap[index]) > 1)
            {
                T temp = this.Heap[smallestChild];
                this.Heap[smallestChild] = this.Heap[index];
                this.Heap[index] = this.Heap[smallestChild];

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
            T parentNode = this.Heap[currentIndex];
            T currentNode;
            do
            {
                // Assign the nodes.
                currentNode = parentNode;
                parentNode = this.Heap[parentIndex];

                // Check to see if current node deserves to be on top more.
                // In a max priority current node's value should be greater than parent node.
                if (this.ValueComparer.Compare(currentNode, parentNode) > 0)
                {
                    this.Heap[currentIndex] = parentNode;
                    this.Heap[parentIndex] = currentNode;

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