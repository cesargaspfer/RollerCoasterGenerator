using UnityEngine;
using static Extruder;
using static RailModelProperties;

public abstract class RailModel
{
    protected private SubRailModel[] _srm;
    protected abstract SubRailModel[] GetSubRailModels();
    public virtual (Mesh[], Material[]) GenerateMeshes(RailProps rp, ModelProps mp, SpaceProps sp, Mesh[] meshes)
    {
        if (_srm == null)
        {
            _srm = this.GetSubRailModels();
        }

        RailSurface[] rs = new RailSurface[_srm.Length];
        ExtrudedRailSurface[] esr = new ExtrudedRailSurface[_srm.Length];


        for (int i = 0; i < _srm.Length; i++)
        {
            // Getting the surface to extrude
            rs[i] = _srm[i].GetRailSurface(mp.Type);

            // Extruding
            int extrusionResolution = _srm[i].GetExtrusionResolution(rp.Length, mp);
            (Vector3[][] extrudedVertices, Vector3[][] extrudedNormals) = Extrude(rs[i].Points, rs[i].Normals,
                                                                            sp, rp, extrusionResolution);
            esr[i] = new ExtrudedRailSurface(extrudedVertices, extrudedNormals);
        }

        if (meshes == null)
        {
            meshes = new Mesh[esr.Length];
            for (int i = 0; i < esr.Length; i++)
                // Triangulating the vertices and generating UV
                meshes[i] = _srm[i].GenerateMesh(mp.Type, esr[i]);
        }
        else
            for (int i = 0; i < esr.Length; i++)
                // Updating the mesh's vertices and normals
                _srm[i].UpdateMesh(esr[i], meshes[i]);

        Material[] materials = new Material[_srm.Length];
        for (int i = 0; i < esr.Length; i++)
            materials[i] = _srm[i].GetMaterial(mp.Type);

        for(int i = 0; i < meshes.Length; i++)
            meshes[i].RecalculateBounds();
            
        return (meshes, materials);
    }
}
