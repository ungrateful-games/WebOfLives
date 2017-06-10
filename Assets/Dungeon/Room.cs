using UnityEngine;
using System.Collections;
using System;

public class RectInt
{
    int _x;
    int _y;
    int _x_rad;
    int _y_rad;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y
    {
        get { return _y; }
        set { _y = value; }
    }

    public int Width
    {
        get { return 2 * _x_rad; }
        set { _x_rad = value / 2; }
    }

    public int Height
    {
        get { return _y_rad * 2; }
        set { _y_rad = value / 2; }
    }

    public RectInt( int x, int y, int width, int height)
    {
        this._x = x;
        this._y = y;
        this._x_rad = width / 2;
        this._y_rad = height / 2;
    }
    public int Right { get { return this._x + this._x_rad; } }
    public int Left { get { return this._x - this._x_rad; } }

    public int Top { get { return this._y + this._y_rad; } }
    public int Bottom { get { return this._y - this._y_rad; } }

    /** @brief 
     * @return true if the boxes are colliding.
     */
    public bool Overlaps( RectInt rect)
    {
        int rx = this._x_rad + rect._x_rad;
        int ry = this._y_rad + rect._y_rad;

        // Abuse underflow.
        return !(
            (uint)(this._x - rect._x + rx) > (rx + rx) ||
            (uint)(this._y - rect._y + ry) > (ry + ry));
    }
}


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
