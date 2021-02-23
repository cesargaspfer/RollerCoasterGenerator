using UnityEngine;

[System.Serializable]
public class Bezier
{
    [SerializeField] private static int _lengthResolution = 20;
    [SerializeField] private static float _maxDistance = 0.1f;

    [SerializeField] private Vector3[] _points;
    [SerializeField] private Vector3[] _tangentVectors;
    [SerializeField] private float _length = -1f;
    
    public Bezier()
    {
        _points = new Vector3[4];
        for(int i = 0; i < 4; i++)
        {
            _points[i] = Vector3.zero;
        }

        _tangentVectors = new Vector3[3];
        _tangentVectors[0] = Vector3.zero;
        _tangentVectors[1] = Vector3.zero;
        _tangentVectors[2] = Vector3.zero;
        _length = -1f;
    }

    public Bezier(Vector3[] points)
    {
        _tangentVectors = new Vector3[3];
        _tangentVectors[0] = Vector3.zero;
        _tangentVectors[1] = Vector3.zero;
        _tangentVectors[2] = Vector3.zero;
        this.updatePoints(points);
    }

    public Bezier Clone()
    {
        return new Bezier(_points);
    }

    public void updatePoints(Vector3[] points)
    {
        Debug.Assert(points.Length == 4, "The length of points isn't equals 4");
        for(int i = 0; i < 4; i++)
            Debug.Assert(points[i] != null, "points[" + i + "] is null");

        _points = points;

        _tangentVectors[0] = - 3f * _points[0] + 9f * _points[1] - 9f * _points[2] + 3f * _points[3];
        _tangentVectors[1] = 6f * _points[0] - 12f * _points[1] + 6f * _points[2];
        _tangentVectors[2] = - 3f * _points[0] + 3f * _points[1];
    }

    public Vector3 Sample(float t)
    {
        t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
            (
                oneMinusT * oneMinusT * oneMinusT * _points[0] +
                3f * oneMinusT * oneMinusT * t * _points[1] +
                3f * oneMinusT * t * t * _points[2] +
                t * t * t * _points[3]
            );
    }

    // Based on: https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve
    public Vector3 GetFirstDerivativeAt(float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
            (
                t * t * _tangentVectors[0] +
                t * _tangentVectors[1] +
                _tangentVectors[2]
            );
	}

    public Vector3 GetTangentAt(float t) {
        Vector3 derivative = this.GetFirstDerivativeAt(t);
        if(derivative.magnitude > 0)
		    return derivative.normalized;
        return derivative;
	}
    
    public float GetLength()
    {
        if (_length != -1)
            return _length;

        _length = 0f;
        for(int i = 0; i < _lengthResolution; i++)
        {
            Vector3 a = this.Sample((float) i / (float)_lengthResolution);
            Vector3 b = this.Sample((float) (i  + 1) / (float)_lengthResolution);
            _length += Vector3.Distance(a, b);
        }
        return _length;
    }

    // Based on: https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve
    public float GetNextT(float t, float distance)
    {
        if(distance > _maxDistance)
        {
            int resolution = Mathf.CeilToInt(distance / _maxDistance);
            float reducedDistance = distance / resolution;

            for(int i = 1; i <= resolution; i++)
            {
                t += reducedDistance / GetFirstDerivativeAt(t).magnitude;
            }

            return t;
        }
        else
        {
            return t + distance / GetFirstDerivativeAt(t).magnitude;
        }
    }

    public float Length
    {
        get {
            return GetLength();
        }
    }
}
