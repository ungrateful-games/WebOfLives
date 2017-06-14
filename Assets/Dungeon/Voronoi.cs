using UnityEngine;
using UnityEditor;

public class Point2D
{
    public float X { get; set; }
    public float Y { get; set; }

    public Point2D(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }
}


public class ParabolaNode
{
    public Point2D Point { get; set; }
    public VoronoiEvent Event { get; set; }

    public float Key
    {
        get { return Point.Y; }
    }

    public ParabolaNode Parent;
    public ParabolaNode Right;
    public ParabolaNode Left;
    bool _isLeaf;
    public bool Red { get; set; }

    public ParabolaNode()
    {
        this.Point = null;
        this._isLeaf = true;
        this.Red = false;
    }

    public ParabolaNode(Point2D point)
    {
        this.Point = point;
        this._isLeaf = false;
        this.Red = true;
    }
 }


public class Beachline
{
    public ParabolaNode _root;
    public ParabolaNode _nil;

    public Beachline()
    {
        this._nil = new ParabolaNode();
        this._root = this._nil;
    }

    public bool IsEmpty()
    {
        return this._root == this._nil;
    }

    public void Insert(ParabolaNode newNode)
    {
        ParabolaNode prevNode = this._nil;
        ParabolaNode currNode = this._root;

        while (currNode != this._nil)
        {
            prevNode = currNode;

            if (newNode.Key < currNode.Key)
                currNode = currNode.Left;
            else
                currNode = currNode.Right;
        }

        newNode.Parent = prevNode;

        if (prevNode == this._nil)
            this._root = newNode;
        else if (newNode.Key < prevNode.Key)
            prevNode.Left = newNode;
        else
            prevNode.Right = newNode;

        newNode.Right = this._nil;
        newNode.Left = this._nil;
        newNode.Red = true;

        this.FixInsert(newNode);
    }

    public void FixInsert(ParabolaNode fixNode)
    {
        ParabolaNode currTarget = fixNode;
        ParabolaNode sibling = this._nil; /// A Sentinal value for checks, this node is "black" reducing the number of checks.

        while ( currTarget.Parent.Red ) 
        {
            if (currTarget.Parent == currTarget.Parent.Parent.Left)
            {
                sibling = currTarget.Parent.Parent.Right; // Get the parent's sibling.
                if (sibling.Red)
                {
                    currTarget.Parent.Red = false;
                    sibling.Red = false;
                    sibling.Parent.Red = true;
                }
                else 
                {
                    if (currTarget == currTarget.Parent.Right)
                    {
                        currTarget = currTarget.Parent;
                        this.LeftRotate(currTarget);
                    }
                    currTarget.Parent.Red = false;
                    currTarget.Parent.Parent.Red = true;
                    RightRotate(currTarget.Parent.Parent);
                }
            }
            else
            {
                sibling = currTarget.Parent.Parent.Left; // Get the parent's sibling.
                if (sibling.Red)
                {
                    currTarget.Parent.Red = false;
                    sibling.Red = false;
                    sibling.Parent.Red = true;
                }
                else
                {
                    if (currTarget == currTarget.Parent.Left)
                    {
                        currTarget = currTarget.Parent;
                        RightRotate(currTarget);
                    }
                    currTarget.Parent.Red = false;
                    currTarget.Parent.Parent.Red = true;
                    LeftRotate(currTarget.Parent.Parent);
                }
            }
        }

        this._root.Red = false;
    }

    public void Delete(ParabolaNode removalNode)
    {
        ParabolaNode x;
        ParabolaNode y = removalNode;
        bool yOrigColor = removalNode.Red;

        if( removalNode.Left == this._nil)
        {
            x = removalNode.Right;
            this.Transplant(removalNode, removalNode.Right);
        }
        else if (removalNode.Right == this._nil)
        {
            x = removalNode.Left;
            this.Transplant(removalNode, removalNode.Left);
        }
        else
        {
            y = this.TreeMin(removalNode.Right);
            yOrigColor = y.Red;
            x = y.Right;

            if (y.Parent == removalNode)
                x.Parent = y;
            else
            {
                this.Transplant(y, y.Right); // Put x in the position of y.
                // Give y the removed node's right tree.
                y.Right = removalNode.Right;
                y.Right.Parent = y;
            }

            this.Transplant(removalNode, y); // Move y into removal node's pposition.
            y.Left = removalNode.Left;
            y.Left.Parent = y;
            y.Red = removalNode.Red;
        }

        // If the original color of the replacement node was black we need to fix the tree because it's in violation of the red black property.
        if (!yOrigColor)
        {
            FixDelete(x);
        }

    }

    public void FixDelete(ParabolaNode fixNode)
    {
        ParabolaNode x = fixNode;
        while (x != this._root && !x.Red)
        {
            if (x == x.Parent.Left)
            {
                ParabolaNode sibling = x.Parent.Left;
                if (sibling.Red)
                {
                    sibling.Red = false;
                    x.Parent.Red = true;
                    this.LeftRotate(x.Parent);
                    sibling = x.Parent.Right;
                }

                if (!(sibling.Left.Red || sibling.Right.Red))
                {
                    sibling.Red = true;
                    x = x.Parent;
                }
                else
                {
                    if (!sibling.Right.Red)
                    {
                        sibling.Left.Red = false;
                        sibling.Red = true;
                        this.RightRotate(sibling);
                        sibling = x.Parent.Right;
                    }
                    sibling.Red = x.Parent.Red;
                    x.Parent.Red = false;
                    sibling.Right.Red = false;
                    this.RightRotate(x.Parent);
                    x = this._root;
                }
            }
            else
            {
                ParabolaNode sibling = x.Parent.Right;
                if (sibling.Red)
                {
                    sibling.Red = false;
                    x.Parent.Red = true;
                    this.RightRotate(x.Parent);
                    sibling = x.Parent.Left;
                }

                if (!(sibling.Left.Red || sibling.Right.Red))
                {
                    sibling.Red = true;
                    x = x.Parent;
                }
                else
                {
                    if (!sibling.Left.Red)
                    {
                        sibling.Right.Red = false;
                        sibling.Red = true;
                        this.LeftRotate(sibling);
                        sibling = x.Parent.Left;
                    }
                    sibling.Red = x.Parent.Red;
                    x.Parent.Red = false;
                    sibling.Left.Red = false;
                    this.LeftRotate(x.Parent);
                    x = this._root;
                }
            }            
        }

        x.Red = false;
    }

    public void Transplant(ParabolaNode orig, ParabolaNode replacement)
    {
        if (orig.Parent == this._nil)
            this._root = replacement;
        else if (orig == orig.Parent.Left)
            orig.Parent.Left = replacement;
        else orig.Parent.Right = replacement;

        replacement.Parent = orig.Parent;
    }

    public ParabolaNode TreeMin(ParabolaNode tree)
    {
        ParabolaNode prev = this._nil;
        ParabolaNode current = tree.Left;

        while(current != this._nil)
        {
            prev = current;
            current = current.Left;
        }

        return prev;
    }

    public void LeftRotate(ParabolaNode x)
    {
        ParabolaNode y = x.Right;
        x.Right = y.Left;

        if(y.Left != this._nil)
        {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;

        if (x.Parent == this._nil) // No parent.
            this._root = y;
        else if (x == x.Parent.Left) // Left child of parent.
            x.Parent.Left = y;
        else
            x.Parent.Right = y;
        y.Left = x;
        x.Parent = y.Parent;
    }

    public void RightRotate(ParabolaNode y)
    {
        ParabolaNode x = y.Left;
        y.Left = x.Right;

        if (x.Right != this._nil)
        {
            x.Right.Parent = y;
        }

        x.Parent = y.Parent;

        if (y.Parent == this._nil) // No parent.
            this._root = x;
        else if (y == y.Parent.Left) // Left child of parent.
            y.Parent.Left = x;
        else
            y.Parent.Right = x;
        x.Right = y;
        y.Parent = x.Parent;
    }
}

public class VoronoiEvent : System.IComparable
{
    public Point2D Point { get; set; }
    public float X;
    public bool IsSite;
    public ParabolaNode parabola;


    public VoronoiEvent(float x, float y, bool isSite)
    {
        this.Point = new Point2D(x, y);
        this.X = x;
        this.IsSite = isSite;
        this.parabola = null;
    }

    // Sorts compare to sorts lowest to highest deliberately.
    public int CompareTo(object obj)
    {
        return Mathf.CeilToInt(((VoronoiEvent)obj).X - this.X);
    }
}



public class Voronoi 
{
    PriorityQueue<VoronoiEvent> _events;
    Beachline _beachLine;


    public Voronoi()
    {
        this._events = new PriorityQueue<VoronoiEvent>();
        this._beachLine = new Beachline();
    }

    public void PushSite(int x, int y)
    {
        this._events.Push(new VoronoiEvent(x, y, true));
    }

    public void CreateEdges()
    {
        while(this._events.Count > 0)
        {
            VoronoiEvent ve = this._events.Pop();

            if (ve.IsSite) ProcessSite(ve);
            else ProcessParabola(ve);
        }
    }

    private void ProcessSite(VoronoiEvent vEvent)
    {
        // If the beachline is empty insert a new parabola.
        if(this._beachLine.IsEmpty())
        {
            this._beachLine.Insert(new ParabolaNode(vEvent.Point));
            return;
        }

        // FIXME You were here.

    }

    private void ProcessParabola(VoronoiEvent vEvent)
    {

    }
    
}