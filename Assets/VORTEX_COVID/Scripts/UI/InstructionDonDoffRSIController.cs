using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionDonDoffRSIController : MonoBehaviour
{
    public InstructionUI[] instructionPanels;
    public AvatarSwapper avatarSwapper;

    private bool roleSelected;
    private bool donningComplete;
    private bool rsiComplete;
    
    void Start()
    {
        foreach (InstructionUI panel in instructionPanels)
        {
            panel.UpdateProcedure("RSI");
        }
    }

    private void AllPanelsDisplayNext()
    {
        foreach (InstructionUI panel in instructionPanels)
        {
            panel.DisplayNextText();
        }
    }

    public void RoleSelected(string role)
    {
        foreach (InstructionUI panel in instructionPanels)
        {
            panel.UpdateRole(role);
        }

        if (!roleSelected)
        {
            roleSelected = true;
            AllPanelsDisplayNext();
        }
    }

    public void RoleSelected(int roleIndex, int avatarIndex)
    {
        string role = avatarSwapper.avatarLists[roleIndex].name;

        foreach (InstructionUI panel in instructionPanels)
        {
            panel.UpdateRole(role);
        }

        if (!roleSelected)
        {
            roleSelected = true;
            AllPanelsDisplayNext();
        }
    }

    public void DonningComplete()
    {
        if (!donningComplete)
        {
            donningComplete = true;
            AllPanelsDisplayNext();
        }
    }
    
    public void RSIComplete()
    {
        if (!rsiComplete)
        {
            rsiComplete = true;
            AllPanelsDisplayNext();
        }
    }
}
