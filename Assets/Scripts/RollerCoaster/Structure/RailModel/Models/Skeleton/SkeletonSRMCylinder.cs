using UnityEngine;
using static RailModelProperties;

public class SkeletonSRMCylinder : SubRailModel
{
    private int _modelResolution = 6;
    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return (int)lenght * 2;
    }
    public override RailSurface GetRailSurface(RailType type)
    {
        if (!_rs.Equals(default(RailSurface)))
            return _rs;

        _modelResolution = SkeletonRMProperties.modelResolution;

        Vector3[] points = new Vector3[_modelResolution * 3];
        Vector3[] normals = new Vector3[_modelResolution * 3];

        float mainCylinderRadius = SkeletonRMProperties.mainCylinderRadius;
        float sideCylinderRadius = SkeletonRMProperties.sideCylinderRadius;
        float cylinderOffset = SkeletonRMProperties.cylinderOffset;

        // Generating points and normals
        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            points[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));
            normals[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));

            points[i + _modelResolution] = points[i] * sideCylinderRadius + new Vector3(0f, cylinderOffset, cylinderOffset);
            normals[i + _modelResolution] = normals[i];

            points[i + _modelResolution * 2] = points[i] * sideCylinderRadius + new Vector3(0f, cylinderOffset, -cylinderOffset);
            normals[i + _modelResolution * 2] = normals[i];

            points[i] *= mainCylinderRadius;
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
        int[] triangles = new int[6 * vertices.Length * vertices[0].Length];
        Vector2[] UVs = new Vector2[vertices.Length * vertices[0].Length];

        // Transforming vertices and normals to array
        (newVertices, newNormals) = ConvertPointsToArray(esr);

        // Triangulating the vertices
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            for (int j = 0; j < (vertices[i].Length - 3) / 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int index = 6 * (i * vertices[i].Length + k * _modelResolution + j);
                    int y = k * _modelResolution + j;
                    triangles[index] = CalculateIndex(i, y, vertices[i].Length);
                    triangles[index + 1] = CalculateIndex(i + 1, y, vertices[i].Length);
                    triangles[index + 2] = CalculateIndex(i, y + 1, vertices[i].Length);

                    triangles[index + 3] = CalculateIndex(i, y + 1, vertices[i].Length);
                    triangles[index + 4] = CalculateIndex(i + 1, y, vertices[i].Length);
                    triangles[index + 5] = CalculateIndex(i + 1, y + 1, vertices[i].Length);
                }
            }
        }

        // Calculating UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < vertices[i].Length / 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    UVs[i * vertices[i].Length + k * _modelResolution + j] =
                        new Vector2((float)j / ((float)vertices[i].Length - 1f), (float)i / (float)vertices.Length);
                }
            }
        }

        // Setting the mesh
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        // Return the calculated mesh
        return mesh;
    }

    private int CalculateIndex(int i, int j, int length)
    {
        return i * length + j;
    }
}