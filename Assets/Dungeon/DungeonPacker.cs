using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        this.Cols = Mathf.CeilToInt((1.0f * BoundaryWidth) / GridWidth);
        this.Rows = Mathf.CeilToInt((1.0f * BoundaryHeight) / GridHeight);

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
            collision |= CellRooms[i].Dimensions.Overlaps( room.Dimensions);
            //Debug.Log("new room: " + room.Dimensions + "old Room:" + CellRooms[i].Dimensions  + " collides: " +collision);

        }
        //Debug.Log(collision);

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
            allowed &= (!this.collides(ref value, corner_a) && !this.collides(ref value, corner_b) );
            //Debug.Log( "After double check "  + allowed);
        

            int corner_c = this.hash(value.Dimensions.xMax, value.Dimensions.yMin);

            // If the top edge is in the same spatial index, just insert the room into the two slots (if allowed.)
            if (allowed && corner_a == corner_c)
            {
                //Debug.Log("hash size: " + this.Cells.Length + " R: " + value.Dimensions.xMax + " L: " + value.Dimensions.xMin + " T:" + value.Dimensions.yMax + " B:" + value.Dimensions.yMin);
                //Debug.Log("a:" + corner_a + " b:" + corner_b);
                this.Cells[corner_a].Add(value);
                this.Cells[corner_b].Add(value);
            }
            else if(allowed)
            {
                int corner_d = this.hash(value.Dimensions.xMin, value.Dimensions.yMax);
                //Debug.Log("hash size: " + this.Cells.Length + " R: " + value.Dimensions.Right + " L: " + value.Dimensions.Left + " T:" + value.Dimensions.Top + " B:" + value.Dimensions.Bottom);
                //Debug.Log("a:" + corner_a + " b:" + corner_b + " c: " + corner_c + " d: " + corner_d);
                allowed &= (!this.collides(ref value, corner_c) && !this.collides(ref value, corner_d));

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
        System.Random testGen;
        // Init the seed of the dungeon.
        if (this.PlaceSeed != -1)
        {
            placementGen = new System.Random(this.PlaceSeed);
            testGen = new System.Random(this.PlaceSeed);
        }
        else
        { 
            placementGen = new System.Random();
            testGen = new System.Random();
        }

    int room_id = 0;


        // XXX Maybe a Balanced Tree might be better?
        // XXX Heap, do a heapsort and insert into the spatial hash and a standalone array? 
        // Create a new list of rooms with the total desired room count.
        this.Rooms = new List<Room>(this.NumRooms);
        for (room_id = 0; room_id < this.NumRooms; ++room_id)
        {
            int width = placementGen.Next(this.RoomWidthMin, this.RoomWidthMax);
            if (width % 2 == 0) width--;

            int height = placementGen.Next(this.RoomHeightMin, this.RoomHeightMax);
            if (height % 2 == 0) height--;

            // TODO Generate the positions here too.
            // TODO the widths need to be modified.
            // Generate a new room with a randomized dimension.
            this.Rooms.Insert(room_id, new Room( room_id, width, height ));
        }

        // Sort based on the "Room Weight", or area, smallest first.


        //this.Rooms.Sort();
        /*
        delegate (Room a, Room b)
        {
            return b.CompareTo(a);
        });*/

        // Set the window that the rooms can be created in.
        int minX = this.RoomWidthMax / 2 + this.RoomOffsetX + 1; ;
        int maxX = this.Width - (this.RoomWidthMax / 2) - this.RoomOffsetX -1;

        int minY = this.RoomHeightMax / 2 + this.RoomOffsetY + 1;
        int maxY = this.Height - (this.RoomHeightMax / 2) - this.RoomOffsetY -1;
        
        // Make a new spatial hash with the room dimensions the maximum value of the room size
        // TODO Tune?
        SpatialRoomHash RoomMap = new SpatialRoomHash(this.Width, this.Height, this.RoomWidthMax + 1, this.RoomHeightMax + 1);

        // Add the rooms to the spatial hash, largest area first, and try to pack it as tightly as possible.
        //for (int i = room_id-1; i >= 0; --i)
        for (int i = 0;  i < room_id; ++i)
        {
           // Rect RoomDimension = this.Rooms[i].Dimensions;

            int failures = 0;
            bool success = false;
            int cMaxX = maxX - (int) (this.RoomWidthMax - this.Rooms[i].Dimensions.width) ;
            int cMaxY = maxY - (int)(this.RoomHeightMax - this.Rooms[i].Dimensions.height);
            //Debug.Log(cMaxX + "  " + cMaxY);
            do
            {
                // Generate the room coordinate, add 1 to the base offsets so we can force odd numbered room placements.
                // TODO ensure widths are regular.
                // Might as well do some collision resolution.
                // TODO should this be 
                int x = testGen.Next(minX, cMaxX);
                if (x % 2 == 1) x--;

                int y = testGen.Next(minY, cMaxY);
                if (y % 2 == 1) y--;

                this.Rooms[i].MoveRoom(x, y);
                //Debug.Log("i:" + i + " " + this.Rooms[i].Dimensions.ToString());

                success =  RoomMap.insert(this.Rooms[i]);
            } while (!success && failures++ < this.MaxFailureCount );


            // If the room failed, remove it.
            if(!success)
            {
                //Debug.Log("Removing room");
                this.Rooms.RemoveAt(i--);
                room_id--;
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
