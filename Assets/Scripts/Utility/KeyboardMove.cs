using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class KeyboardMove : MonoBehaviour
    {
        public KeyCode forwardKey;
        public KeyCode backwardKey;
        public KeyCode leftKey;
        public KeyCode rightKey;
        public KeyCode upKey;
        public KeyCode downKey;

        public float speed = 1f;

        public KeyCode speedModKey;
        public float speedModifier = 2f;

        int KeysToAxis(KeyCode forward, KeyCode backward)
        {
            return (Input.GetKey(forward) ? 1 : 0) + (Input.GetKey(backward) ? -1 : 0);
        }

        // Update is called once per frame
        void Update()
        {
            float xMove = KeysToAxis(rightKey, leftKey);
            float yMove = KeysToAxis(upKey, downKey);
            float zMove = KeysToAxis(forwardKey, backwardKey);

            transform.Translate(
                new Vector3(xMove, yMove, zMove) *
                Time.deltaTime *
                (Input.GetKey(speedModKey) ? speedModifier * speed : speed));
        }
    }
}
