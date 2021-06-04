using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.HurricaneVR.Framework.Shared.Utilities;
using UnityEngine;

namespace HurricaneVR.Framework.SteamVR
{
    public class HVRSteamInitializer : MonoBehaviour
    {
        private void Start()
        {
            this.ExecuteNextUpdate(Initialize);
        }

        private void Initialize()
        {
            Valve.VR.SteamVR.Initialize();
        }
    }
}
