using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class ParticleTriggerField : MonoBehaviour
{
    private struct FloatPair
    {
        public float InvMass;
        public float InvRotMass;
    }

    public ObiSolver solver;
    protected Matrix4x4 solver2World;
    protected Vector3 WorldPositionOfParticle(int p) { return solver2World.MultiplyPoint3x4(solver.positions[p]); }
    protected Matrix4x4 world2Solver;
    protected Vector3 SolverPositionOfPoint(Vector3 position) { return world2Solver.MultiplyPoint3x4(position); }

    private Dictionary<int, FloatPair> particleHistory = new Dictionary<int, FloatPair>();
    protected void AddNewParticlesToParticleHistory(List<int> particles)
    {
        foreach (int p in particles)
        {
            if (!particleHistory.ContainsKey(p))
                particleHistory.Add(p,
                    new FloatPair()
                    {
                        InvMass = solver.invMasses[p],
                        InvRotMass = solver.invRotationalMasses[p]
                    });
        }
    }

    protected List<int> _justExitedParticles = new List<int>();
    protected List<int> _stayingParticles = new List<int>();
    protected List<int> _justEnteredParticles = new List<int>();

    protected List<int> previousFrameParticles = new List<int>();

    public virtual void Start()
    {
        solver2World = solver.transform.localToWorldMatrix;
        world2Solver = solver.transform.worldToLocalMatrix;
    }

    protected void StartManipulatingParticles(List<int> particles)
    {
        if (particles.Count == 0) return;
        AddNewParticlesToParticleHistory(particles);

        foreach (int p in particles)
        {
            solver.invMasses[p] = -1;
            solver.invRotationalMasses[p] = -1;
        }
    }

    protected void StopManipulatingParticles(List<int> particles)
    {
        if (particles.Count == 0) return;
        foreach (int p in particles)
        {
            solver.invMasses[p] = particleHistory[p].InvMass;
            solver.invRotationalMasses[p] = particleHistory[p].InvRotMass;
        }
    }

    protected virtual List<int> CurrentFrameParticleFilter(List<int> particles) { return particles; }
    protected virtual void OnParticlesEnterField(List<int> particles) { }
    protected virtual void OnParticlesExitField(List<int> particles) { }
    protected virtual void OnParticlesStayInField(List<int> particles) { }
    public void TriggerField(List<int> currentFrameParticles)
    {
        currentFrameParticles.Sort();

        CurrentFrameParticleFilter(currentFrameParticles);

        _justExitedParticles = new List<int>();
        _stayingParticles = new List<int>();
        _justEnteredParticles = new List<int>();

        foreach (int p in previousFrameParticles)
        {
            if (currentFrameParticles.Contains(p))
            {
                _stayingParticles.Add(p);
            }
            else
            {
                _justExitedParticles.Add(p);
            }
        }

        foreach (int p in currentFrameParticles)
        {
            if (!previousFrameParticles.Contains(p))
                _justEnteredParticles.Add(p);
        }

        OnParticlesEnterField(_justEnteredParticles);
        OnParticlesExitField(_justExitedParticles);
        OnParticlesStayInField(_stayingParticles);

        previousFrameParticles = new List<int>();
        foreach (int p in currentFrameParticles)
        {
            previousFrameParticles.Add(p);
        }
    }
}
