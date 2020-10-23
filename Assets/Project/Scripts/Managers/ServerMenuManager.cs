using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    public class ServerMenuManager : MonoBehaviour
    {
        #region Public Variables
        [Header("Menus")]
        public NetworkOverlayMenu networkOverlayMenuPrefab;

        [Header("References")]
        public GameObject Camera;
        #endregion

        #region Private Variables
        [SerializeField]
        private Transform _menuParent;
        Quaternion horizontalCanvas;   
        private Stack<Menu> _menuStack = new Stack<Menu>();
        #endregion

        private static ServerMenuManager _instance;
        public static ServerMenuManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance = null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                InitializaeMenus();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void InitializaeMenus()
        {
            if (_menuParent == null)
            {
                GameObject menuParentObject = new GameObject("[MENUS]");
                _menuParent = menuParentObject.transform;
            }

            /*
            if(mainCanvas !=null)
            {
                _menuParent = mainCanvas.transform;
            }
            else
            {
                Debug.Log("Error, MenuManager: No Main Canvas Set");
            }
            */
            
            Menu[] menuPrefabs = { networkOverlayMenuPrefab};
            foreach (Menu prefab in menuPrefabs)
            {
                if (prefab != null)
                {
                    Menu menuInstance = Instantiate(prefab, _menuParent);
                    if (prefab != networkOverlayMenuPrefab)
                    {
                        menuInstance.gameObject.SetActive(false);
                    }
                    else
                    {
                        OpenMenu(menuInstance);
                    }
                }
            }
        }

        public void OpenMenu(Menu menuInstance)
        {
            if (menuInstance == null)
            {
                Debug.LogWarning("MENU MANAGER OpenMenu ERROR: invalid menu");
                return;
            }

            if (_menuStack.Count > 0)
            {
                foreach (Menu menu in _menuStack)
                {
                    menu.gameObject.SetActive(false);
                }
            }

            menuInstance.gameObject.SetActive(true);
            _menuStack.Push(menuInstance);
        }

        public void CloseMenu()
        {
            if (_menuStack.Count == 0)
            {
                Debug.LogWarning("SERVER MENU MANAGER CloseMenu ERROR: No menus in stack");
                return;
            }

            Menu topMenu = _menuStack.Pop();
            topMenu.gameObject.SetActive(false);

            if (_menuStack.Count > 0)
            {
                Menu nextMenu = _menuStack.Peek();
                nextMenu.gameObject.SetActive(true);
            }
        }
    }
}
