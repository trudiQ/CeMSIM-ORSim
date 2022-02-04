using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CEMSIM;

public class InstructionDonDoffRSIController : MonoBehaviour
{
    public InstructionUI[] instructionPanels;
    public AvatarSwapper avatarSwapper;
    public float avatarPopupDuration = 5f;

    private InstructionUI avatarInstructionPanel;

    private bool roleSelected;
    private bool donningComplete;
    private bool rsiComplete;
    
    void Start()
    {
        foreach (InstructionUI panel in instructionPanels)
            panel.UpdateProcedure("RSI");
    }

    // Update the role of each panel and display the next message
    public void RoleSelected(string role)
    {
        foreach (InstructionUI panel in instructionPanels)
            panel.UpdateRole(role);

        avatarInstructionPanel?.UpdateRole(role);

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
            panel.UpdateRole(role);

        avatarInstructionPanel?.UpdateRole(role);

        if (!roleSelected)
        {
            roleSelected = true;
            AllPanelsDisplayNext();
        }
    }

    // Mark donning as complete and display next panel
    public void DonningComplete()
    {
        if (!donningComplete)
        {
            donningComplete = true;
            AllPanelsDisplayNext();
        }
    }

    // Mark RSI as complete and display next panel
    public void RSIComplete()
    {
        if (!rsiComplete)
        {
            rsiComplete = true;
            AllPanelsDisplayNext();
        }
    }

    // Get the instruction panel from the new avatar
    public void GetAvatarPanel()
    {
        avatarInstructionPanel = avatarSwapper.gameObject.GetComponentInChildren<InstructionUI>();
        avatarInstructionPanel?.UpdateProcedure("RSI");
    }

    private void AllPanelsDisplayNext()
    {
        foreach (InstructionUI panel in instructionPanels)
            panel.TransitionToNextInstruction();

        avatarInstructionPanel?.TransitionToNextInstruction();  // Change text before opening
        avatarInstructionPanel?.Open();
    }
}
