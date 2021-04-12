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
    [SerializeField] private Transform _groundPrefab;
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
    #pragma warning disable 0649
    [SerializeField] private int _chunksLength;

    [SerializeField] private float[] _amplifiers;
    [SerializeField] private Transform[] _grounds;
    [SerializeField] private Vector3[][] _vertices;
    [SerializeField] private float _chunkSize;
    [SerializeField] private float _offset;

    void Start()
    {
        float offset = _size * 0.5f;

        _amplifiers = ReadHeightmap();

        _grounds = new Transform[_chunksLength * _chunksLength];
        _vertices = new Vector3[_chunksLength * _chunksLength][];
        _chunkSize = _size / (float)_chunksLength;
        _offset = _size * 0.5f;
        for(int i = 0; i < _chunksLength; i++)
        {
            for(int j = 0; j < _chunksLength; j++)
            {
                Instantiate(i, j);
            }
        }
    }

    public void Instantiate(int groundIndexX, int groundIndexZ)
    {
        Transform ground = Instantiate(_groundPrefab, Vector3.zero, Quaternion.identity);
        ground.transform.SetParent(this.transform);

        float xPos = groundIndexX * _chunkSize - _offset;
        float zPos = groundIndexZ * _chunkSize - _offset;

        ground.transform.localPosition = new Vector3(xPos, 0f, zPos);
        ground.transform.localScale = Vector3.one;
        ground.transform.name = "Ground";

        int groundIndex = groundIndexX * _chunksLength + groundIndexZ;
        _grounds[groundIndex] = ground;

        Mesh mesh = new Mesh();
        _vertices[groundIndex] = new Vector3[_resolution * _resolution];
        Vector3[] normals = new Vector3[_vertices[groundIndex].Length];
        int[] triangles = new int[((_resolution - 1) * (_resolution - 1)) * 6];
        Vector2[] UVs = new Vector2[_vertices[groundIndex].Length];


        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                float x = _chunkSize * ((float)i / (float)(_resolution - 1));
                float z = _chunkSize * ((float)j / (float)(_resolution - 1));
                _vertices[groundIndex][i * _resolution + j] = new Vector3(x, GetHeight(new Vector2(x + xPos, z + zPos)), z);
            }
        }

        for (int i = 0; i < _resolution - 1; i++)
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

        mesh.vertices = _vertices[groundIndex];
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();

        ground.GetComponent<MeshFilter>().mesh = mesh;
        ground.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // TODO: Use in support
    public float GetHeight(Vector2 position)
    {
        float newX = (position.x + _offset) * ((float) ((_resolution - 1) * (_chunksLength - 1)) / _size);
        float newZ = (position.y + _offset) * ((float) ((_resolution - 1) * (_chunksLength - 1)) / _size);

        position = new Vector2(newX, newZ);
        float x = position.x;
        float z = position.y;

        int xMin = Mathf.Max(0, Mathf.RoundToInt(x - _convolutionSize - 1));
        int zMin = Mathf.Max(0, Mathf.RoundToInt(z - _convolutionSize - 1));
        int xMax = Mathf.Min(_resolution * _chunksLength, Mathf.RoundToInt(x + _convolutionSize + 1));
        int zMax = Mathf.Min(_resolution * _chunksLength, Mathf.RoundToInt(z + _convolutionSize + 1));

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
        return _amplifiers[aX * _resolution * _chunksLength + aZ] * Mathf.Exp(exponent) / (2 * Mathf.PI * sigma2);
    }

    public float[] GetAplifiers()
    {
        return _amplifiers;
    }

    public void SetAplifiers(float[] amplifiers)
    {
        if(amplifiers == null)
            amplifiers = new float[_resolution * _resolution * _chunksLength * _chunksLength];
        if(amplifiers.Length != _resolution * _resolution * _chunksLength * _chunksLength)
        {
            Debug.LogError("amplifiers.Length != " + _resolution * _resolution * _chunksLength * _chunksLength + "in Terrain.SetAplifiers");
            return;
        }
        _amplifiers = amplifiers;
        float offset = _size * 0.5f;
        for(int groundX = 0; groundX < _chunksLength; groundX++)
        {
            for (int groundZ = 0; groundZ < _chunksLength; groundZ++)
            {
                int groundIndex = groundX * _chunksLength + groundZ;
                float xPos = groundX * _chunkSize - _offset;
                float zPos = groundZ * _chunkSize - _offset;
                for (int i = 0; i < _resolution; i++)
                {
                    for (int j = 0; j < _resolution; j++)
                    {
                        float x = _chunkSize * ((float)i / (float)(_resolution - 1));
                        float z = _chunkSize * ((float)j / (float)(_resolution - 1));
                        _vertices[groundIndex][i * _resolution + j] = new Vector3(x, GetHeight(new Vector2(x + xPos, z + zPos)), z);
                    }
                }
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.vertices = _vertices[groundIndex];
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.RecalculateNormals();
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
                _grounds[groundIndex].GetComponent<MeshCollider>().sharedMesh = _grounds[groundIndex].GetComponent<MeshFilter>().mesh;
            }
        }
    }

    public void UpdateAplifiers(Vector3 worldPosition, float radius, float intencity, float opacity)
    {
        radius *= 0.5f;
        float newX = (worldPosition.x + _offset) * ((float)((_resolution - 1) * (_chunksLength - 1)) / _size);
        float newZ = (worldPosition.z + _offset) * ((float)((_resolution - 1) * (_chunksLength - 1)) / _size);
        float adjustedRadius = radius * ((float)((_resolution - 1) * (_chunksLength - 1)) / _size);

        Vector2 position = new Vector2(newX, newZ);
        float x = position.x;
        float z = position.y;

        int xMin = Mathf.Max(0, Mathf.RoundToInt(x - adjustedRadius - 1));
        int zMin = Mathf.Max(0, Mathf.RoundToInt(z - adjustedRadius - 1));
        int xMax = Mathf.Min(_resolution * _chunksLength - 1, Mathf.RoundToInt(x + adjustedRadius + 1));
        int zMax = Mathf.Min(_resolution * _chunksLength - 1, Mathf.RoundToInt(z + adjustedRadius + 1));

        for (int i = xMin; i <= xMax; i++)
        {
            if (Mathf.Abs(i - x) <= adjustedRadius)
            {
                for (int j = zMin; j <= zMax; j++)
                {
                    float distance = Vector3.Magnitude(position - new Vector2(i, j));
                    if (distance <= adjustedRadius)
                    {
                        _amplifiers[i * _resolution * _chunksLength + j] += Mathf.Lerp(1f, 0f, distance / adjustedRadius) * opacity * intencity;
                    }
                }
            }
        }
        radius *= 2;

        int xMinGround = Mathf.Max(0, (int) Mathf.Floor((_offset + worldPosition.x - adjustedRadius - 5) / (float) _chunkSize) - 1);
        int zMinGround = Mathf.Max(0, (int) Mathf.Floor((_offset + worldPosition.z - adjustedRadius - 5) / (float) _chunkSize) - 1);
        int xMaxGround = Mathf.Min(_chunksLength - 1, (int) Mathf.Ceil((_offset + worldPosition.x + adjustedRadius + 5) / (float) _chunkSize) + 1);
        int zMaxGround = Mathf.Min(_chunksLength - 1, (int) Mathf.Ceil((_offset + worldPosition.z + adjustedRadius + 5) / (float) _chunkSize) + 1);

        for (int groundX = xMinGround; groundX <= xMaxGround; groundX++)
        {
            for (int groundZ = zMinGround; groundZ <= zMaxGround; groundZ++)
            {
                int groundIndex = groundX * _chunksLength + groundZ;
                float xPos = groundX * _chunkSize - _offset;
                float zPos = groundZ * _chunkSize - _offset;

                float localX = (worldPosition.x - xPos) * ((float)((_resolution - 1)) / _chunkSize);
                float localZ = (worldPosition.z - zPos) * ((float)((_resolution - 1)) / _chunkSize);

                if((localX + adjustedRadius < -2 && localZ + adjustedRadius < -2) || (localX - adjustedRadius > _chunkSize + 2 && localZ - adjustedRadius > _chunkSize + 2))
                    continue;
                
                xMin = Mathf.Max(0, Mathf.RoundToInt(localX - radius + 2));
                zMin = Mathf.Max(0, Mathf.RoundToInt(localZ - radius + 2));
                xMax = Mathf.Min(_resolution, Mathf.RoundToInt(localX + radius + 2));
                zMax = Mathf.Min(_resolution, Mathf.RoundToInt(localZ + radius + 2));

                Vector2 localBrushPosition = new Vector2(localX, localZ);
                for (int i = xMin; i < xMax; i++)
                {
                    for (int j = zMin; j < zMax; j++)
                    {
                        float distance = Vector3.Magnitude(localBrushPosition - new Vector2(i, j));
                        if (distance <= radius + 2)
                        {
                            float tmpX = _chunkSize * ((float)i / (float)(_resolution - 1));
                            float tmpZ = _chunkSize * ((float)j / (float)(_resolution - 1));
                            _vertices[groundIndex][i * _resolution + j] = new Vector3(tmpX, GetHeight(new Vector2(tmpX + xPos, tmpZ + zPos)), tmpZ);
                        }
                    }
                }
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.vertices = _vertices[groundIndex];
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.RecalculateNormals();
                _grounds[groundIndex].GetComponent<MeshFilter>().mesh.RecalculateBounds();
                _grounds[groundIndex].GetComponent<MeshCollider>().sharedMesh = _grounds[groundIndex].GetComponent<MeshFilter>().mesh;
            }
        }
    }

    private float[] ReadHeightmap()
    {
        float[] map = new float[_resolution * _resolution * _chunksLength * _chunksLength];
        float resolutionLength = (float)((_resolution - 1) * (_chunksLength - 1));
        for (int i = 0; i < _resolution * _chunksLength; i++)
        {
            for (int j = 0; j < _resolution * _chunksLength; j++)
            {
                int x = (int)(((float)i / resolutionLength) * _heightmap.width);
                int z = (int)(((float)j / resolutionLength) * _heightmap.height);
                map[i * (_resolution * _chunksLength) + j] = Mathf.Clamp(_heightmap.GetPixel(x, z).grayscale - 0.04f, 0f, 1f) * 100f;
            }
        }
        return map;
    }
}
