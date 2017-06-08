using UnityEngine;
using System.Collections;
using System;

public class Room : System.IComparable<Room>
{
    Rect _dimensions;
    int _roomWeight;

    int _roomID;

    public Rect Dimensions
    {
        get { return _dimensions; }
        set { _dimensions = value; }
    }

    public int RoomID
    {
        get { return _roomID; }
        set { _roomID = value; }
    }


    public Room(int width, int height, int x, int y)
    {
        this._dimensions = new Rect(x, y, width, height);
        this._roomWeight = width * height;
    }

    public Room(int room_id, int width, int height)
    {
        this._roomID = room_id;
        this._dimensions = new Rect(0, 0, width, height);
        this._roomWeight = width * height;
    }

    /** @brief Moves the room position.
     * 
     */
    public void MoveRoom(int x, int y)
    {
        this._dimensions.x = x;
        this._dimensions.y = y;
    }

    public int CompareTo(Room other)
    {
        return this._roomWeight - other._roomWeight;
    }
}
