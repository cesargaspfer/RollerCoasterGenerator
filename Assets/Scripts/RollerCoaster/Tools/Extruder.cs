using UnityEngine;
using static Algebra;

public class Extruder
{
    public static (Vector3[][], Vector3[][]) Extrude(Vector3[] points, Vector3[] normals, SpaceProps sp, RailProps rp, int resolution)
    {
        // Transforming points to initial basis
        Matrix4x4 initialTransfMatrix = OPBTMatrix(Matrix4x4.identity, sp.Basis);

        Vector3[] newVertices = TransformPoints(initialTransfMatrix, points);
        Vector3[] newNormals = TransformPoints(initialTransfMatrix, normals);
        
        // Extruding
        Vector3[][] extrudedVertices = new Vector3[resolution + 1][];
        Vector3[][] extrudedNormals = new Vector3[resolution + 1][];

        float inclination = GetInclinationToMatrixLookAt(sp.Basis, ThreeRotationMatrix(sp.Basis, rp.Radians) * sp.Basis);
        float distance = rp.Length / (float) (resolution);
        float t = 0f;

        for(int i = 0; i < resolution + 1; i++)
        {
            
            // Calculetes the rotation matrix
            // Matrix4x4 rotationMatrix = ThreeRotationMatrix(sp.Basis, t * rp.Radians);
            Matrix4x4 rotationMatrix = MatrixLookAt(sp.Basis, sp.Curve.GetTangentAt(t), t * inclination);

            // Transforms
            extrudedVertices[i] = TransformPoints(rotationMatrix, newVertices);
            extrudedNormals[i] = TransformPoints(rotationMatrix, newNormals);

            // Offsets the points according to the curve
            Vector3 offset = sp.Curve.Sample(t) - sp.Position;
            for(int j = 0; j < extrudedVertices[i].Length; j++)
            {
                extrudedVertices[i][j] += offset;
            }

            t = sp.Curve.GetNextT(t, distance);
            // t = (float) i / (float) resolution;
            if(i == resolution - 1)
                t = 1f;
        }

        // Returns the extruded points and normals
        return (extrudedVertices, extrudedNormals);
    }
}
