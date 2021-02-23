using UnityEngine;
using static RailModelProperties;

public class ThreeCylindersSRMCube : SubRailModel
{
    private int _modelResolution = 5;
    private int _lastType = -1;

    public override Material GetMaterial(RailType type)
    {
        return Resources.Load("Materials/RollerCoaster/Black", typeof(Material)) as Material;
    }

    protected virtual (float, Vector3) GetParams(RailType type)
    {
        float radius = 0f;
        Vector3 offset = Vector3.zero;

        if (type == RailType.Platform)
        {
            radius = ThreeCylindersRMProperties.plataformRadius;
            offset = ThreeCylindersRMProperties.plataformOffset;
        }
        else if (type == RailType.Brake || type == RailType.Impulsor)
        {
            radius = ThreeCylindersRMProperties.brakesRadius;
            offset = ThreeCylindersRMProperties.brakesOffset;
        }
        else
        {
            radius = ThreeCylindersRMProperties.leverRadius;
            offset = ThreeCylindersRMProperties.leverOffset;
        }

        return (radius, offset);
    }

    protected virtual bool IsDesignedRailType(RailType type)
    {
        return type != RailType.Normal;
    }

    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return (int)lenght * 2;
    }
    public override RailSurface GetRailSurface(RailType type)
    {
        if (!this.IsDesignedRailType(type))
            return new RailSurface(new Vector3[0], new Vector3[0]);

        if (!_rs.Equals(default(RailSurface)) && _lastType == (int) type)
            return _rs;

        Vector3[] points = new Vector3[_modelResolution * 2];
        Vector3[] normals = new Vector3[_modelResolution * 2];

        (float radius, Vector3 offset) = this.GetParams(type);
        float sin45 = Mathf.Sqrt(2) * 0.5f;

        // Generating points and normals
        points[0] = new Vector3(0f,  1f,  1f) * radius;
        points[1] = new Vector3(0f,  1f, -1f) * radius;
        points[2] = new Vector3(0f, -1f, -1f) * radius;
        points[3] = new Vector3(0f, -1f,  1f) * radius;
        points[4] = new Vector3(0f,  1f,  1f) * radius;

        for (int i = 0; i < _modelResolution; i++)
        {
            normals[i] = points[i] * sin45;
            
            points[i + _modelResolution] = points[i] + new Vector3(0f, offset.y, -offset.z);
            normals[i + _modelResolution] = normals[i];

            points[i] += offset;
        }

        _rs = new RailSurface(points, normals);
        _lastType = (int) type;

        return _rs;
    }

    public override Mesh GenerateMesh(RailType type, ExtrudedRailSurface esr)
    {
        if (!this.IsDesignedRailType(type))
            return new Mesh();

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
            for (int j = 0; j < (vertices[i].Length - 2) / 2; j++)
            {
                for (int k = 0; k < 2; k++)
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
            for (int j = 0; j < vertices[i].Length / 2; j++)
            {
                for (int k = 0; k < 2; k++)
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
