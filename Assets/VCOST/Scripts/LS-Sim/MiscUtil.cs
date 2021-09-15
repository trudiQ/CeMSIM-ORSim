using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MiscUtil : MonoBehaviour
{
    [ShowInInspector]
    public void ModifyConfigurableJoint()
    {
        foreach(ConfigurableJoint c in FindObjectsOfType<ConfigurableJoint>())
        {
            c.enablePreprocessing = false;
            c.projectionMode = JointProjectionMode.PositionAndRotation;
        }
    }
}
