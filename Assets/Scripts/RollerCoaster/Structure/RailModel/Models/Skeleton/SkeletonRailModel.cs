using UnityEngine;
using static RailModelProperties;

public class SkeletonRailModel : RailModel
{
    private RailSurface[] _rs;

    protected override SubRailModel[] GetSubRailModels()
    {
        return new SubRailModel[8]
        {
            new SkeletonSRMCylinder(),
            new SkeletonSRMSides(),
            
            new ThreeCylindersSRMCube(),
            new ThreeCylindersSRMCubeSides(),

            new SkeletonSRMTransversals(0),
            new SkeletonSRMTransversals(1),
            new SkeletonSRMTransversals(2),
            new SkeletonSRMTransversalsCylinders(),
        };
    }
}