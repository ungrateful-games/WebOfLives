
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

            // Only process Valid Events.
            if (ve.IsValid)
            {
                if (ve.IsSite) ProcessSite(ve);
                else ProcessParabola(ve);
            }
        }
    }

    private void ProcessSite(VoronoiEvent vEvent)
    {
        ParabolaNode eventParabola = new ParabolaNode(vEvent.Point);

        // If the beachline is empty insert a new parabola.
        if(this._beachLine.IsEmpty())
        {
            this._beachLine.Insert(eventParabola);
            return;
        }

        // Find the parabola that this point collides with.
        this._beachLine.AddSiteParabola(eventParabola);
    }

    private void ProcessParabola(VoronoiEvent vEvent)
    {

    }
    
}