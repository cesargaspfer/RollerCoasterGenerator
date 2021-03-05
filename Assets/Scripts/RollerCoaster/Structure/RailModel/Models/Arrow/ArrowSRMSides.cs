using UnityEngine;
using static RailModelProperties;

public class ArrowSRMSides : SubRailModel
{
    private int _modelResolution = 6;

    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return 1;
    }

    public override RailSurface GetRailSurface(RailType type)
    {
        if (!_rs.Equals(default(RailSurface)))
            return _rs;

        // Declaring the variables
        _modelResolution = BasicRMProperties.modelResolution;
        Vector3[] points = new Vector3[_modelResolution + 1];
        Vector3[] normals = new Vector3[_modelResolution + 1];

        // Generating points and normals
        points[0] = Vector3.zero;
        normals[0] = Vector3.right;

        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            points[i + 1] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle)) * 0.2f;
            normals[i + 1] = Vector3.right;
        }

        _rs = new RailSurface(points, normals);

        // Return the calculated points and normals
        return _rs;
    }
    public override Mesh GenerateMesh(RailType type, ExtrudedRailSurface esr)
    {
        Vector3[][] vertices = esr.Points;
        Vector3[][] normals = esr.Normals;

        // Declaring the variables
        Mesh mesh = new Mesh();
        Vector3[] newVertices = new Vector3[vertices[0].Length];
        Vector3[] newNormals = new Vector3[vertices[0].Length];
        int[] triangles = new int[3 * (vertices[0].Length - 1)];
        Vector2[] UVs = new Vector2[vertices[0].Length];

        // Transforming vertices and normals to array
        for (int i = 0; i < vertices[0].Length; i++)
        {
            newVertices[i] = vertices[0][i];
            newNormals[i] = -normals[0][i];
        }

        // Triangulating the vertices
        for (int j = 0; j < vertices[0].Length - 1; j++)
        {
            int index = 3 * j;
            triangles[index] = 0;
            triangles[index + 1] = j + 1;
            triangles[index + 2] = ((j + 1) % _modelResolution) + 1;
        }

        // Calculating UVs
        UVs[0] = new Vector2(0.5f, 0.5f);
        Vector2 offset = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            UVs[i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.5f + offset;
        }

        // Setting the mesh
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        // Return the calculated mesh
        return mesh;
    }

    public override void UpdateMesh(ExtrudedRailSurface esr, Mesh mesh)
    {
        Vector3[] newVertices = new Vector3[esr.Points[0].Length];
        Vector3[] newNormals = new Vector3[esr.Points[0].Length];
        for (int i = 0; i < 1; i++)
        {
            newVertices[i] = esr.Points[0][i];
            newNormals[i] = -esr.Normals[0][i];
        }
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
    }
}
