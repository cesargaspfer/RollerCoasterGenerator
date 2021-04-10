using UnityEngine;

public class Terrain : MonoBehaviour
{
    private static Terrain _inst;
    public static Terrain inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<Terrain>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private int _size;
    #pragma warning disable 0649
    [SerializeField] private int _resolution;
    #pragma warning disable 0649
    [SerializeField] private float _sigma;
    #pragma warning disable 0649
    [SerializeField] private float _convolutionSize;
    #pragma warning disable 0649
    [SerializeField] private Texture2D _heightmap;

    [SerializeField] private float[] _amplifiers;
    [SerializeField] private Vector3[] _vertices;

    void Start()
    {
        float offset = _size * 0.5f;

        _amplifiers = ReadHeightmap();

        Mesh mesh = new Mesh();
        _vertices = new Vector3[_resolution * _resolution];
        Vector3[] normals = new Vector3[_vertices.Length];
        int[] triangles = new int[((_resolution - 1) * (_resolution - 1)) * 6];
        Vector2[] UVs = new Vector2[_vertices.Length];


        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                float x = _size * ((float) i / (float) (_resolution - 1)) - offset;
                float z = _size * ((float) j / (float) (_resolution - 1)) - offset;
                _vertices[i * _resolution + j] = new Vector3(x, GetHeight(new Vector2(x, z)), z);
            }
        }

        for(int i = 0; i < _resolution - 1; i++)
        {
            for (int j = 0; j < _resolution - 1; j++)
            {
                int index = 6 * (i * (_resolution - 1) + j);
                triangles[index] = i * _resolution + j;
                triangles[index + 1] = i * _resolution + (j + 1);
                triangles[index + 2] = (i + 1) * _resolution + j;

                triangles[index + 3] = i * _resolution + (j + 1);
                triangles[index + 4] = (i + 1) * _resolution + (j + 1);
                triangles[index + 5] = (i + 1) * _resolution + j;
            }
        }

        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                UVs[i * _resolution + j] = (new Vector2(i, j)) / (_resolution - 1);
                normals[i * _resolution + j] = Vector3.up;
                int x = (int)(((float)i / (float)(_resolution - 1)) * _heightmap.width);
                int z = (int)(((float)j / (float)(_resolution - 1)) * _heightmap.height);
            }
        }

        mesh.vertices = _vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();

        this.GetComponent<MeshFilter>().mesh = mesh;
        this.GetComponent<MeshCollider>().sharedMesh = mesh;
    }


    // TODO: Use in support
    public float GetHeight(Vector2 position)
    {
        float newX = (position.x + _size * 0.5f) * ((float) (_resolution - 1) / _size);
        float newZ = (position.y + _size * 0.5f) * ((float) (_resolution - 1) / _size);

        position = new Vector2(newX, newZ);
        float x = position.x;
        float z = position.y;
        float xRound = Mathf.Round(x);
        float zRound = Mathf.Round(z);

        int xMin = Mathf.Max(0, Mathf.RoundToInt(x - _convolutionSize - 1));
        int zMin = Mathf.Max(0, Mathf.RoundToInt(z - _convolutionSize - 1));
        int xMax = Mathf.Min(_resolution, Mathf.RoundToInt(x + _convolutionSize + 1));
        int zMax = Mathf.Min(_resolution, Mathf.RoundToInt(z + _convolutionSize + 1));

        float height = 0f;
        for(int i = xMin; i < xMax; i++)
        {
            if (Mathf.Abs(i - x) <= _convolutionSize)
            {
                for (int j = zMin; j < zMax; j++)
                {
                    if(Mathf.Abs(j - z) <= _convolutionSize)
                    {
                        height += Sample(position, i, j);
                    }
                }
            }
        }

        return height;
    }

    private float Sample(Vector2 pos, int aX, int aZ)
    {
        float sigma2 = _sigma * _sigma;
        float x = aX - pos.x;
        float z = aZ - pos.y;
        float exponent = - (x * x + z * z) / (sigma2);
        return _amplifiers[aX * _resolution + aZ] * Mathf.Exp(exponent) / (2 * Mathf.PI * sigma2);
    }

    public float[] GetAplifiers()
    {
        return _amplifiers;
    }

    public void SetAplifiers(float[] amplifiers)
    {
        if(amplifiers == null)
            amplifiers = new float[_resolution * _resolution];
        if(amplifiers.Length != _resolution * _resolution)
        {
            Debug.LogError("amplifiers.Length != " + _resolution * _resolution + "in Terrain.SetAplifiers");
            return;
        }
        _amplifiers = amplifiers;
        float offset = _size * 0.5f;
        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                float x = _size * ((float)i / (float)(_resolution - 1)) - offset;
                float z = _size * ((float)j / (float)(_resolution - 1)) - offset;
                _vertices[i * _resolution + j] = new Vector3(x, GetHeight(new Vector2(x, z)), z);
            }
        }
        this.GetComponent<MeshFilter>().mesh.vertices = _vertices;
        this.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    private float[] ReadHeightmap()
    {
        float[] map = new float[_resolution * _resolution];
        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                int x = (int)(((float)i / (float)(_resolution - 1)) * _heightmap.width);
                int z = (int)(((float)j / (float)(_resolution - 1)) * _heightmap.height);
                map[i * _resolution + j] = Mathf.Clamp(_heightmap.GetPixel(x, z).grayscale - 0.04f, 0f, 1f) * 100f;
            }
        }
        return map;
    }
}
