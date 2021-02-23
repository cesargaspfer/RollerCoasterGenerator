using UnityEngine;
using static RailModelProperties;

public abstract class SubRailModel
{
    private protected RailSurface _rs;
    public abstract int GetExtrusionResolution(float lenght, ModelProps mp);
    public abstract RailSurface GetRailSurface(RailType type);
    public abstract Mesh GenerateMesh(RailType type, ExtrudedRailSurface esr);

    public virtual Material GetMaterial(RailType type)
    {
        return Resources.Load("Materials/RollerCoaster/Metal", typeof(Material)) as Material;
    }

    public virtual void UpdateMesh(ExtrudedRailSurface esr, Mesh mesh)
    {
        (Vector3[] newVertices, Vector3[] newNormals) = ConvertPointsToArray(esr);
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
    }

    protected virtual (Vector3[], Vector3[]) ConvertPointsToArray(ExtrudedRailSurface esr)
    {
        Vector3[][] vertices = esr.Points;
        Vector3[][] normals = esr.Normals;

        // Declaring the variables
        Vector3[] newVertices = new Vector3[vertices.Length * vertices[0].Length];
        Vector3[] newNormals = new Vector3[vertices.Length * vertices[0].Length];

        // Transforming vertices and normals to array
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < vertices[i].Length; j++)
            {
                int index = i * vertices[i].Length + j;
                newVertices[index] = vertices[i][j];
                newNormals[index] = normals[i][j];
            }
        }

        return (newVertices, newNormals);
    }
}