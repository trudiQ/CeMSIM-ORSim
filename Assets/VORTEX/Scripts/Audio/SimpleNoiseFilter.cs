using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SimpleNoiseFilter : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static float SimpleGaussian(System.Random random, double mean, double stddev)
    {
        double x1 = 1 - random.NextDouble();
        double x2 = 1 - random.NextDouble();

        double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
        return (float)(y1 * stddev + mean);
    }

}
