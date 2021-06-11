using UnityEngine;
using static RailModelProperties;

public class SkeletonSRMTransversals : SubRailModel
{
    private float _zOffset = 0.05f;
    private int _id = 0;

    public SkeletonSRMTransversals(int id)
    {
        _id = id;
    }

    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return (int)lenght;
    }

    public override RailSurface GetRailSurface(RailType type)
    {
        if (!_rs.Equals(default(RailSurface)))
            return _rs;

        _zOffset = SkeletonRMProperties.zOffset;

        Vector3[] points = new Vector3[8];
        Vector3[] normals = new Vector3[8];

        // Generating points and normals
        if(_id == 0)
        {
            points[3] = new Vector3(_zOffset, 0.32f, -0.3f);
            points[2] = new Vector3(_zOffset, 0.28f, -0.3f);
            points[1] = new Vector3(_zOffset, 0.28f, 0.3f);
            points[0] = new Vector3(_zOffset, 0.32f, 0.3f);
        }
        else if (_id == 1)
        {
            points[3] = new Vector3(_zOffset, 0.32f, -0.29f);
            points[2] = new Vector3(_zOffset, 0.29f, -0.32f);
            points[1] = new Vector3(_zOffset, -0.01f, -0.01f);
            points[0] = new Vector3(_zOffset, 0.02f, 0.02f);
        }
        else
        {
            points[3] = new Vector3(_zOffset, 0.29f, 0.32f);
            points[2] = new Vector3(_zOffset, 0.32f, 0.29f);
            points[1] = new Vector3(_zOffset, 0.02f, -0.01f);
            points[0] = new Vector3(_zOffset, -0.01f, 0.02f);
        }

        for (int i = 0; i < 4; i++)
        {
            normals[i] = Vector3.right;
            normals[4 + i] = -Vector3.right;
            points[4 + i] = points[i] - Vector3.right * _zOffset;
        }

        _rs = new RailSurface(points, normals);

        return _rs;
    }

    public override Mesh GenerateMesh(RailType type, ExtrudedRailSurface esr)
    {
        Vector3[][] vertices = esr.Points;
        Vector3[][] normals = esr.Normals;

        // Declaring the variables
        Mesh mesh = new Mesh();
        Vector3[] newVertices;
        Vector3[] newNormals;
        int[] triangles = new int[9 * vertices.Length * vertices[0].Length];
        Vector2[] UVs = new Vector2[vertices.Length * vertices[0].Length];

        // Transforming vertices and normals to array
        (newVertices, newNormals) = ConvertPointsToArray(esr);

        // Triangulating the vertices
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            int index = 3 * vertices[0].Length * i;
            int index2 = vertices[0].Length * i;
            triangles[index] = index2;
            triangles[index + 1] = index2 + 1;
            triangles[index + 2] = index2 + 2;

            triangles[index + 3] = index2 + 2;
            triangles[index + 4] = index2 + 3;
            triangles[index + 5] = index2 + 0;

            index += 6;
            index2 += 4;
            triangles[index] = index2;
            triangles[index + 1] = index2 + 2;
            triangles[index + 2] = index2 + 1;

            triangles[index + 3] = index2 + 2;
            triangles[index + 4] = index2 + 0;
            triangles[index + 5] = index2 + 3;
        }

        int l = vertices[0].Length;
        int res = l / 2;
        int offset = 3 * vertices.Length * l;

        for (int i = 0; i < vertices.Length - 1; i++)
        {
            for (int j = 0; j < l / 2; j++)
            {
                int index = offset + 6 * (i * l + j);
                triangles[index] = i * l + (j);
                triangles[index + 1] = i * l + (j + res);
                triangles[index + 2] = i * l + (j + 1) % res;

                triangles[index + 3] = i * l + (j + 1) % res;
                triangles[index + 4] = i * l + (j + res);
                triangles[index + 5] = i * l + ((j + 1) % res + res);
            }
        }

        // Calculating UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                int index = vertices[0].Length * i + j * 4;
                UVs[index] = new Vector2(0.5f, 0f);
                UVs[index + 1] = new Vector2(0f, 1f);
                UVs[index + 2] = new Vector2(0.5f, 0.6666666f);
                UVs[index + 3] = new Vector2(1f, 0f);
            }
        }

        // Setting the mesh
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Return the calculated mesh
        return mesh;
    }
}