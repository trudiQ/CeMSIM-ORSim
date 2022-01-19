using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class CollisionFilterField : ParticleTriggerField
{
    private int defaultPhase = 0;
    private int excludePseudoColonPhase = 0;

    public override void Start()
    {
        base.Start();

        defaultPhase = ObiUtils.MakeFilter(0x0000fffe, 0);
        excludePseudoColonPhase = ObiUtils.MakeFilter(0x0000fffa, 0);
    }

    protected override void OnParticlesEnterField(List<int> particles)
    {
        foreach(int p in particles)
        {
            solver.filters[p] = excludePseudoColonPhase;
        }
    }

    protected override void OnParticlesExitField(List<int> particles)
    {
        foreach (int p in particles)
        {
            solver.filters[p] = defaultPhase;
        }
    }
}
