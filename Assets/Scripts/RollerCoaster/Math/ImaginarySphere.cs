using UnityEngine;
using static Algebra;

public class ImaginarySphere
{
    public static (Bezier, Matrix4x4, Vector3) CalculateImaginarySphere(SpaceProps sp, RailProps rp, float radius = -1f)
    {
        Vector3 position = sp.Position;
        Matrix4x4 basis = sp.Basis;

        // Calculates the rotation matrix to the final segment basis
        Matrix4x4 rotationMatrix = ThreeRotationMatrix(basis, rp);                  

        // Calculates the radius of the imaginary sphere
        Vector3 v1 = basis.GetColumn(0);
        Vector3 v2 = rotationMatrix.MultiplyPoint3x4(v1);

        float angle = Angle(v1, v2);
        if(radius == -1)
        {
            radius = rp.Length;
            if((angle < -0.01 && angle > -3.1414) || (angle > 0.01 && angle < 3.1414))
            {
                radius = rp.Length * (Mathf.Tan(0.5f * angle) / angle);
            }
            else
            {
                radius *= 0.5f;
            }
        }

        // Calculates the final basis
        Matrix4x4 finalBasis = rotationMatrix * basis;

        // Calculates the Bezier points and curve
        Vector3[] bezierPoints = new Vector3[4];
        bezierPoints[0] = position;
        bezierPoints[1] = position + approximate(radius, v1);

        bezierPoints[3] = position + radius * v1 + radius * v2;
        bezierPoints[2] = bezierPoints[3] - approximate(radius, v2);

        Bezier curve = new Bezier(bezierPoints);

        Vector3 finalPoint = bezierPoints[3];

        return((curve, finalBasis, finalPoint));
    }

    private static Vector3 approximate(float radius, Vector3 vector)
    {
        return 0.55191496f * radius * vector;
    }
}
