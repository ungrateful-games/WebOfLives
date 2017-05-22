using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]    // Fo user input.
[RequireComponent(typeof(Renderer))]
public class TileMap : MonoBehaviour {

    public int TileCountX = 100;
    public int TileCountY = 100;
    public float TileSize = 1.0f;
    

   	// Use this for initialization
	void Start () {
        BuildMesh();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetMeshTexture(Texture2D texture)
    {
        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;
    }

    public void BuildMesh()
    {
        // Compute the numbers
        int num_tiles = TileCountX * TileCountY;
        int num_triangles = num_tiles * 2;

        int vx = TileCountX + 1;
        int vy = TileCountY + 1;
        int num_verts = vx * vy;

        // Build the arrays.
        Vector3[] verts   = new Vector3[num_verts];
        Vector3[] normals = new Vector3[num_verts];
        Vector2[] uvs     = new Vector2[num_verts];

        int[] triangles = new int[num_triangles * 3]; // 3 points per triangle

        for (int y = 0; y < vy; y++) {
            for(int x = 0; x < vx; x++) {
                int index = y * vx + x;

                verts[index] = new Vector3( x * TileSize, -y * TileSize, 0);
                normals[index] = Vector3.up;
                uvs[index] = new Vector2((float) x / TileCountX, 1 - (float) y / TileCountY);                 
            }
        }

        // Build the triangles.
        for (int y = 0; y < TileCountY; y++)  {
            for (int x = 0; x < TileCountX; x++) {
                int index = y * TileCountX + x;
                int tri_index = index * 6;
                int vert_index = y * vx + x;

                // 1---2
                // | \ |
                // 3---4
                // Clockwise winding?
                triangles[tri_index     ] = vert_index         ; // 1
                triangles[tri_index + 1 ] = vert_index + vx + 1; // 3
                triangles[tri_index + 2 ] = vert_index + vx    ; // 4

                triangles[tri_index + 3 ] = vert_index         ; // 1
                triangles[tri_index + 4 ] = vert_index + 1     ; // 2
                triangles[tri_index + 5 ] = vert_index  + 1 + vx; // 4
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        MeshFilter mesh_filter = GetComponent<MeshFilter>();
        MeshCollider mesh_collider = GetComponent<MeshCollider>();

        mesh_filter.mesh = mesh;
        mesh_collider.sharedMesh = mesh;
    } 
}
