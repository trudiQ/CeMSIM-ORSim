using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPETracker : MonoBehaviour
{
    public GameObject RightGlove0;
    public GameObject LeftGlove0;
    public GameObject RightGlove1;
    public GameObject LeftGlove1;
    public GameObject N95Mask;
    public GameObject EyeProtection;
    public GameObject HeadCover;


    public List<string> PPEDon;
    public List<string> PPEDoff;

    private bool GlovesDonBoolR0;
    private bool GlovesDonBoolL0;
    private bool GlovesDonBoolR1;
    private bool GlovesDonBoolL1;
    private bool N95MaskDonBool;
    private bool EPDonBool;
    private bool HeadCoverDonBool;



    private bool GlovesDoffBoolR0;
    private bool GlovesDoffBoolL0;
    private bool GlovesDoffBoolR1;
    private bool GlovesDoffBoolL1;
    private bool N95MaskDoffBool;
    private bool EPDoffBool;
    private bool HeadCoverDoffBool;


    private bool RightGlovesBool0;
    private bool LeftGlovesBool0;
    private bool RightGlovesBool1;
    private bool LeftGlovesBool1;
    private bool N95MaskBool;
    private bool EyeProtectionBool;
    private bool HeadCoverBool;
    // Start is called before the first frame update
    void Start()
    {
        GlovesDonBoolR0 = true;
        GlovesDonBoolL0 = true;
        GlovesDonBoolR1 = true;
        GlovesDonBoolL1 = true;
        N95MaskDonBool = true;
        EPDonBool = true;
        HeadCoverDonBool = true;

        GlovesDoffBoolR0 = false;
        GlovesDoffBoolL0 = false;
        GlovesDoffBoolR1 = false;
        GlovesDoffBoolL1 = false;
        N95MaskDoffBool = false;
        EPDoffBool = false;
        HeadCoverDoffBool = false;
    }

    // Update is called once per frame
    void Update()
    {

        RightGlovesBool0 = RightGlove0.activeSelf;
        LeftGlovesBool0 = LeftGlove0.activeSelf;
        RightGlovesBool1 = RightGlove1.activeSelf;
        LeftGlovesBool1 = LeftGlove1.activeSelf;
        N95MaskBool = N95Mask.activeSelf;
        EyeProtectionBool = EyeProtection.activeSelf;
        HeadCoverBool = HeadCover.activeSelf;
        if (RightGlovesBool0 == false && GlovesDonBoolR0 == true)
        {
            PPEDon.Add("glove");
            GlovesDonBoolR0 = false;
            GlovesDoffBoolR0 = true;

        }
        else if (LeftGlovesBool0 == false && GlovesDonBoolL0 == true)
        {
            PPEDon.Add("glove");
            GlovesDonBoolL0 = false;
            GlovesDoffBoolL0 = true;
        }
        else if (RightGlovesBool1 == false && GlovesDonBoolR1 == true)
        {
            PPEDon.Add("glove");
            GlovesDonBoolR1 = false;
            GlovesDoffBoolR1 = true;
        }
        else if (LeftGlovesBool1 == false && GlovesDonBoolL1 == true)
        {
            PPEDon.Add("glove");
            GlovesDonBoolL1 = false;
            GlovesDoffBoolL1 = true;
        }
        else if(N95MaskBool == false && N95MaskDonBool == true)
        {
            PPEDon.Add("N95Mask");
            N95MaskDonBool = false;
            N95MaskDoffBool = true;
        }
        else if (EyeProtectionBool == false && EPDonBool == true)
        {
            PPEDon.Add("Eye Protection");
            EPDonBool = false;
            EPDoffBool = true;
        }
        else if (HeadCoverBool == false && HeadCoverDonBool == true)
        {
            PPEDon.Add("Head Cover");
            HeadCoverDonBool = false;
            HeadCoverBool = true;
        }





        //if (RightGlovesBool0 == true && GlovesDoffBoolR0 == true)
        //{
        //    PPEDoff.Add("glove");
        //    GlovesDoffBoolR0 = false;

        //}
        //else if (LeftGlovesBool0 == true && GlovesDoffBoolL0 == true)
        //{
        //    PPEDoff.Add("glove");
        //    GlovesDoffBoolL0 = false;
        //}
        //else if (RightGlovesBool1 == true && GlovesDoffBoolR1 == true)
        //{
        //    PPEDoff.Add("glove");
        //    GlovesDonBoolR1 = false;
    //    }
    //    else if (LeftGlovesBool1 == false && GlovesDoffBoolL1 == true)
    //    {
    //        PPEDoff.Add("glove");
    //        GlovesDoffBoolL1 = false;
    //    }
    //    else if (N95MaskBool == true && N95MaskDoffBool == true)
    //    {
    //        PPEDoff.Add("N95Mask");
    //        N95MaskDoffBool = false;
    //    }
    //    else if (EyeProtectionBool == true && EPDoffBool == true)
    //    {
    //        PPEDoff.Add("Eye Protection");
    //        EPDoffBool = false;
    //    }
    //    else if (HeadCoverBool == true && HeadCoverDoffBool == true)
    //    {
    //        PPEDoff.Add("Head Cover");
    //        HeadCoverDoffBool = false;
    //    }
    }
}
