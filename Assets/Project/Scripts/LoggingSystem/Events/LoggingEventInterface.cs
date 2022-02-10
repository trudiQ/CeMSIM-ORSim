using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    namespace Logger
    {
        /// <summary>
        /// This interface defines three functions that need to be implemented by any state that will be fed to the Logging system.
        /// </summary>
        public interface LoggingEventInterface
        {
            /// <summary>
            /// Serialize the event to a string. Used to add to a txt-based log file
            /// </summary>
            /// <returns></returns>
            string ToString();


            /// <summary>
            /// Format the instance to a csv record.
            /// </summary>
            /// <returns></returns>
            string ToCSV();

            /// <summary>
            /// Serialize the event to a json element. This function is the required by all states.
            /// </summary>
            /// <returns>A json string formualated using json functions provided in BaseEvent</returns>
            string ToJson();
            
        }
    }
}
