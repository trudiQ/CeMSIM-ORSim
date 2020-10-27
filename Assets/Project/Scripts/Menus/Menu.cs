using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    public abstract class Menu<T> : Menu where T : Menu<T>
    {
        private static T _instance;
        public static T Instance { get { return _instance; } }

        protected virtual void Awake()
        {
            if (_instance = null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = (T)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }

    //[RequireComponent(typeof(Canvas))]
    public abstract class Menu : MonoBehaviour
    {
        #region Private Variables
        /*private const float _distance = 1.0f;
        private GameObject _camera;        
        private Quaternion horizontalCanvas;
        private float slerpSpeed = 4.0f;
        protected bool slerp = false;
        private bool setPos = true;
        private Vector3 posTo;
        private Quaternion rotTo;
        */
        #endregion

        public virtual void OnBackPressed()
        {
            ServerMenuManager.Instance.CloseMenu();
        }

    }
}