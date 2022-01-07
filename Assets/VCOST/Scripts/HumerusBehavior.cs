using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class HumerusBehavior : MonoBehaviour
{
    public List<Transform> spawnAtTransforms;
    public HumerusRodBehavior p_rod;
    private List<HumerusRodBehavior> activeRods = new List<HumerusRodBehavior>();
    public ObiSolver solver;

    public CircularStaplerBehavior retractBehavior;

    public void SpawnRopes()
    {
        foreach(Transform t in spawnAtTransforms)
        {
            HumerusRodBehavior rod = Instantiate(p_rod, t.position, t.rotation, solver.transform);
            rod.AttachControlPointTo("left", solver.transform);
            rod.AttachControlPointTo("leftmid", solver.transform);
            rod.AttachControlPointTo("right", solver.transform);
            rod.AttachControlPointTo("rightmid", solver.transform);
            activeRods.Add(rod);
        }
    }

    public void EnableRetract()
    {
        retractBehavior.enabled = true;
    }

    public void SlackenRods()
    {
        foreach(HumerusRodBehavior h in activeRods)
        {
            h.DisableParticleAttachment("right");
        }
    }
}
