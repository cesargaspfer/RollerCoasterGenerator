using UnityEngine;

public class Generator
{
    private RollerCoaster _rc;
    public Generator(RollerCoaster rollerCoaster)
    {
        _rc = rollerCoaster;
        TestCoaster();
    }

    public void Generate()
    {
        _rc.AddRail();
        _rc.UpdateLastRailAdd(railType: 0);
        for(int i = 0; i < 30; i++)
        {
            _rc.AddRail();
            _rc.UpdateLastRailAdd(elevation: ((int)Random.Range(-6, 7)) * Mathf.PI / 18f, rotation: ((int) Random.Range(-6, 7)) * Mathf.PI / 18f, inclination: 0f, length: 0, railType: 1);
        }
        _rc.AddRail();
        _rc.AddFinalRail();
    }

    private void TestCoaster()
    {
        float pi = Mathf.PI;
        _rc.AddRail();
        _rc.UpdateLastRailAdd(railType: 0);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 6f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 2);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: -pi / 4f, rotation: 0f, inclination: 0f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: pi / 4f, rotation: pi / 4f, inclination: -pi / 4f, length: 0, railType: 1);
        _rc.AddRail();
        _rc.UpdateLastRailAdd(elevation: 0f, rotation: pi / 4f, inclination: 0f, length: 0, railType: 3);
        _rc.AddRail();
        _rc.AddFinalRail();

        // Debug Rails

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.25f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, -Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, -Mathf.PI * 0.25f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.25f, Mathf.PI * 0.25f, Mathf.PI * 0.5f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI * 0.25f, Mathf.PI * 0.25f, Mathf.PI * 0.5f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI / 6f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI / 6f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI / 12f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI / 12f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI / 12f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI / 12f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.AddRail();
        // this.AddRail();
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.25f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(0f, Mathf.PI * 0.25f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(Mathf.PI * 0.5f, 0f, 0f, 5f));
        // this.AddRail();
        // this.UpdateLastRail(rp: _currentGlobalrp + new RailProps(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, 0f, 5f));
        // this.AddRail();
        // this.AddFinalRail();

        // _mp.ModelId = 0;
        // for(float elevation = -90f; elevation <= 90f; elevation += 15f)
        // {
        //     for (float rotation = -90f; rotation <= 90f; rotation += 15f)
        //     {
        //         _finalPosition = Vector3.zero;
        //         _finalBasis = Matrix4x4.identity;
        //         this.AddRail();
        //         _lastGlobalrp = new RailProps(0f, 0f, 0f, rp.Length);
        //         _currentGlobalrp = new RailProps(elevation * Mathf.PI / 180f, rotation * Mathf.PI / 180f, 0f, rp.Length);

        //         _finalPosition = Vector3.zero;
        //         _finalBasis = Matrix4x4.identity;
        //         this.UpdateRail(rp: _currentGlobalrp);
        //     }
        // }
    }
}
