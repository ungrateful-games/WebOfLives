using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BinRoom
{
    [SerializeField]
    public int X;
    [SerializeField]
    public int Y;
    [SerializeField]
    public int W;
    [SerializeField]
    public int H;
    [SerializeField]
    bool Used;
    [SerializeField]
    BinRoom Right;
    [SerializeField]
    BinRoom Down;

    public BinRoom(int x, int y, int w, int h, bool inUse)
    {
        this.Used = inUse;
        this.X = x;
        this.Y = y;
        this.W = w;
        this.H = h;

        this.Right = null;
        this.Down  = null;
    }

    /** @brief Attempt to place a room starting in this room.
     *  
     *  Algorithm will perform a depth first search of the room space and attempt to place a room rectangle in that space.
     *
     */ 
    public bool PlaceRoom(int w, int h, int oX, int oY)
    {
       
        BinRoom targetNode = this.FindSpace(w,h);
        bool NodeFound = targetNode != null;

        if (NodeFound)
        {
            targetNode.Right = new BinRoom(targetNode.X + w + oX, targetNode.Y         , targetNode.W - w , targetNode.H - oY, false);
            targetNode.Down  = new BinRoom(targetNode.X         , targetNode.Y + h + oY, targetNode.W - oX, targetNode.H - h , false);

            targetNode.W = w;
            targetNode.H = h;
            targetNode.Used = true;
        }

        return NodeFound;
    }

    public BinRoom FindSpace(int w, int h)
    {
        BinRoom retRoom = null;

        // If this room is in use, traverse the tree.
        if ( this.Used )
        {
            // First check Down
            if (this.Down != null)
            {
                retRoom = this.Down.FindSpace(w, h);
            }

            // Then check Right if no space is located and the Right Exists.
            if (retRoom == null && this.Right != null)
            {
                retRoom = this.Right.FindSpace(w, h);
            }
        }
        else if ( this.W > w && this.H > h )
        {
            retRoom = this;
        }
        return retRoom;
    }

    public int PopulateArray( ref BinRoom[] binRooms, int index)
    {
        // If the down is not null we can use it for populating the array 
        if (this.Down != null)
        {
            // If the room was used we can populate it.
            if (this.Down.Used)
                index = this.Down.PopulateArray(ref binRooms, index);
            else
                this.Down = null;
        }

        // Cache the room.
        binRooms[index++] = this;

        // If the down is not null we can use it for populating the array 
        if (this.Right != null)
        {
            // If the room was used we can populate it.
            if (this.Right.Used)
                index = this.Right.PopulateArray(ref binRooms, index);
            else
                this.Right = null;
        }

        return index;
    }

    public override string ToString()
    {
        return "x: " + X + " y: " + Y + " W: " + W + " H:" + H;
    }
}

[System.Serializable]
public class DungeonPacker
{
    private BinRoom RootRoom;

    [SerializeField]
    public BinRoom[] Rooms;

    [SerializeField]
    public int Width = 50;

    [SerializeField]
    public int Height = 50;

    [SerializeField]
    public int PlaceSeed = -1;

    [SerializeField]
    public int NumRooms = 0;

    [SerializeField]
    public int MaxFailureCount = 5;

    [SerializeField]
    public int RoomWidthMin;

    [SerializeField]
    public int RoomWidthMax;

    [SerializeField]
    public int RoomHeightMin;

    [SerializeField]
    public int RoomHeightMax;

    [SerializeField]
    public int RoomOffsetX;

    [SerializeField]
    public int RoomOffsetY;


    public DungeonPacker( int width, int height, int numRooms, float density, int dungeonSeed )
    {
        //RootRoom.PlaceRoom();
    }
    
    public void GenerateDungeon()
    {
        System.Random placementGen;

        // Init the seed of the dungeon.
        if (this.PlaceSeed != -1)
            placementGen = new System.Random(this.PlaceSeed);
        else
            placementGen = new System.Random();

        this.RootRoom = new BinRoom(this.RoomOffsetX, this.RoomOffsetY, this.Width - (2 * this.RoomOffsetX), this.Height - (2 *this.RoomOffsetY), false);

        int areaConsumed = 0, roomCount = 0, failureCount = 0;
        int areaTotal = this.Width * this.Height;

        // Populate the dungeon with a bin packing algorithm.
        for (roomCount = 0, failureCount = 0; roomCount < this.NumRooms && failureCount < this.MaxFailureCount; ++roomCount)
        {
            int roomWidth = placementGen.Next( this.RoomWidthMin, this.RoomWidthMax) ;
            int roomHeight = placementGen.Next(this.RoomHeightMin, this.RoomHeightMax);
            int roomArea   = roomWidth * roomHeight;

            // Sanity checker.
            if ( areaConsumed + roomArea <= areaTotal && 
                this.RootRoom.PlaceRoom(roomWidth, roomHeight, this.RoomOffsetX, this.RoomOffsetY))
            {
                areaConsumed += roomArea;
            }
            else
            {
                failureCount++;
            }
        }

        Debug.Log("Available Area: " + areaTotal + " AreaConsumed: " + areaConsumed + " Area Unpacked: " + (areaTotal - areaConsumed) + " Rooms: " + roomCount + " Failures: " + failureCount);

        // Populate an array of rooms for speedier/simpler access.
        int createdRooms = roomCount - failureCount;
        if (createdRooms > 0)
        {
            this.Rooms = new BinRoom[createdRooms];
            this.RootRoom.PopulateArray(ref this.Rooms, 0);
        }
    }


	
}
