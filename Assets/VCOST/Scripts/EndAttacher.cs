using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

public class EndAttacher : MonoBehaviour
{
    private PositionConstraint p;

    public UnityEvent _e_TriggerEnterEvent;

    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<PositionConstraint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Obi") return;
        ConstraintSource source = new ConstraintSource() { sourceTransform = other.transform, weight = 1 };
        p.AddSource(source);
        p.constraintActive = true;
        p.translationOffset = transform.position - other.transform.position;
    }
}
