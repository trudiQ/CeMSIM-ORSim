using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugularVenousDistension : MonoBehaviour
{
    [SerializeField]
    private int leftJVDKeyIndex = 0;
    [SerializeField]
    private int rightJVDKeyIndex = 0;
    [SerializeField]
    private int percentInflatedLeft = 0;
    [SerializeField]
    private int percentInflatedRight = 0;
    private SkinnedMeshRenderer meshRenderer;

    // private AbdominalInflation abdominalInflation;
    // private InsufflationPressure insufflationPressure;
    // private JugularVenousDistension jugularVenousDistension;
    // private TrachealDeviation trachealDeviation;
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = this.GetComponent<SkinnedMeshRenderer>();

        // abdominalInflation = this.GetComponent<AbdominalInflation>();
        // insufflationPressure = this.GetComponent<InsufflationPressure>();
        // jugularVenousDistension = this.GetComponent<JugularVenousDistension>();
        // trachealDeviation = this.GetComponent<TrachealDeviation>();
    }

    // Update is called once per frame
    void Update()
    {
        // int p = insufflationPressure.GetPercentInflated();
        // float severity = ScenarioManager.Instance.pneumothoraxSeverity;
        // jugularVenousDistension.SetLeftDistension((int)(100 * (1.2f - severity)));
        // jugularVenousDistension.SetRightDistension((int)(100 *(1.2f - severity)));
        // trachealDeviation.SetDeviationPercent((int)(100 * (1.2f - severity)));
    }

    public void SetLeftDistension(int value)
    {
        if(value <= 100)
        {
            percentInflatedLeft = value;
            meshRenderer.SetBlendShapeWeight(leftJVDKeyIndex, percentInflatedLeft);
        }
    }

    public void SetRightDistension(int value)
    {
        if(value <= 100)
        {
            percentInflatedRight = value;
            meshRenderer.SetBlendShapeWeight(rightJVDKeyIndex, percentInflatedRight);
        }
    }
}
