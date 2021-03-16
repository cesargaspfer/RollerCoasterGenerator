public class SupportsManager
{
    public static Supports[] _supports = null;

    public static Supports GetSupports(int id)
    {
        if (_supports == null)
            GenerateSupports();

        return _supports[id];
    }

    private static void GenerateSupports()
    {
        _supports = new Supports[1]
        {
            new CylinderSupport(),
        };
    }
}

