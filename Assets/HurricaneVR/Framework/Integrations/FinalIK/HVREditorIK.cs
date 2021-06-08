using RootMotion.FinalIK;
using UnityEngine;

namespace HurricaneVR.Framework.Shared.HandPoser
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(VRIK))]
    [RequireComponent(typeof(HVRIKTargets))]
    public class HVREditorIK : MonoBehaviour
    {
        public Transform LeftTarget => Targets.LeftTarget;
        public Transform RightTarget => Targets.RightTarget;

        public HVRIKTargets Targets;

        private VRIK ik;

        void Start()
        {
            ik = GetComponent<VRIK>();
            Targets = GetComponent<HVRIKTargets>();

            ik.solver.SetToReferences(ik.references);
            ik.solver.Initiate(ik.transform);
        }

        void OnEnable()
        {
            Start();
        }

        void Update()
        {
            if (ik == null) return;

            if (ik.fixTransforms) ik.solver.FixTransforms();

            // Apply animation here if you want
            ik.solver.leftArm.target = LeftTarget;

            ik.solver.rightArm.target = RightTarget;

            ik.solver.Update();
        }
    }
}