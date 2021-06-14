using UnityEngine;
using System;

public class Algebra
{
    public static float Angle(Vector3 from, Vector3 to)
    {
        return Vector3.Angle(from, to) * Mathf.PI / 180f;
    }

    public static Matrix4x4 RotationMatrix(float radians, Vector3 axis)
    {
        float axisMagitude = axis.magnitude;
        if(radians == Mathf.PI && axisMagitude == 0.0)
        {
            Matrix4x4 specialMatrix = new Matrix4x4();
            Vector3[] rows = new Vector3[3]
                {
                    new Vector3(1.0f,  0.0f,  0.0f),
                    new Vector3(0.0f, -1.0f,  0.0f),
                    new Vector3(0.0f,  0.0f, -1.0f)
                };
            for(int i = 0; i < 3; i++)
                specialMatrix.SetRow(i, rows[i]);
            return specialMatrix;
        }
        if(radians == 0.0 || axisMagitude == 0.0)
        {
            return Matrix4x4.identity;
        }

        float degrees = radians * 180 / Mathf.PI;
        Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
        Matrix4x4 matrix = Matrix4x4.Rotate(rotation);
        return matrix;
    }

    public static Matrix4x4 ThreeRotationMatrix(Matrix4x4 basis, RailProps rp)
    {
        Vector3 props = new Vector3(rp.Elevation, rp.Rotation, rp.Inclination);
        return ThreeRotationMatrix(basis, props);
    }

    public static Matrix4x4 ThreeRotationMatrix(Matrix4x4 basis, Vector3 radians)
    {
        Vector3 degrees = radians;
        Vector3 x = basis.GetColumn(0);
        Vector3 y = basis.GetColumn(1);
        Vector3 z = basis.GetColumn(2);

        // Calculate the three rotations, the first two are global and the last is local
        Vector3 projected = new Vector3(x.x, 0f, x.z).normalized;
        if(y.y < 0)
            projected = -projected;
        Vector3 cross = Vector3.Cross(projected, Vector3.up);
        if(cross.magnitude < 0.1f)
            cross = z;
        Matrix4x4 matrix1 = RotationMatrix(degrees[0], cross);
        x = TransformPoints(matrix1, new Vector3[1] { x })[0];

        cross = Vector3.Cross(z, Vector3.right);
        if (cross.magnitude < 0.1f)
            cross = y;
        Matrix4x4 matrix2 = RotationMatrix(degrees[1], Vector3.up);
        x = TransformPoints(matrix2, new Vector3[1] { x })[0];

        Matrix4x4 matrix3 = RotationMatrix(degrees[2], x);

        // The final matrix
        Matrix4x4 matrix = matrix3 * (matrix2 * matrix1);

        return matrix;
    }

    public static (float, float) GetRotationAngles(Matrix4x4 basis, Vector3 va, Vector3 vb)
    {

        Vector3 pva = Vector3.ProjectOnPlane(va, Vector3.up).normalized;
        Vector3 pvb = Vector3.ProjectOnPlane(vb, Vector3.up).normalized;
        if(Mathf.Abs(Vector3.Dot(va, Vector3.up)) >= 0.999999f)
        {
            pva = Vector3.ProjectOnPlane((Vector3) basis.GetColumn(1), Vector3.up).normalized;
        }
        float rotation = Angle(pva, pvb);
        if (rotation >= 3.1415)
            rotation = 0;

        Vector3 tmpCross = Vector3.Cross(va, pvb);
        Matrix4x4 tmpRotationMatrix = ThreeRotationMatrix(basis, new RailProps(0f, rotation, 0f, 5f));
        if ((pvb - tmpRotationMatrix.MultiplyPoint3x4(pva)).magnitude > 0.001f)
        {
            rotation = -rotation;
        }

        Matrix4x4 rotationMatrix = RotationMatrix(-rotation, Vector3.up);
        vb = TransformPoints(rotationMatrix, new Vector3[1] { vb })[0];

        Vector3 projected = new Vector3(basis.GetColumn(0).x, 0f, basis.GetColumn(0).z).normalized;
        if (basis.GetColumn(1).y < 0)
            projected = -projected;
        Vector3 cross = Vector3.Cross(projected, Vector3.up);
        if (cross.magnitude < 0.1f)
            cross = basis.GetColumn(2);

        Vector3 px = Vector3.ProjectOnPlane(basis.GetColumn(0), cross).normalized;
        Vector3 pr = Vector3.ProjectOnPlane(vb, cross).normalized;
        float elevation = Angle(px, pr);
        if (elevation >= 3.1415)
            elevation = 0;
            
        tmpRotationMatrix = ThreeRotationMatrix(basis, new RailProps(elevation, 0f, 0f, 5f));
        if ((pr - tmpRotationMatrix.MultiplyPoint3x4(px)).magnitude > 0.001f)
        {
            elevation = -elevation;
        }


        return (elevation, rotation);
    }

    public static Vector3[] TransformPoints(Matrix4x4 matrix, Vector3[] points)
    {
        Vector3[] newPoints = new Vector3[points.Length];
        for(int i = 0; i < newPoints.Length; i++)
        {
            newPoints[i] = matrix.MultiplyPoint3x4(points[i]);
        }
        return newPoints;
    }

    public static Matrix4x4 OPBTMatrix(Matrix4x4 from, Matrix4x4 to)
    {
        Vector3 fX = from.GetColumn(0);
        Vector3 fY = from.GetColumn(1);
        Vector3 tX = to.GetColumn(0);
        Vector3 tY = to.GetColumn(1);

        // Fisrt Rotation
        float angle = Algebra.Angle(fY, tY);
        float angle1 = angle;
        Vector3 cross = Vector3.Cross(fY, tY).normalized;
        Vector3 cross1 = Vector3.Cross(fY, tY);

        Matrix4x4 matrix1 = RotationMatrix(angle, cross);
        if(angle != 0f)
        {
            fX = matrix1.MultiplyPoint3x4(fX);
            fY = matrix1.MultiplyPoint3x4(fY);
        }
            

        // Second Rotation
        angle = Algebra.Angle(fX, tX);
        cross = Vector3.Cross(fX, tX).normalized;

        Matrix4x4 matrix2;
        if(angle > 3.14 || cross.magnitude == 0f)
        {
            matrix2 = RotationMatrix(angle, -tY);
        }
        else
        {
            matrix2 = RotationMatrix(angle, cross);
        }

        Matrix4x4 finalMatrix = matrix2 * matrix1;
        return finalMatrix;
    }

    // Source from: https://forum.unity.com/threads/how-to-assign-matrix4x4-to-transform.121966/
    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    // Source from: https://forum.unity.com/threads/how-to-assign-matrix4x4-to-transform.121966/
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        Quaternion rotation;
        
        if(matrix.ValidTRS())
            rotation = matrix.rotation;
        else
            rotation = new Quaternion(0f, 0f, 1f, 0f);

        // return Quaternion.LookRotation(forward, upwards);
        return rotation;
    }

    public static Matrix4x4 MatrixLookAt(Matrix4x4 from, Vector3 to, float inclination)
    {
        Vector3 fx = from.GetColumn(0);

        float angle = Algebra.Angle(fx, to);
        Vector3 cross = Vector3.Cross(fx, to).normalized;
        Matrix4x4 rotationMatrix = RotationMatrix(angle, cross);
        Matrix4x4 finalRotationMatrix = RotationMatrix(inclination, to) * rotationMatrix;

        return finalRotationMatrix;
    }

    public static float GetInclinationToMatrixLookAt(Matrix4x4 from, Matrix4x4 to)
    {
        Vector3 fx = from.GetColumn(0);
        Vector3 fy = from.GetColumn(1);
        Vector3 tx = to.GetColumn(0);
        Vector3 ty = to.GetColumn(1);

        float angle = Algebra.Angle(fx, tx);
        Vector3 cross = Vector3.Cross(fx, tx).normalized;
        Matrix4x4 rotationMatrix = RotationMatrix(angle, cross);
        if (angle >= 3.1415)
            angle = 0f;
        if (angle != 0f)
        {
            fy = rotationMatrix.MultiplyPoint3x4(fy);
        }
        
        float inclination = Algebra.Angle(fy, ty);
        Matrix4x4 rotationMatrix2 = RotationMatrix(inclination, tx);
        if (inclination >= 3.1415)
            inclination = 0f;
        if (inclination != 0f)
        {
            fy = rotationMatrix2.MultiplyPoint3x4(fy);
        }
        if ((ty - fy).magnitude > 0.001f)
        {
            inclination = -inclination;
        }

        return inclination;
    }

    // Based on Mathematics for 3D Game Programming and Computer Graphics, Third Edition, chapter 5.2
    public static float GetSignedDistanceFromPlane(Vector3 planeNormal, Vector3 pointInPlane, Vector3 point)
    {
        float D = -Vector3.Dot(planeNormal, pointInPlane);
        return Vector3.Dot(planeNormal, point) + D;
    }

    public static float GetSignedYAngleFromPlane(Vector3 planeNormal, Vector3 pointInPlane, Vector3 point, Vector3 pointDirection)
    {
        float signedDistance = GetSignedDistanceFromPlane(planeNormal, pointInPlane, point);
        if(signedDistance > 0)
            planeNormal = -planeNormal;
        Vector3 projected = Vector3.ProjectOnPlane(pointDirection, Vector3.up).normalized;
        float angle = Angle(planeNormal, projected);
        if((RotationMatrix(angle,Vector3.Cross(planeNormal, projected)).MultiplyPoint3x4(projected) - planeNormal).magnitude > 0.1f)
            angle = -angle;
        return angle;
    }
}
