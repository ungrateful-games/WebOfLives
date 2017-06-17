
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

    /**
     * @brief The Parabolas are defined by the Leaves of the tree.
     * 
     * @return The leaf that most closely matches the supplied point.
     * 
     */
    public ParabolaNode FindCollidedParabola(float key)
    {
        ParabolaNode node = this._root;
        ParabolaNode prevNode = node;

        while (node != this._nil )
        {
            prevNode = node;
            if (node.Key > key) node = node.Left;
            else node = node.Right;
        }

        return prevNode;

    }

    /**
     * @brief Processes a "Site Parabola"
     * 
     * 1. Retrieve the Site that this Parabola should be intersecting.
     * 2. If the event had a circle event it is set to invalid, as the queue should have processed this event before this process.
     * 3. A new Inner Parabola must be created to signify an intersection.
     */
    public void AddSiteParabola(ParabolaNode newSite)
    {
        ParabolaNode oldSite = this.FindCollidedParabola(newSite.Key);

        // Handle fake circle events (don't rip from the PQueue, just flag as bad and move along.
        if (oldSite.CircleEvent != null) oldSite.CircleEvent.IsValid = false;

        #region insert
        // This new node should replace the oldSite in the tree.
        ParabolaNode intersection = new ParabolaNode(); // TODO what site should this store?

        // Make a valid Red Black tree to insert.
        intersection.Red = false;

        // New site is red and a leaf.
        newSite.Red = true;
        newSite.Left = newSite.Right = this._nil;

        // Make the old site red regardless, we'll be inserting the intersection in a bit.
        oldSite.Red = true;

        // Old site is guaranteed to be a leaf.
        if (oldSite.Parent.Right == oldSite) oldSite.Parent.Right = this._nil;
        else oldSite.Parent.Left = this._nil;

        // Assign the parents.
        newSite.Parent = oldSite.Parent = intersection;

        // Insert the nodes in the appropriate locations.
        if ( newSite.Key > oldSite.Key ) 
        {
            intersection.Right = newSite;
            intersection.Left  = oldSite;
        }
        else
        {
            intersection.Left  = newSite;
            intersection.Right = oldSite;
        }

        // Insert the Proper Red Black Tree and let the BST balance itself.
        this.Insert(intersection);
        #endregion

        // Need to structure the insert so the inner/leaf property is preserved.

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

        while (currTarget.Parent.Red)
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

        if (removalNode.Left == this._nil)
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

        while (current != this._nil)
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

        if (y.Left != this._nil)
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