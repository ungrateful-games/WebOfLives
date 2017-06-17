
using UnityEngine;

public class VoronoiEvent : System.IComparable
{
    public Point2D Point { get; set; }
    public float Y;
    public bool IsSite;
    public bool IsValid;
    public ParabolaNode parabola;


    public VoronoiEvent(float x, float y, bool isSite)
    {
        this.Point = new Point2D(x, y);
        this.Y = y;
        this.IsSite = isSite;
        this.IsValid = true;
        this.parabola = null;
    }

    public int CompareTo(object obj)
    {
        return Mathf.CeilToInt(this.Y - ((VoronoiEvent)obj).Y);
    }
}
