﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum CellTypes
{
    Bedrock   = 0,
    Room      = 1,
    Corridor  = 2,
    UpWall    = 3,
    DownWall  = 4,
    RightWall = 5,
    LeftWall  = 6,
    VertWall  = 7,
    HorizWall = 8
}


public class MapGenerator : MonoBehaviour {
    
    public const byte BEDROCK   = 0;
    public const byte ROOM      = 1;
    public const byte CORRIDOR  = 2;
    public const byte TOP       = 4;
    public const byte RIGHT     = 8;
    public const byte BOTTOM    = 16;
    public const byte LEFT      = 32;
    public const byte WALL      = 64;
    public const byte NOPASS    = 128;

    [SerializeField]
    public GameObject TileMapPrefab;
    // TODO 
    public int MapWidth = 500; //: The width of the map to be generated in tiles.
    public int MapHeight = 500; //: The height of the map to be generated in tiles.

    public int MaxMapSegmentWidth = 100;   //: The maximum width of a map segment in tiles.
    public int MaxMapSegmentHeight = 100;  //: The maximum height of a map segment in tiles.

    public int NumRooms = 30; // The number of rooms to generate.
    public int MinRoomWidth  = 3;
    public int MinRoomHeight = 3;
    public int RoomVarianceX = 9;
    public int RoomVarianceY = 9;


    public CellTypes[] Cells; // The contents of the map cells, independent of number of segments.

    public int sparseness = 2;

    public TileMap[] MapSegments;   // An Array of map segments, may not be needed ? XXX
    private int SegCountX = 0;
    private int SegCountY = 0;

    public int TileResolution = 32;

    public Texture2D Tileset;
    public Vector2[] TileMappings;

    public void InitMap()
    {
        // TODO move somewhere else.
        this.TileMappings = new Vector2[Tileset.width * Tileset.height];
        for(int y = this.TileResolution, index = 0; y <= Tileset.height; y+=this.TileResolution)
        {
            for(int x= 0; x < Tileset.width; x+=this.TileResolution, index++)
            {
                this.TileMappings[index] = new Vector2(x, Tileset.height -  y);
               // Debug.Log((CellTypes)index + " "  + this.TileMappings[index].x + " " + this.TileMappings[index].y);
            }
        }

        #region GenMapSegments
        // Get the number of segments on each axis and initalize the array.
        this.SegCountX = (int)Mathf.Ceil((float)this.MapWidth / MaxMapSegmentWidth);
        this.SegCountY = (int)Mathf.Ceil((float)this.MapHeight / MaxMapSegmentHeight);
        this.MapSegments = new TileMap[this.SegCountX * this.SegCountY];
        
        // Sizing helper variables.
        int WidthRemaining = MapWidth;
        int HeightRemaining = MapHeight;
        int width, height;

        for (int y = 0; y < this.SegCountY; y++)
        {
            height = Mathf.Min(HeightRemaining, MaxMapSegmentHeight);

            for (int x = 0; x < this.SegCountX; x++)
            {
                width = Mathf.Min(WidthRemaining, MaxMapSegmentWidth);

                // Generate the map and then transpose it to where it belongs in the game world.
                GameObject TileMapObj = Instantiate(TileMapPrefab, new Vector3(x * MaxMapSegmentWidth, -y * MaxMapSegmentHeight, 0), Quaternion.identity, this.transform) as GameObject;

                TileMap TileMap = TileMapObj.GetComponent<TileMap>();
                TileMap.TileCountX = width;
                TileMap.TileCountY = height;
                TileMap.TileSize = 1;
                TileMap.BuildMesh();

                this.MapSegments[y * this.SegCountX + x] = TileMap; 
            }
            // Perform after Columns down
            WidthRemaining = MapWidth;
            HeightRemaining -= MaxMapSegmentHeight;
        }
        #endregion
        
        this.GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        this.Cells = new CellTypes[MapWidth * MapHeight]; // Default set to zero ("Bedrock").

        byte[] maskedCells = new byte[MapHeight * MapWidth]; // Build the initial "design" for the map.

        #region SealEdge
        // Prevent the edges of the map from hosting cells the user can enter.
        for(int y = 0, index = 0; y < MapHeight; y++)
        {
            index = y * MapWidth;
            if (y > 0 || y < MapHeight - 1)
            {
                maskedCells[index] = NOPASS; // "Left wall"
                maskedCells[index + MapWidth - 1] = NOPASS; // "Right Wall"
            }
            else
            {
                for (int x = 0; x < MapWidth; x++, index++)
                {
                    maskedCells[index] = NOPASS; 
                }
            }
        }
        #endregion

        while (this.NumRooms-- > 0)
        {
            PlaceRoom(ref maskedCells);
        }

        // Generate the corridors.
        ScanCorridors();


        for (int y = 0; y < this.SegCountY; y++)
        {
            for (int x = 0; x < this.SegCountX; x++)
            {
                this.PaintTiles( x, y );
            }
        }
    }

    public void PaintTiles( int segX, int segY )
    {
        // Get the tile meap
        TileMap Map = this.MapSegments[segY * this.SegCountX + segX];

        int xStart = segX * this.MaxMapSegmentWidth;
        int x, y = segY * this.MaxMapSegmentHeight;

        int xMax = xStart + Map.TileCountX;
        int yMax = y + Map.TileCountY;

        Texture2D texture = new Texture2D(Map.TileCountX * this.TileResolution, Map.TileCountY * this.TileResolution);

        // Iterate and generate the tile map.
        for (; y < yMax; y++)
        {
 
            for (x = xStart; x < xMax; x++)
            {
                Vector2 mapping = TileMappings[(int)this.Cells[y * MapWidth + x]];
              //  Debug.Log(this.Cells[y * MapWidth + x] + " " + mapping.x + " " + mapping.y);

                texture.SetPixels(x * this.TileResolution, y * this.TileResolution, this.TileResolution, this.TileResolution, 
                        this.Tileset.GetPixels((int)mapping.x, (int)mapping.y, this.TileResolution, this.TileResolution));
                //
            }            
        }

        // Build the texture.
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        Map.SetMeshTexture(texture);
    }

    public void PlaceRoom(ref byte[] maskedCells)
    {
        // Find out where the rooms belong first.
        #region GenRoom
        // Compute an odd numbered starting location.
        int roomX = (int)(Random.value * (MapWidth  - 3 - MinRoomWidth )) + 3;
        int roomY = (int)(Random.value * (MapHeight - 3 - MinRoomHeight)) + 3;

        if ( roomX % 2 == 0 )  roomX += 1;
        if ( roomY % 2 == 0 )  roomY += 1;

        int x = roomX, y = roomY;

        // Compute an odd numbered room width (they always need to be odd for consistency.
        int roomWidth  = (int)(Random.value * (RoomVarianceX)) + MinRoomWidth;
        int roomHeight = (int)(Random.value * (RoomVarianceY)) + MinRoomHeight;

        if (roomWidth  % 2 == 0) roomX += 1;
        if (roomHeight % 2 == 0) roomY += 1;

        int roomXMax  = Mathf.Min(roomWidth  + roomX, MapWidth - 3);
        int roomYMax  = Mathf.Min(roomHeight + roomY, MapWidth - 3);
        #endregion

        #region RoomPlacement
        bool validRoom = true;

        // Place the rooms.
        for (; y < roomYMax && validRoom; y++)
        {
            for (x = roomX; x < roomXMax && validRoom; x++)
            {
                validRoom = validRoom && (maskedCells[y * MapWidth + x] == BEDROCK);
                maskedCells[ y * MapWidth + x ] |= ROOM;
            }
        }

        #endregion

        #region RoomValidation
        // FIXME THIS IS BROKEN?
        // If the room was valid place the room walls.
        // Else the room was invalid, revert the touched rooms
        if (validRoom)
        { 
            int index = 0;
            byte roomType = 0;
            for (y = roomY; y < roomYMax; y++) {

                index = y * MapWidth + roomX;

                if (y > roomY || y < roomYMax) {
                    maskedCells[index] = ROOM | WALL | LEFT; // "Left wall"
                    maskedCells[index + MapWidth - 1] = ROOM | WALL | RIGHT; // "Right Wall"
                }
                else {
                    // Set the room type
                    if (y == roomY) roomType = ROOM | WALL | BOTTOM;
                    else roomType = ROOM | WALL | TOP;

                    for (x = roomX; x < roomXMax; x++, index++) 
                        maskedCells[index] = roomType;                    
                }
            }

            // TODO build the room obj
        }
        else
        { 
            for (y--, x -= 2; y >= roomY; y--, x = roomXMax - 1)
                for (; x >= roomX; x--)
                    maskedCells[y * MapWidth + x] = BEDROCK;
        }

        #endregion
    }

    void ScanCorridors()
    {
        List<int> Walls = new List<int>(this.Cells.Length);

        int yMax = this.MapHeight - 1;
        int xMax = this.MapWidth - 1;
        for(int y = 2, index = this.MapWidth + 1; y < yMax; ++y)
        {
            for(int x = 2; x < xMax; ++x, ++index )
            {
                if (this.Cells[index] == CellTypes.Bedrock)
                    PlaceCorridors(index, Walls);
            }
        }

        /*
        for(int pass = 0; pass < this.sparseness; ++pass)
        {
            SparsifyCorridors();
        }
        */

    }

    void PlaceCorridors(int corridor, List<int> Walls)
    {
        // Since this is a spanning tree I think this might work?
        // FIXME do the math John.
        // TODO optimize this data structure.

        this.Cells[corridor] = CellTypes.Corridor;

        #region BuildStartWalls
        // TODO this is Coppy pasted!
        // LeftWall, verify we aren't on an edge.
        if ((corridor) % this.MapWidth != 0 &&
            this.Cells[corridor - 1] == CellTypes.Bedrock)
        {
            // Debug.Log("LeftWall: " + (endCell - 1));
            Walls.Add(corridor - 1);
            this.Cells[corridor - 1] = CellTypes.LeftWall;
        }

        // RightWall
        if ((corridor + 1) % this.MapWidth != 0 &&
            this.Cells[corridor + 1] == CellTypes.Bedrock)
        {
            //Debug.Log("RightWall: " + (endCell + 1));

            Walls.Add(corridor + 1);
            this.Cells[corridor + 1] = CellTypes.RightWall;
        }

        // DownWall
        if (corridor - this.MapWidth > 0 &&
            this.Cells[corridor - this.MapWidth] == CellTypes.Bedrock)
        {
            //Debug.Log("DownWall: " + (endCell - this.MapWidth));

            Walls.Add(corridor - this.MapWidth);
            this.Cells[corridor - this.MapWidth] = CellTypes.DownWall;
        }

        // UpWall
        if (corridor + this.MapWidth < this.Cells.Length &&
            this.Cells[corridor + this.MapWidth] == CellTypes.Bedrock)
        {
            Walls.Add(corridor + this.MapWidth);
            this.Cells[corridor + this.MapWidth] = CellTypes.UpWall;
        }
        #endregion


        /*
        Walls.Add(0);
        this.Cells[0] = CellTypes.Corridor;
        
        this.Cells[0] = CellTypes.Corridor;
        this.Cells[1] = CellTypes.RightWall;
        this.Cells[this.MapWidth] = CellTypes.UpWall;

        Walls.Add(1);
        Walls.Add(this.MapWidth);
        */
        int endCell;
        CellTypes endVal, wallValue;
  
        while (Walls.Count > 0)
        {
            //Debug.Log(Walls.Count);
            int targetWall = (int)(Random.value * (Walls.Count - 1));

            int cellIndex = Walls[targetWall];
            wallValue = this.Cells[cellIndex];

           // Debug.Log(cellIndex);
            //Debug.Log(wallValue);
            switch (wallValue)
            {
                case CellTypes.UpWall: // UpWall
                    //startCell = cellIndex - this.MapWidth;
                    endCell   = cellIndex + this.MapWidth;
                    if (endCell >= this.Cells.Length) endCell = -1;

                    break;
                case CellTypes.DownWall: // DownWall
                    //startCell = cellIndex + this.MapWidth;
                    endCell   = cellIndex - this.MapWidth;

                    break; 
                case CellTypes.RightWall: // RightWall
                    //startCell = cellIndex - 1;

                    // FIXME there might be an out of bounds thing here.
                    endCell   = cellIndex + 1;

                    break;
                case CellTypes.LeftWall: // LeftWall
                    //startCell = cellIndex + 1;
                    endCell   = cellIndex - 1 ;
                    break;

                default:
                  //  startCell = -1;
                    endCell = -1;
                    break;

            }

            if ( endCell > 0)
            {
                // Out of Range exception was found.
                try
                {
                    endVal = this.Cells[endCell];
                }
                catch 
                {
                    // Edge case for right wall failure detected.
                    // Looks like the logic check doesn't work.
                    Debug.Log("EndCell: "  + endCell); //EndCell: 22500
                    Debug.Log("CellIndex: " + cellIndex); // CellIndex: 22499
                    Debug.Log("Wall: " + wallValue); //Wall: RightWall
                    Walls.RemoveAt(targetWall);
                    continue;
                }
                //startVal = this.Cells[startCell];

                // If the end value is bedrock, we can generate.
                if (endVal == CellTypes.Bedrock)
                {
                    // Populate the walls flanking the corridor we're skipping.
                    if (wallValue == CellTypes.UpWall || wallValue == CellTypes.DownWall)
                    {
                        // FIXME this logic doesn't always work!

                        if ((cellIndex + 1) % this.MapWidth != 0 &&
                            this.Cells[cellIndex + 1] == CellTypes.Bedrock)
                        {
                            Walls.Add(cellIndex + 1);
                            this.Cells[cellIndex + 1] = CellTypes.RightWall;
                        }
                        if (cellIndex % this.MapWidth != 0 &&
                            this.Cells[cellIndex - 1] == CellTypes.Bedrock)
                        {
                            Walls.Add(cellIndex - 1);
                            this.Cells[cellIndex - 1] = CellTypes.LeftWall;
                        }
                    }
                    else
                    {

                        if (cellIndex - this.MapWidth > 0 &&
                            this.Cells[cellIndex - this.MapWidth] == CellTypes.Bedrock)
                        {
                            Walls.Add(cellIndex - this.MapWidth);
                            this.Cells[cellIndex - this.MapWidth] = CellTypes.DownWall;
                        }


                        if ((cellIndex + this.MapWidth) < this.Cells.Length &&
                            this.Cells[cellIndex + this.MapWidth] == CellTypes.Bedrock)
                        {
                            Walls.Add(cellIndex + this.MapWidth);
                            this.Cells[cellIndex + this.MapWidth] = CellTypes.UpWall;
                        }
                    }

                    // Set the wall and the neighbor as in the maze.
                    this.Cells[cellIndex] = CellTypes.Corridor;
                    this.Cells[endCell] = CellTypes.Corridor;


                    // Add the walls, todo edge test?
                    // LeftWall, verify we aren't on an edge.
                    if ((endCell) % this.MapWidth != 0 &&
                        this.Cells[endCell - 1] == CellTypes.Bedrock)
                    {
                       // Debug.Log("LeftWall: " + (endCell - 1));
                        Walls.Add(endCell - 1);
                        this.Cells[endCell - 1] = CellTypes.LeftWall;
                    }

                    // RightWall
                    if ((endCell + 1) % this.MapWidth != 0 &&
                        this.Cells[endCell + 1] == CellTypes.Bedrock)
                    {
                        //Debug.Log("RightWall: " + (endCell + 1));

                        Walls.Add(endCell + 1);
                        this.Cells[endCell + 1] = CellTypes.RightWall;
                    }

                    // DownWall
                    if (endCell - this.MapWidth > 0 &&
                        this.Cells[endCell - this.MapWidth] == CellTypes.Bedrock)
                    {
                        //Debug.Log("DownWall: " + (endCell - this.MapWidth));

                        Walls.Add(endCell - this.MapWidth);
                        this.Cells[endCell - this.MapWidth] = CellTypes.DownWall;
                    }

                    // UpWall
                    if (endCell + this.MapWidth < this.Cells.Length &&
                        this.Cells[endCell + this.MapWidth] == CellTypes.Bedrock)
                    {
                        Walls.Add(endCell + this.MapWidth);
                        this.Cells[endCell + this.MapWidth] = CellTypes.UpWall;
                    }
                }
            }
            Walls.RemoveAt(targetWall);                
        }
    }

    void SparsifyCorridors()
    {
        int cellIndex, upIndex, downIndex, leftIndex, rightIndex, adjCorridors, safeIndex = -1;
        for( int y = 0; y < this.MapHeight; ++y)
        {
            for(int x = 0; x< this.MapWidth; ++x)
            {
                cellIndex = y * this.MapWidth + x;
                if (this.Cells[cellIndex] != CellTypes.Corridor) continue;

                adjCorridors = 0;

                if (y < (this.MapHeight -1))
                {            
                    upIndex = cellIndex + this.MapWidth;
                    
                    if(this.Cells[upIndex] == CellTypes.Room || this.Cells[upIndex] == CellTypes.Corridor )
                    {
                        adjCorridors++;
                        safeIndex = upIndex;
                    }
                }
                else
                    upIndex = -1;

                if (y > 0)
                {
                    downIndex = cellIndex - this.MapWidth;

                    if (this.Cells[downIndex] == CellTypes.Room || this.Cells[downIndex] == CellTypes.Corridor)
                    {
                        adjCorridors++;
                        safeIndex = downIndex;
                    }
                }
                else
                    downIndex = -1;


                if( x > 0 )
                {
                    leftIndex = cellIndex - 1;

                    if (this.Cells[leftIndex] == CellTypes.Room || this.Cells[leftIndex] == CellTypes.Corridor)
                    {
                        adjCorridors++;
                        safeIndex = leftIndex;
                    }
                }
                else
                    leftIndex = -1;

                if (x < (this.MapWidth -1))
                {
                    rightIndex = cellIndex + 1;

                    if (this.Cells[rightIndex] == CellTypes.Room || this.Cells[rightIndex] == CellTypes.Corridor)
                    {
                        adjCorridors++;
                        safeIndex = rightIndex;
                    }
                }
                else
                    rightIndex = -1;

                if( adjCorridors <= 1 )
                {
                    if (upIndex >= 0 && safeIndex != upIndex) this.Cells[upIndex] = CellTypes.Bedrock;
                    if (downIndex >= 0 && safeIndex != downIndex) this.Cells[downIndex] = CellTypes.Bedrock;
                    if (rightIndex >= 0 && safeIndex != rightIndex) this.Cells[rightIndex] = CellTypes.Bedrock;
                    if (leftIndex >= 0 && safeIndex != leftIndex) this.Cells[leftIndex] = CellTypes.Bedrock;
                }
            }
        }

    }
    // Use this for initialization
    void Start () {
        InitMap();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
