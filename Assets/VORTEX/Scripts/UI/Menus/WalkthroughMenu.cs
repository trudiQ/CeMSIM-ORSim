using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WalkthroughMenu : Menu<WalkthroughMenu>
{
    public Text stepText;
    public Text objectiveText;

    public string obj1 = "Objective 1";
    public string obj2 = "Objective 2";

    public string step1="Step 1";
    public string step2="Step 2";
    public string step3="Step 3";

    // Start is called before the first frame update
    void Start()
    {
        SetObjectiveText(1);
        SetStepText(1);
    }

    public void SetObjectiveText(int i)
    {
        if(i==1)
        {
            objectiveText.text = obj1;
        }
        else if (i == 2)
        {
            objectiveText.text = obj2;
        }
    }

    public void SetStepText(int i)
    {
        if(i==1)
        {
            stepText.text = step1;
        }
        else if (i == 2)
        {
            stepText.text = step2;
        }
        else if(i ==3)
        {
            stepText.text = step3;
        }
    }
}
