using System.Collections.Generic;
using UnityEngine;
using static Algebra;

public class CylinderSupport: Supports
{
    private int _modelResolution = 6;
    private float _radius = 0.15f;

    public override Material GetMaterial()
    {
        return Resources.Load("Materials/RollerCoaster/Support", typeof(Material)) as Material;
    }

   public override (Mesh[], Material[]) GenerateMeshes(Rail rail, float t)
   {
        Matrix4x4 basis = rail.GetBasisAt(t);
        Vector3 initialPosition = rail.sp.Curve.Sample(t);

        List<Vector3[]> vertices = new List<Vector3[]>();
        List<Vector3[]> normals = new List<Vector3[]>();

        Matrix4x4 rotationMatrix = RotationMatrix(Mathf.PI * 0.5f, basis.GetColumn(2));
        Matrix4x4 initialBasis = rotationMatrix * basis;

        Vector3 finalPosition = initialPosition - (Vector3)basis.GetColumn(1);
        Matrix4x4 finalBasis = initialBasis;

        float upAngle = Angle(-basis.GetColumn(1), Vector3.down);
        Vector3 cross;

        if (upAngle > Mathf.PI / 2)
        {
            
            Matrix4x4 rotatedBasis = initialBasis;

            Vector3 downProjected = Vector3.ProjectOnPlane(Vector3.down, basis.GetColumn(0));
            upAngle = Angle(-basis.GetColumn(1), downProjected);
            cross = basis.GetColumn(0);

            rotationMatrix = RotationMatrix(upAngle * 0.25f, cross);
            if(Angle(rotationMatrix.MultiplyPoint3x4(-basis.GetColumn(1)), downProjected) > upAngle)
                upAngle = -upAngle;

            rotationMatrix = RotationMatrix(upAngle * 0.25f, cross);
            finalBasis = rotationMatrix * initialBasis;
            vertices.Add(null);
            normals.Add(null);
            (vertices[0], normals[0]) = GenerateCylinder(initialPosition, initialBasis, finalPosition, finalBasis);

            initialBasis = finalBasis;
            initialPosition = finalPosition;

            rotationMatrix = RotationMatrix(upAngle * 0.5f, cross);
            finalPosition = initialPosition - 1.5f * (Vector3) (rotationMatrix * basis.GetColumn(1));

            finalBasis = rotationMatrix * rotatedBasis;
            float tmpAngle = Angle(finalBasis.GetColumn(0), Vector3.down);
            rotationMatrix = RotationMatrix(tmpAngle * 0.5f, Vector3.Cross(Vector3.down, finalBasis.GetColumn(0)));
            finalBasis = rotationMatrix * finalBasis;
            
            vertices.Add(null);
            normals.Add(null);
            (vertices[1], normals[1]) = GenerateCylinder(initialPosition, initialBasis, finalPosition, finalBasis);

            initialBasis = finalBasis;
            tmpAngle = Angle(finalBasis.GetColumn(0), Vector3.up);
            rotationMatrix = RotationMatrix(tmpAngle, Vector3.Cross(finalBasis.GetColumn(0), Vector3.up));
            finalBasis = rotationMatrix * finalBasis;
            initialPosition = finalPosition;

            finalPosition = new Vector3(initialPosition.x, Terrain.inst.GetHeight(new Vector2(initialPosition.x, initialPosition.z)) - 1f, initialPosition.z);

            vertices.Add(null);
            normals.Add(null);
            (vertices[2], normals[2]) = GenerateCylinder(initialPosition, initialBasis, finalPosition, finalBasis);
        }
        else{
            cross = Vector3.Cross(-basis.GetColumn(1), Vector3.down);
            finalBasis = RotationMatrix(upAngle * 0.5f, cross) * initialBasis;

            vertices.Add(null);
            normals.Add(null);
            (vertices[0], normals[0]) = GenerateCylinder(initialPosition, initialBasis, finalPosition, finalBasis);

            Matrix4x4 tmpBasis = RotationMatrix(upAngle, cross) * initialBasis;
            initialBasis = finalBasis;
            finalBasis = tmpBasis;
            initialPosition = finalPosition;
            
            finalPosition = new Vector3(initialPosition.x, Terrain.inst.GetHeight(new Vector2(initialPosition.x, initialPosition.z)) - 1f, initialPosition.z);
            vertices.Add(null);
            normals.Add(null);
            (vertices[1], normals[1]) = GenerateCylinder(initialPosition, initialBasis, finalPosition, finalBasis);
        }


        int verticesCount =_modelResolution * 2 * vertices.Count;
        Vector3[] finalVertices = new Vector3[verticesCount];
        Vector3[] finalNormals = new Vector3[verticesCount];
        Vector2[] UVs = new Vector2[verticesCount];


        int[] triangles = new int[finalVertices.Length * 6];

        for (int i = 0; i < finalVertices.Length / (_modelResolution * 2); i++)
        {
            int baseIndex = i * 2 * _modelResolution;
            for (int j = 0; j < _modelResolution - 1; j++)
            {
                int index = 6 * (i * 2 * _modelResolution + j);
                triangles[index] = baseIndex + j;
                triangles[index + 1] = baseIndex + j + 1;
                triangles[index + 2] = baseIndex + j + _modelResolution;

                triangles[index + 3] = baseIndex + j + 1;
                triangles[index + 4] = baseIndex + j + _modelResolution + 1;
                triangles[index + 5] = baseIndex + j + _modelResolution;
            }
            for (int j = 0; j < 2 * _modelResolution; j++)
            {
                finalVertices[baseIndex + j] = vertices[i][j];
                finalNormals[baseIndex + j] = normals[i][j];
                UVs[baseIndex + j] = new Vector2((float)j / ((float)_modelResolution - 1f), (float)i / (float)finalVertices.Length);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = finalVertices;
        mesh.normals = finalNormals;
        mesh.uv = UVs;
        mesh.triangles = triangles;

        return (new Mesh[1] { mesh }, new Material[1]{ GetMaterial() });
   }

   private (Vector3[], Vector3[]) GenerateCylinder(Vector3 initialPositon, Matrix4x4 initialBasis, Vector3 finalPosition, Matrix4x4 finalBasis)
   {
        Vector3[] vertices = new Vector3[_modelResolution];
        Vector3[] normals = new Vector3[_modelResolution];

        // Generating vertices and normals
        for (int i = 0; i < _modelResolution; i++)
        {
            float angle = (i / ((float)_modelResolution - 1f)) * 2f * Mathf.PI;
            vertices[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle)) * _radius;
            normals[i] = new Vector3(0f, Mathf.Sin(angle), Mathf.Cos(angle));
        }

        Vector3[] firstCircleVertices = TransformPoints(initialBasis, vertices);
        Vector3[] firstCircleNormals = TransformPoints(initialBasis, normals);
        Vector3[] finalCircleVertices = TransformPoints(finalBasis, vertices);
        Vector3[] finalCircleNormals = TransformPoints(finalBasis, normals);

        for(int i = 0; i < _modelResolution; i++)
        {
            firstCircleVertices[i] += initialPositon;
            finalCircleVertices[i] += finalPosition;
        }

        Vector3[] finalVertices = new Vector3[2 * _modelResolution];
        Vector3[] finalNormals = new Vector3[2 * _modelResolution];
        for (int i = 0; i < _modelResolution; i++)
        {
            finalVertices[i] = firstCircleVertices[i];
            finalVertices[i + _modelResolution] = finalCircleVertices[i];
            finalNormals[i] = firstCircleNormals[i];
            finalNormals[i + _modelResolution] = finalCircleNormals[i];
        }

        return (finalVertices, finalNormals);
   }
}
