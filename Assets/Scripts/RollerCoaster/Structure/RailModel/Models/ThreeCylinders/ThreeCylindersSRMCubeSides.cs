using UnityEngine;
using static RailModelProperties;


public class ThreeCylindersSRMCubeSides : SubRailModel
{
    private int _modelResolution = 4;
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
        return 2;
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
        points[0] = new Vector3(0f, 1f, 1f) * radius;
        points[1] = new Vector3(0f, 1f, -1f) * radius;
        points[2] = new Vector3(0f, -1f, -1f) * radius;
        points[3] = new Vector3(0f, -1f, 1f) * radius;

        for (int i = 0; i < _modelResolution; i++)
        {
            normals[i] = Vector3.right;

            points[i + _modelResolution] = points[i] + new Vector3(0f, offset.y, -offset.z);
            normals[i + _modelResolution] = Vector3.right;

            points[i] += offset;
        }

        _rs = new RailSurface(points, normals);
        _lastType = (int) type;

        // Return the calculated points and normals
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
        Vector3[] newVertices = new Vector3[2 * vertices[0].Length];
        Vector3[] newNormals = new Vector3[2 * vertices[0].Length];
        int[] triangles = new int[24 * vertices[0].Length];
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
        int l = vertices[0].Length / 2;
        for (int k = 0; k < 2; k++)
        {
            int index = 6 * l * k;
            triangles[index] = l * k;
            triangles[index + 1] = 1 + l * k;
            triangles[index + 2] = 2 + l * k;

            triangles[index + 3] = l * k;
            triangles[index + 4] = 2 + l * k;
            triangles[index + 5] = 3 + l * k;
        }

        for (int k = 2; k < 4; k++)
        {
            int index = 6 * l * k;
            triangles[index] = l * k;
            triangles[index + 1] = 2 + l * k;
            triangles[index + 2] = 1 + l * k;

            triangles[index + 3] = l * k;
            triangles[index + 4] = 3 + l * k;
            triangles[index + 5] = 2 + l * k;
        }

        // Calculating UVs
        for (int i = 0; i < 4; i++)
        {
            UVs[4 * i + 0] = new Vector2(1f, 1f);
            UVs[4 * i + 1] = new Vector2(1f, 0f);
            UVs[4 * i + 2] = new Vector2(0f, 0f);
            UVs[4 * i + 3] = new Vector2(0f, 1f);
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
