using UnityEngine;
using System.Collections;



public class Room
{
    Rect _dimensions;
    public Rect Dimensions
    {
        get { return _dimensions; }
        set { _dimensions = value; }
    }

    int RoomDetails;

    // Room tree.
    Room RightRoom;
    Room DownRoom;

    // 
    bool RoomUsed;

    public Room(int width, int height, int x, int y)
    {

    }

    public Room(int width, int height)
    {
        Dimensions = new Rect(0, 0, width, height);
    }


    public void InsertRoom(ref Room newRoom )
    {
        
    }



    //Room()

}
