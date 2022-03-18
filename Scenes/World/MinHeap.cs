using System;
using System.Collections.Generic;
using Godot;

public class MinHeap<TElement, TPriority>
{
    private List<(TElement, TPriority)> _heap;
    private IComparer<TPriority> _comparer;

    public int Count { get { return _heap.Count; } }

    public MinHeap() : this(Comparer<TPriority>.Default) { }
    public MinHeap(IComparer<TPriority> comparer)
    {
        _heap = new List<(TElement, TPriority)>();
        _comparer = comparer;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        _heap.Add((element, priority));
        SwimUp(_heap.Count - 1);
    }

    public TElement Dequeue()
    {
        var last = _heap.Count - 1;
        Swap(0, last);
        var elem = _heap[last].Item1;
        _heap.RemoveAt(last);
        SinkDown(0);
        return elem;
    }

    private void SwimUp(int index)
    {
        var current = index;
        var parent = Parent(current);
        while (Compare(current, parent) < 0)
        {
            Swap(current, parent);
            current = parent;
            parent = Parent(current);
        }
    }
    private void SinkDown(int index)
    {
        if (IsLeaf(index))
        {
            return;
        }
        var leftChild = LeftChild(index);
        if (Compare(index, leftChild) > 0)
        {
            Swap(index, leftChild);
            SinkDown(leftChild);
            return;
        }
        var rightChild = RightChild(index);
        if (Compare(index, rightChild) > 0)
        {
            Swap(index, rightChild);
            SinkDown(rightChild);
            return;
        }
    }
    private int Compare(int i1, int i2)
    {
        return _comparer.Compare(_heap[i1].Item2, _heap[i2].Item2);
    }
    private void Swap(int i1, int i2)
    {
        var temp = _heap[i1];
        _heap[i1] = _heap[i2];
        _heap[i2] = temp;
    }
    private bool IsLeaf(int index)
    {
        return index >= (_heap.Count - 1) / 2;
    }
    private int Parent(int index)
    {
        return (index - 1) / 2;
    }
    private int LeftChild(int index)
    {
        return index * 2 + 1;
    }
    private int RightChild(int index)
    {
        return index * 2 + 2;
    }
}