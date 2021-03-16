using UnityEngine;

public abstract class Supports
{
    public abstract Material GetMaterial();
    public abstract (Mesh[], Material[]) GenerateMeshes(Rail rail, float t);
}
