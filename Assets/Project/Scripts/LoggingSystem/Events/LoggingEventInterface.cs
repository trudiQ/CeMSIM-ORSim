using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        public interface LoggingEventInterface
        {
            /// <summary>
            /// Serialize the event to a string. Used to add to log file
            /// </summary>
            /// <returns></returns>
            string ToString();


            /// <summary>
            /// Format the instance to a csv record. 
            /// </summary>
            /// <returns></returns>
            string ToCSV();

            /// <summary>
            /// Serialize the event to a json element.
            /// </summary>
            /// <returns></returns>
            string ToJson();
            
        }
    }
}
