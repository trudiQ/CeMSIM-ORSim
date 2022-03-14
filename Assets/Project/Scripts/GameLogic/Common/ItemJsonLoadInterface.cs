using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CEMSIM
{
    namespace GameLogic
    {
        interface ItemJsonLoadInterface
        {
            void DigestJsonObject(JObject jObject);
        }
    }
}