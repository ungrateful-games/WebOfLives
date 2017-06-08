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

    public const int BEDROCK = 0;

    // Room data  
    public const int NOPASS = 0x1;
    public const int ROOM = 0x2;
    public const int CORRIDOR = 0x4;
    public const int WALL = 0x8;


    // Wall Direction.
    public const int TOP       = 0x10;
    public const int RIGHT     = 0x20;
    public const int BOTTOM    = 0x40;
    public const int LEFT      = 0x80;
    public const int DIRECTION = TOP | RIGHT | BOTTOM | LEFT;

    // Wall Metadata. 
    public const int DOOR  = 0x100;
    public const int LIGHT = 0x200;
    public const int PLACE_HOLDER3 = 0x400;
    public const int SECRET = 0x800;
     
    // ???
    public const int PLACE_HOLDER5 = 0x1000;
    public const int PLACE_HOLDER6 = 0x2000;
    public const int PLACE_HOLDER7 = 0x4000;
    public const int PLACE_HOLDER8 = 0x8000;
                                    
    // Group Metadata                    
    public const int PLACE_HOLDER9 = 0x10000;
    public const int PLACE_HOLDER10 = 0x20000;
    public const int PLACE_HOLDER11 = 0x40000;
    public const int PLACE_HOLDER12 = 0x80000;
                                    
    public const int PLACE_HOLDER13 = 0x100000;
    public const int PLACE_HOLDER14 = 0x200000;
    public const int PLACE_HOLDER15 = 0x400000;
    public const int PLACE_HOLDER16 = 0x800000;

    public const int GROUP_SHIFT = 16;
    public const int GROUP_MASK = 0xFF << GROUP_SHIFT;
    public const int GROUP_CLEAR = ~GROUP_MASK;

    // Byte 4: Meta data           
    public const int IN_MAZE = 0x1000000;   // Helper flag for generating the maze.
    public const int PLACE_HOLDER18 = 0x2000000;
    public const int PLACE_HOLDER19 = 0x4000000;
    public const int PLACE_HOLDER20 = 0x8000000;
                                    
    public const int PLACE_HOLDER21 = 0x10000000;
    public const int PLACE_HOLDER22 = 0x20000000;
    public const int PLACE_HOLDER23 = 0x40000000;
    public const int PLACE_HOLDER24 = 1 << 32;

    // HELPER MASKs
    public const int CELL_TYPE = NOPASS | ROOM | CORRIDOR | WALL;
    public const int MAZE_TERM = IN_MAZE | CORRIDOR | ROOM | NOPASS;
    public const int CLEAR_WALL = ~(DIRECTION | WALL);



    [SerializeField]
    public DungeonPacker Dungeon;

    [Tooltip("The tile Map Object that will render the map.")]
    public GameObject TileMapPrefab;
    // TODO 
    /*
    [Tooltip("Width of the map in tiles.")]
    public int MapWidth = 99; //: The width of the map to be generated in tiles.

    [Tooltip("Heght of the map in tiles.")]
    public int MapHeight = 99; //: The height of the map to be generated in tiles.
    */
    public const int MaxMapSegmentWidth = 149;   //: The maximum width of a map segment in tiles.
    public const int MaxMapSegmentHeight = 149;  //: The maximum height of a map segment in tiles.

    /*
    public int MaxNumRooms = 30; // The number of rooms to generate.
    public int MinRoomWidth  = 3;
    public int MinRoomHeight = 3;
    public int RoomVarianceX = 9;
    public int RoomVarianceY = 9;
    */

    //public CellTypes[] Cells; // The contents of the map cells, independent of number of segments.
    public int[] MaskedCells;
    //public int sparseness = 2;

    public TileMap[] MapSegments;   // An Array of map segments, may not be needed ? XXX
    private int SegCountX = 0;
    private int SegCountY = 0;

    public int TileResolution = 32;

    public Texture2D Tileset;
    public Vector2[] TileMappings;

    public void InitMap()
    {
        this.TileMappings = new Vector2[Tileset.width * Tileset.height];
        for (int y = this.TileResolution, index = 0; y <= Tileset.height; y += this.TileResolution)
        {
            for (int x = 0; x < Tileset.width; x += this.TileResolution, index++)
            {
                this.TileMappings[index] = new Vector2(x, Tileset.height - y);
                // Debug.Log((CellTypes)index + " "  + this.TileMappings[index].x + " " + this.TileMappings[index].y);
            }
        }

        this.Dungeon.GenerateDungeon();

        this.MaskedCells = new int[this.Dungeon.Height * this.Dungeon.Width];

        for (int i = this.Dungeon.Rooms.Count - 1; i >= 0; --i)
        {
            Room Room = this.Dungeon.Rooms[i];

            for (int y = (int)(Room.Dimensions.height + Room.Dimensions.y -1); y >= (int)Room.Dimensions.y ; --y)
            {
                for (int x = (int)(Room.Dimensions.width + Room.Dimensions.x - 1), index = y * this.Dungeon.Width + x; 
                    x >= Room.Dimensions.x && index >0 && index < this.MaskedCells.Length; 
                    --x, --index)
                {
                    this.MaskedCells[index] = ROOM;
                }
            }
        }

        #region GenMapSegments
        // Get the number of segments on each axis and initalize the array.
        this.SegCountX = (int)Mathf.Ceil((float)this.Dungeon.Width  / MaxMapSegmentWidth);
        this.SegCountY = (int)Mathf.Ceil((float)this.Dungeon.Height / MaxMapSegmentHeight);
        this.MapSegments = new TileMap[this.SegCountX * this.SegCountY];

        // Sizing helper variables.
        int WidthRemaining  = this.Dungeon.Width ;
        int HeightRemaining = this.Dungeon.Height;
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
                this.PaintTiles(x, y);
            }
            // Perform after Columns down
            WidthRemaining = this.Dungeon.Width;
            HeightRemaining -= MaxMapSegmentHeight;
        }
        #endregion
    }

    public void PaintTiles(int segX, int segY)
    {
        // Get the tile meap
        TileMap Map = this.MapSegments[segY * this.SegCountX + segX];

        int xStart = segX * MapGenerator.MaxMapSegmentWidth;
        int x, y = segY * MapGenerator.MaxMapSegmentHeight;

        int xMax = xStart + Map.TileCountX;
        int yMax = y + Map.TileCountY;

        Texture2D texture = new Texture2D(Map.TileCountX * this.TileResolution, Map.TileCountY * this.TileResolution);

        // Iterate and generate the tile map.
        for (; y < yMax; y++)
        {
            for (x = xStart; x < xMax; x++)
            {
                //Debug.Log("x: " + x + " y: " + y + " Cell Type: " + ((int)this.MaskedCells[y * this.Dungeon.Width + x] & (CELL_TYPE)));
                Vector2 mapping = TileMappings[(int)this.MaskedCells[y * this.Dungeon.Width + x] & (CELL_TYPE )];
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
