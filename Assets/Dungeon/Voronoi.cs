using UnityEngine;
using UnityEditor;


public class Parabola
{
    public float x;
    public float y;

    public Parabola()
    {
    }
}
public class VoronoiEvent : System.IComparable
{

    public float X;
    public float Y;
    public bool IsSite;
    public Parabola parabola;


    public VoronoiEvent(float x, float y, bool isSite)
    {
        this.X = x;
        this.Y = y;
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

   

    public Voronoi()
    {
        this._events = new PriorityQueue<VoronoiEvent>(); 
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

    }

    private void ProcessParabola(VoronoiEvent vEvent)
    {

    }
    
}