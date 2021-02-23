using UnityEngine;
using static RailModelProperties;

public class ThreeCylindersSRMSides : SubRailModel
{
    private int _modelResolution = 6;
    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return 2;
    }
    public override RailSurface GetRailSurface(RailType type)
    {
        if (!_rs.Equals(default(RailSurface)))
            return _rs;

        // Declaring the variables
        _modelResolution = ThreeCylindersRMProperties.modelResolution;
        Vector3[] points = new Vector3[(_modelResolution + 1) * 3];
        Vector3[] normals = new Vector3[(_modelResolution + 1) * 3];

        float mainCylinderRadius = ThreeCylindersRMProperties.mainCylinderRadius;
        float sideCylinderRadius = ThreeCylindersRMProperties.sideCylinderRadius;
        float cylinderOffset = ThreeCylindersRMProperties.cylinderOffset;

        // Generating points and normals
        points[0] = Vector3.zero;
        normals[0] = Vector3.right;

        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            points[i + 1] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));
            normals[i + 1] = Vector3.right;
        }

        for (int i = 0; i < _modelResolution + 1; i++)
        {
            points[i + (_modelResolution + 1)] = points[i] * sideCylinderRadius + new Vector3(0f, cylinderOffset, cylinderOffset);
            normals[i + (_modelResolution + 1)] = normals[i];

            points[i + (_modelResolution + 1) * 2] = points[i] * sideCylinderRadius + new Vector3(0f, cylinderOffset, -cylinderOffset);
            normals[i + (_modelResolution + 1) * 2] = normals[i];

            points[i] *= mainCylinderRadius;
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
        Vector3[] newVertices = new Vector3[2 * vertices[0].Length];
        Vector3[] newNormals = new Vector3[2 * vertices[0].Length];
        int[] triangles = new int[6 * vertices[0].Length];
        Vector2[] UVs = new Vector2[2 * vertices[0].Length];

        // Transforming vertices and normals to array
        for (int i = 0; i < vertices[0].Length; i++)
        {
            newVertices[i] = vertices[0][i];
            newNormals[i] = -normals[0][i];

            newVertices[vertices[0].Length + i] = vertices[vertices.Length - 1][i];
            newNormals[vertices[0].Length + i] = normals[vertices.Length - 1][i];
        }

        // Triangulating the vertices
        int l = vertices[0].Length / 3;
        for (int j = 0; j < _modelResolution; j++)
        {
            for(int k = 0; k < 3; k++)
            {
                int index = 3 * (j + l * k);
                triangles[index] = l * k;
                triangles[index + 1] = j + 1 + l * k;
                triangles[index + 2] = ((j + 1) % _modelResolution) + 1 + l * k;
            }
        }
        int indexOffset = 3 * vertices[0].Length;
        for (int j = 0; j < _modelResolution; j++)
        {
            for (int k = 0; k < 3; k++)
            {
                int index = indexOffset + 3 * (j + l * k);
                triangles[index] = vertices[0].Length + l * k;
                triangles[index + 1] = vertices[0].Length + ((j + 1) % _modelResolution) + 1 + l * k;
                triangles[index + 2] = vertices[0].Length + j + 1 + l * k;
            }
        }

        // Calculating UVs
        UVs[0] = new Vector2(0.5f, 0.5f);
        UVs[l] = UVs[0];
        UVs[2 * l] = UVs[0];
        UVs[vertices[0].Length] = UVs[0];
        UVs[vertices[0].Length + l] = UVs[0];
        UVs[vertices[0].Length + 2 * l] = UVs[0];
        Vector2 offset = UVs[0];
        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            UVs[i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.5f + offset;
            UVs[vertices[0].Length + i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.5f + offset;

            UVs[i + 1 + l] = UVs[i + 1];
            UVs[vertices[0].Length + i + 1 + l] = UVs[vertices[0].Length + i + 1];

            UVs[i + 1 + 2 * l] = UVs[i + 1];
            UVs[vertices[0].Length + i + 1 + 2 * l] = UVs[vertices[0].Length + i + 1];
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
        Vector3[] newVertices = new Vector3[2 * esr.Points[0].Length];
        Vector3[] newNormals = new Vector3[2 * esr.Points[0].Length];
        for (int i = 0; i < esr.Points[0].Length; i++)
        {
            newVertices[i] = esr.Points[0][i];
            newNormals[i] = -esr.Normals[0][i];

            newVertices[esr.Points[0].Length + i] = esr.Points[esr.Points.Length - 1][i];
            newNormals[esr.Points[0].Length + i] = esr.Normals[esr.Points.Length - 1][i];
        }
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
    }
}
