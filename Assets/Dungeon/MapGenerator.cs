using UnityEngine;
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
    
    public const byte NOPASS    = 1;
    public const byte ROOM      = 2;
    public const byte CORRIDOR  = 4;
    public const byte WALL      = 8;

    public const byte CELL_TYPE = NOPASS | ROOM | CORRIDOR | WALL;


    public const byte TOP       = 16;
    public const byte RIGHT     = 32;
    public const byte BOTTOM    = 64;
    public const byte LEFT      = 128;
    public const byte DIRECTION = TOP | RIGHT | BOTTOM | LEFT;

    [Tooltip("The tile Map Object that will render the map.")]
    public GameObject TileMapPrefab;
    // TODO 
    [Tooltip("Width of the map in tiles.")]
    public int MapWidth = 150; //: The width of the map to be generated in tiles.

    [Tooltip("Heght of the map in tiles.")]
    public int MapHeight = 150; //: The height of the map to be generated in tiles.

    public int MaxMapSegmentWidth = 100;   //: The maximum width of a map segment in tiles.
    public int MaxMapSegmentHeight = 100;  //: The maximum height of a map segment in tiles.

    public int MaxNumRooms = 30; // The number of rooms to generate.
    public int MinRoomWidth  = 3;
    public int MinRoomHeight = 3;
    public int RoomVarianceX = 9;
    public int RoomVarianceY = 9;

    public CellTypes[] Cells; // The contents of the map cells, independent of number of segments.
    public byte[] MaskedCells;
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

        this.MaskedCells = new byte[MapHeight * MapWidth]; // Build the initial "design" for the map.

        #region SealEdge
        // Prevent the edges of the map from hosting cells the user can enter.
        // TODO explode to two separate loops? Might be the same number of checks.
        for (int y = 0, index = 0; y < MapHeight; y++)
        {
            index = y * MapWidth;
            if (y > 0 && y < MapHeight - 1)
            {
                this.MaskedCells[index] = NOPASS; // "Left wall"
                this.MaskedCells[index + MapWidth - 1] = NOPASS; // "Right Wall"
            }
            else
            {
                for (int x = 0; x < MapWidth; x++, index++)
                {
                    this.MaskedCells[index] = NOPASS;
                }
            }
        }
        #endregion

        int numRooms = MaxNumRooms;
        while (numRooms-- > 0)
        {
            PlaceRoom();
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
   
    public void PlaceRoom()
    {
        // Find out where the rooms belong first.
        #region GenRoom
        // Compute an even numbered starting location.
        // Room Positions are even so we can keep a border on the map.
        // Compute an odd numbered room width (they always need to be odd for consistency.
        // Room Widths are odd because they're inclusive widths.
        // TODO play with weight functions.

        // Horizontal
        int roomX = (int)(Random.value * (MapWidth - 4 - MinRoomWidth)) + 2;
        if (roomX % 2 == 1) roomX += 1;

        int roomWidth = (int)(Random.value * (RoomVarianceX)) + MinRoomWidth;
        if (roomWidth % 2 == 0) roomWidth += 1;
        int roomXMax  = Mathf.Min(roomWidth  + roomX, MapWidth - 2);
        roomWidth = roomXMax - roomX;

        //  Vertical
        int roomY = (int)(Random.value * (MapHeight - 4 - MinRoomHeight)) + 2;
        if (roomY % 2 == 1) roomY += 1;

        int roomHeight = (int)(Random.value * (RoomVarianceY)) + MinRoomHeight;
        if (roomHeight % 2 == 0) roomHeight += 1;
        int roomYMax  = Mathf.Min(roomHeight + roomY, MapWidth - 2);
        roomHeight = roomYMax - roomY;
        #endregion

        #region RoomPlacement
        bool validRoom = true;
        int x = roomX, y = roomY, index = 0;
        // Place the rooms.
        for (; y < roomYMax && validRoom; ++y)
        {
            for (x = roomX,  index = y * MapWidth + x; x < roomXMax && validRoom; ++x, ++index)
            {
                validRoom = validRoom && (this.MaskedCells[index] == BEDROCK);
                this.MaskedCells[index ] |= ROOM;
            }
        }
        #endregion

        #region RoomValidation
        // FIXME THIS IS BROKEN?
        // If the room was valid place the room walls.
        // Else the room was invalid, revert the touched rooms
        if (validRoom)
        { 
            index = 0;
            byte roomType = 0;

            // FIXME Can we make this more elegant?
            for (y = roomY ; y <= roomYMax; y++) {
                index = y * MapWidth + roomX - 1;

                if (y > roomY && y < roomYMax) {
                    this.MaskedCells[index] = ROOM | WALL | LEFT; // "Left wall"
                    this.MaskedCells[index + roomWidth + 1] = ROOM | WALL | RIGHT; // "Right Wall"
                }
                else {
                    // Set the room type
                    if (y == roomY) roomType = ROOM | WALL | BOTTOM;
                    else roomType = ROOM | WALL | TOP;

                    for (x = roomX; x < roomXMax + 2; x++, index++) 
                        this.MaskedCells[index] = roomType;                    
                }
            }
            
            // TODO build the room obj
        }
        else
        { 
            for (y--, x -= 2; y >= roomY; y--, x = roomXMax - 1)
                for (; x >= roomX; x--)
                    this.MaskedCells[y * MapWidth + x] = BEDROCK;
        }

        #endregion
    }

    void ScanCorridors()
    {
        List<int> Walls = new List<int>(this.MaskedCells.Length / 2);

        int yMax = this.MapHeight - 1;
        int xMax = this.MapWidth - 1;
        
        for (int y = 1; y < yMax; ++y)
        {
            for(int x = 1, index = y * this.MapWidth + 1 ; x < xMax; ++x, ++index )
            {
                if (this.MaskedCells[index] == BEDROCK)
                    PlaceCorridors(index,ref Walls);
                return;
            }
        }

        /*
        for(int pass = 0; pass < this.sparseness; ++pass)
        {
            SparsifyCorridors();
        }
        */

    }

    // Masks horizontally and adds the walls to the list if needed.
    void MaskHorizontal(int index, ref List<int> Walls)
    {
        if ((this.MaskedCells[index - 1] & CELL_TYPE) == BEDROCK)
            Walls.Add(index - 1);
        this.MaskedCells[index - 1] |= LEFT | WALL;

        if ((this.MaskedCells[index + 1] & CELL_TYPE) == BEDROCK)
            Walls.Add(index + 1);
        this.MaskedCells[index + 1] |= RIGHT | WALL;
    }

    // MasksVertically and adds the walls to the list if needed.
    void MaskVertical(int index, ref List<int> Walls)
    {
        if ((this.MaskedCells[index + this.MapWidth] & CELL_TYPE ) == BEDROCK)
            Walls.Add(index + this.MapWidth);
        this.MaskedCells[index + this.MapWidth] = TOP | WALL;

        // FIXME THIS FAILS.
        if ((this.MaskedCells[index - this.MapWidth] & CELL_TYPE) == BEDROCK)
            Walls.Add(index - this.MapWidth);
        this.MaskedCells[index - this.MapWidth] = BOTTOM | WALL;
    }


    void PlaceCorridors(int corridor, ref List<int> Walls)
    {
        // Since this is a spanning tree I think this might work?
        // FIXME do the math John.
        // TODO optimize this data structure.
        this.MaskedCells[corridor] = CORRIDOR;

        #region BuildStartWalls
        // For corridors and rooms we don't care if they're directional, but we can use that in the future.
        MaskHorizontal(corridor,ref Walls);
        MaskVertical(corridor, ref Walls);
        #endregion

        int endCell;
        byte wallValue, dir;
  
        while (Walls.Count > 0)
        {
            //Debug.Log(Walls.Count);
            int targetWall = (int)(Random.value * (Walls.Count - 1));
            int cellIndex = Walls[targetWall];

            wallValue = this.MaskedCells[cellIndex];
            dir = (byte)(wallValue & DIRECTION);

            switch (dir)
            {
                case LEFT:
                    endCell = cellIndex - 1;                    
                    break;
                case RIGHT:
                    endCell = cellIndex + 1;
                    break;
                case TOP:
                    endCell = cellIndex + this.MapWidth;
                    break;
                case BOTTOM:
                    endCell = cellIndex - this.MapWidth;
                    break;
                default:
                    endCell = -1;
                    break;
            }

            if (endCell > 0 )//&& (this.MaskedCells[endCell] & (ROOM | NOPASS | )) == BEDROCK )
            {
                this.MaskedCells[cellIndex] &= CORRIDOR;
                this.MaskedCells[endCell]   = (byte)(WALL | dir);

                if((dir & (LEFT | RIGHT)) != 0 )
                {
                    MaskHorizontal(cellIndex, ref Walls);
                    MaskVertical(cellIndex, ref Walls);
                }
                else
                {
                    MaskHorizontal(cellIndex, ref Walls);
                    MaskVertical(cellIndex, ref Walls);
                }

                MaskHorizontal(endCell, ref Walls);
                MaskVertical(endCell, ref Walls); // FIXME THIS RUINS EVERYTHING
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

    public void PaintTiles(int segX, int segY)
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

                Vector2 mapping = TileMappings[(int)this.MaskedCells[y * MapWidth + x] & (CORRIDOR )];
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


    // Use this for initialization
    void Start () {
        InitMap();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
