using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;

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
            panel.TransitionToNextInstruction();
        }
    }

    // Update the role of each panel and display the next message
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

    // Update the role of each panel and display the next message
    public void RoleSelected(int roleIndex, int avatarIndex)
    {
        if (!roleSelected && roleIndex == avatarSwapper.defaultRole && avatarIndex == avatarSwapper.defaultAvatar)
            return;

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
