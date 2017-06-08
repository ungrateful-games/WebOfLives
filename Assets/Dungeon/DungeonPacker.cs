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

public class SpatialRoomHash
{
    List<Room>[] Cells;
    int CellWidth;
    int CellHeight;
    int Rows;
    int Cols;

    public SpatialRoomHash( int BoundaryWidth, int BoundaryHeight, int GridWidth, int GridHeight )
    {
        this.CellWidth  = GridWidth;
        this.CellHeight = GridHeight;
        this.Cols = Mathf.CeilToInt(BoundaryWidth / GridWidth);
        this.Rows = Mathf.CeilToInt(BoundaryHeight / GridHeight);

        // Populate the cells with lists.
        this.Cells = new List<Room>[this.Cols * this.Rows];
        for (int i = 0; i < this.Cells.Length; ++i)
        {
            this.Cells[i] = new List<Room>();
        }
    }

    private int hash( float x, float y)
    {
        return (int)(y/this.CellHeight)  * this.Cols + (int)(x / this.CellWidth);
    }

    private bool collides(ref Room room, int index)
    {
        bool collision = false;
        List<Room> CellRooms = this.Cells[index];

        for (int i = 0; i < CellRooms.Count && !collision; ++i)
        {
            collision |= CellRooms[i].Dimensions.Overlaps(room.Dimensions);
        }

        return collision;
    }

    public bool insert(Room value)
    {
        bool allowed = true;
        int corner_a = this.hash(value.Dimensions.xMin, value.Dimensions.yMin);
        int corner_b = this.hash(value.Dimensions.xMax, value.Dimensions.yMax);

        // The rectangle fits in one cell.
        if (corner_a == corner_b)
        {
            allowed = !this.collides(ref value, corner_a);

            if (allowed)
            {
                this.Cells[corner_a].Add(value);
            }
        }
        else
        {
            allowed |= (!this.collides(ref value, corner_a) || !this.collides(ref value, corner_b) );

            int corner_c = this.hash(value.Dimensions.xMax, value.Dimensions.yMin);

            // If the top edge is in the same spatial index, just insert the room into the two slots (if allowed.)
            if (allowed && corner_a == corner_c)
            {
                this.Cells[corner_a].Add(value);
                this.Cells[corner_b].Add(value);
            }
            else if(allowed)
            {
                int corner_d = this.hash(value.Dimensions.xMin, value.Dimensions.yMax);
                allowed |= (!this.collides(ref value, corner_c) || !this.collides(ref value, corner_d));

                if(allowed)
                {
                    this.Cells[corner_a].Add(value);
                    this.Cells[corner_b].Add(value);
                    this.Cells[corner_c].Add(value);
                    this.Cells[corner_d].Add(value);
                }
            }
        }
        return allowed;
    }
}

[System.Serializable]
public class DungeonPacker
{

    //private BinRoom RootRoom;

    [SerializeField]
    public List<Room> Rooms;

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


        //this.RootRoom = new BinRoom(this.RoomOffsetX, this.RoomOffsetY, this.Width - (2 * this.RoomOffsetX), this.Height - (2 *this.RoomOffsetY), false);

        int room_id = 0;
        int areaTotal = this.Width * this.Height;

        // XXX Maybe a Balanced Tree might be better?
        // XXX Heap, do a heapsort and insert into the spatial hash and a standalone array? 
        // Create a new list of rooms with the total desired room count.
        this.Rooms = new List<Room>(this.NumRooms);
        for (room_id = 0; room_id < this.NumRooms; ++room_id)
        {
            // TODO Generate the positions here too.
            // TODO the widths need to be modified.
            // Generate a new room with a randomized dimension.
            this.Rooms.Insert(room_id, new Room(
                    room_id,
                    placementGen.Next(this.RoomWidthMin, this.RoomWidthMax),
                    placementGen.Next(this.RoomHeightMin, this.RoomHeightMax)));
        }

        // Sort based on the "Room Weight", or area, smallest first.
        this.Rooms.Sort(delegate (Room a, Room b)
        {
            return b.CompareTo(a);
        });

        // Make a new spatial hash with the room dimensions the maximum value of the room size
        // TODO Tune?
        SpatialRoomHash RoomMap = new SpatialRoomHash(this.Width, this.Height, this.RoomWidthMax, this.RoomHeightMax);

        // Add the rooms to the spatial hash, largest area first, and try to pack it as tightly as possible.
        for (int i = room_id-1; i >= 0; --i)
        {
            Rect RoomDimension = this.Rooms[i].Dimensions;
            int failures = 0;
            bool success = false;
            do
            {
                // Generate the room coordinate, add 1 to the base offsets so we can force odd numbered room placements.
                // TODO ensure widths are regular.
                // Might as well do some collision resolution.
                int x = placementGen.Next( this.RoomOffsetX + 1, (int)(this.Width  - RoomDimension.width  - this.RoomOffsetX ) );
                if (x % 2 == 0) x--;

                int y = placementGen.Next( this.RoomOffsetY + 1, (int)(this.Height - RoomDimension.height - this.RoomOffsetY ) );
                if (y % 2 == 0) y--;

                this.Rooms[i].MoveRoom(x, y);
                Debug.Log("i:" + i + " " + this.Rooms[i].Dimensions.ToString());

                success =  true;//RoomMap.insert(this.Rooms[i]);
            } while (!success && failures++ < this.MaxFailureCount );


            // If the room failed, remove it.
            if(!success)
            {
                Debug.Log("Removing room");
                this.Rooms.RemoveAt(i);
            }
            
        }

        /*
        Debug.Log("Available Area: " + areaTotal + " AreaConsumed: " + areaConsumed + " Area Unpacked: " + (areaTotal - areaConsumed) + " Rooms: " + roomCount + " Failures: " + failureCount);

        // Populate an array of rooms for speedier/simpler access.
        int createdRooms = roomCount - failureCount;
        if (createdRooms > 0)
        {
            this.Rooms = new BinRoom[createdRooms];
            this.RootRoom.PopulateArray(ref this.Rooms, 0);
        }
        */
    }


	
}
