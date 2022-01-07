using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCreator : MonoBehaviour
{
    public StaticHoleColliderBehavior p_sphere;
    public int amountPerRing = 16;
    public float sphereOffsetDistanceFromRing = 2;
    public int amountOfRings = 3;
    public float ringOffsetDistance = 1f;
    public float lagMultiplierFalloffPerIteration = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < amountOfRings; i++)
        {
            CreateRingOfSpheres(
                transform.position + (Vector3.forward * ringOffsetDistance * i), 
                1 - (lagMultiplierFalloffPerIteration * i)
                );
        }
    }

    void CreateRingOfSpheres(Vector3 surroundingPosition, float lagMult)
    {
        float degreeIncrement = 360 / amountPerRing;
        float radianIncrement = degreeIncrement * (Mathf.PI / 180f);

        for (int i = 0; i < amountPerRing; i++)
        {
            Vector3 pos = new Vector3(
                (Mathf.Cos(radianIncrement * i) * sphereOffsetDistanceFromRing) + surroundingPosition.x,
                (Mathf.Sin(radianIncrement * i) * sphereOffsetDistanceFromRing) + surroundingPosition.y,
                surroundingPosition.z);
            if (transform.parent == null) Instantiate(p_sphere, pos, Quaternion.Euler(0, 0, degreeIncrement * i));
            else Instantiate(p_sphere, pos, Quaternion.Euler(0, 0, degreeIncrement * i), transform.parent);
        }
    }
}
