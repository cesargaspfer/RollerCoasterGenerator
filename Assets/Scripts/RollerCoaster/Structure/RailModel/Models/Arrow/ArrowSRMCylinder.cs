﻿using UnityEngine;
using static RailModelProperties;

public class ArrowSRMCylinder : SubRailModel
{
    private int _modelResolution = 6;
    private float _radius = 1f;

    public override int GetExtrusionResolution(float lenght, ModelProps mp)
    {
        return (int)lenght;
    }

    public override RailSurface GetRailSurface(RailType type)
    {
        if (!_rs.Equals(default(RailSurface)))
            return _rs;

        _modelResolution = BasicRMProperties.modelResolution;
        _radius = BasicRMProperties.radius;
        Vector3[] points = new Vector3[_modelResolution];
        Vector3[] normals = new Vector3[_modelResolution];

        // Generating points and normals
        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            points[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));
            normals[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));
        }

        _rs = new RailSurface(points, normals);

        return _rs;
    }
    public override Mesh GenerateMesh(RailType type, ExtrudedRailSurface esr)
    {
        Vector3[][] vertices = esr.Points;
        Vector3[][] normals = esr.Normals;

        int tmpIndex = vertices.Length - 1;
        for (int j = 0; j < vertices[0].Length; j++)
            vertices[tmpIndex][j] = vertices[tmpIndex - 1][j] - normals[tmpIndex - 1][j];

        tmpIndex--;
        for (int j = 0; j < vertices[0].Length; j++)
            vertices[tmpIndex][j] = vertices[tmpIndex - 1][j] - normals[tmpIndex - 1][j] * 0.5f;

        for(int i = 0; i < vertices.Length - 2; i++)
            for (int j = 0; j < vertices[0].Length; j++)
                vertices[i][j] -= normals[i][j] * 0.8f;

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
            for (int j = 0; j < vertices[i].Length - 1; j++)
            {
                int index = 6 * (i * vertices[i].Length + j);
                triangles[index] = CalculateIndex(i, j, vertices[i].Length);
                triangles[index + 1] = CalculateIndex(i + 1, j, vertices[i].Length);
                triangles[index + 2] = CalculateIndex(i, j + 1, vertices[i].Length);

                triangles[index + 3] = CalculateIndex(i, j + 1, vertices[i].Length);
                triangles[index + 4] = CalculateIndex(i + 1, j, vertices[i].Length);
                triangles[index + 5] = CalculateIndex(i + 1, j + 1, vertices[i].Length);
            }
        }

        // Calculating UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = 0; j < vertices[i].Length; j++)
            {
                UVs[i * vertices[i].Length + j] = new Vector2((float)j / ((float)vertices[i].Length - 1f), (float)i / (float)vertices.Length);
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
