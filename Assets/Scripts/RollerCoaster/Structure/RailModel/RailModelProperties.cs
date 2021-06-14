using UnityEngine;

public class RailModelProperties
{
    public enum RailType
    {
        Platform,
        Normal,
        Lever,
        Brake,
        Impulsor,
    };

    public struct RailSurface
    {
        private Vector3[] _points;
        private Vector3[] _normals;

        public RailSurface(Vector3[] points, Vector3[] normals)
        {
            _points = points;
            _normals = normals;
        }

        public Vector3[] Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public Vector3[] Normals
        {
            get { return _normals; }
            set { _normals = value; }
        }
    }

    public struct ExtrudedRailSurface
    {
        private Vector3[][] _points;
        private Vector3[][] _normals;
        
        public ExtrudedRailSurface(Vector3[][] points, Vector3[][] normals)
        {
            _points = points;
            _normals = normals;
        }

        public Vector3[][] Points
        {
            get { return _points; }
            set { _points = value; }
        }

        public Vector3[][] Normals
        {
            get { return _normals; }
            set { _normals = value; }
        }
    }
}
