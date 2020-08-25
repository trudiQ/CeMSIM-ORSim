# CeMSIM-ORSim
A starting point for all CeMSIM Unity projects.

## Table of Contents


## General Info

## Setup
1) Clone repository and checkout the desired branch.
2) Open project
(For anyone running a beta build before the August 2020 Release must do steps 3-5, fresh installs skip to step 6)
3)Go into Package Manager
4) Select XR Plugin Management and click 'Remove'. If youreceive a notification that there are dependencies (i.e. Oculus XR Plugin), Remove those packages first.
5) Close Unity and reopen the project. Don't worry about errors in the Console window.
6) Download and import the unity package OpenVR-XR-Plugin-Installer: https://github.com/ValveSoftware/unity-xr-plugin/releases/tag/installer
7) Go to Project Settings --> XR Plugin Management, press 'Install XR Plugin Management'. Note: Do not select any Plugins yet, there is a bug that will import Oculus.XR package in an infinite loop further in the setup that will require uninstalling XR Plugin Management.
8) Select 'Initialize XR on Startup'.

## Running CeMSIM-ORSim

### Controls
LH Trigger Button- Teleport
RH Grip Button - Grab

### Interactions
Light switch - Collider