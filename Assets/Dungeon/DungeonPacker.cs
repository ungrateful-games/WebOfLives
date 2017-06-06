using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BinRoom
{
    [SerializeField]
    int X;
    [SerializeField]
    int Y;
    [SerializeField]
    int W;
    [SerializeField]
    int H;
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

            targetNode.Right = new BinRoom(targetNode.X + w + oX, targetNode.Y         , targetNode.W - (w + oX), targetNode.H           , false);
            targetNode.Down  = new BinRoom(targetNode.X         , targetNode.Y + h + oY, targetNode.W           , targetNode.H - (h + oY), false);

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

    public override string ToString()
    {
        return "x: " + X + " y: " + Y + " W: " + W + " H:" + H;
    }
}

[System.Serializable]
public class DungeonPacker {

    [SerializeField]
    public BinRoom RootRoom;

    [SerializeField]
    public int DungeonWidth = 50;

    [SerializeField]
    public int DungeonHeight = 50;

    [SerializeField]
    public int DungeonSeed = -1;

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
        if (this.DungeonSeed != -1)
            placementGen = new System.Random(this.DungeonSeed);
        else
            placementGen = new System.Random();
        //
        //    Random.InitState(this.DungeonSeed);

        this.RootRoom = new BinRoom(this.RoomOffsetX, this.RoomOffsetY, this.DungeonWidth - this.RoomOffsetX, this.DungeonHeight - this.RoomOffsetY, false);

        int areaConsumed = 0;
        int areaTotal = this.DungeonWidth * this.DungeonHeight;
        int roomCount = 0;
        int failureCount = 0;
        // Populate the dungeon with a bin packing algorithm.
        for (roomCount = 0, failureCount = 0; roomCount < this.NumRooms && failureCount < this.MaxFailureCount; ++roomCount)
        {
            int roomWidth = placementGen.Next( this.RoomWidthMin, this.RoomWidthMax) ;
            int roomHeight = placementGen.Next(this.RoomHeightMin, this.RoomHeightMax);
            int roomArea   = roomWidth * roomHeight;

            // Sanity checker.
            if ( areaConsumed + roomArea <= areaTotal && this.RootRoom.PlaceRoom(roomWidth, roomHeight, this.RoomOffsetX, this.RoomOffsetY))
            {
                areaConsumed += roomArea;
            }
            else
            {
                failureCount++;
            }
        }

        Debug.Log("Available Area: "  + areaTotal + " AreaConsumed: " + areaConsumed +  " Area Unpacked: "  + (areaTotal - areaConsumed) + " Rooms: " + roomCount + " Failures: " +  failureCount);
    }


	
}
