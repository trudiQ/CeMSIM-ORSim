using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyComponent : MonoBehaviour
{
    public Component componentToDestroy;

    public void DoDestroyComponent() { Destroy(componentToDestroy); }
}
