using UnityEngine;

public class RailPhysics
{
    public class Props
    {
        private float _velocity;
        private Vector3 _GForce;
        private bool _defined;

        public Props(float velocity, Vector3 GForce)
        {
            _velocity = velocity;
            _GForce = GForce;

            _defined = true;
        }

        public float Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public Vector3 GForce
        {
            get { return _GForce; }
            set { _GForce = value; }
        }

        public bool IsDefined
        {
            get { return _defined; }
        }
    }
    
    private Props _initial;
    private Props _max;
    private Props _final;

    bool _carCompletedSegment = false;

    public RailPhysics (Props initial)
    {
        _initial = initial;
    }

    public Props Initial
    {
        get { return _initial; }
        set { _initial = value; }
    }

    public Props Max
    {
        get { return _max; }
        set { _max = value; }
    }

    public Props Final
    {
        get { return _final; }
        set { _final = value; }
    }

    public bool CarCompletedSegment
    {
        get { return _carCompletedSegment; }
        set { _carCompletedSegment = value; }
    }
}
