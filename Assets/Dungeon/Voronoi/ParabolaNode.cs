
public class ParabolaNode
{
    public Point2D Point { get; set; }
    public VoronoiEvent Event { get; set; }

    public float Key
    {
        get { return Point.X; }
    }

    public ParabolaNode Parent;
    public ParabolaNode Right;
    public ParabolaNode Left;

    public VoronoiEvent CircleEvent;
    // TODO Edge

    public bool Red { get; set; }

    public ParabolaNode()
    {
        this.Point = null;
        this.Red = false;
        this.CircleEvent = null;
    }

    public ParabolaNode(Point2D point)
    {
        this.Point = point;
        this.Red = true;
        this.CircleEvent = null;

    }
}