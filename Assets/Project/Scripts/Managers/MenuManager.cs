using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CEMSIM
{
    public class MenuManager : MonoBehaviour
    {
        #region Public Variables
        public GameObject mainCanvas;
        /*public MainMenu mainMenuPrefab;
        public SettingsMenu settingsMenuPrefab;
        public PauseMenu pauseMenuPrefab;
        public TrainingUIMenu trainingUIPrefab;
        public ConnectionsMenu connectionsMenuPrefab;
        */
        public GameObject Camera;
        #endregion

        #region Private Variables
        [SerializeField]
        private Transform _menuParent;
        Quaternion horizontalCanvas;   
        private Stack<Menu> _menuStack = new Stack<Menu>();
        #endregion

        private static MenuManager _instance;
        public static MenuManager Instance { get { return _instance; } }

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
                GameObject menuParentObject = new GameObject("Canvas");
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
            /*
            //Menu[] menuPrefabs = { mainMenuPrefab, settingsMenuPrefab, pauseMenuPrefab, trainingUIPrefab, connectionsMenuPrefab};
            Camera = GameObject.Find("Main Camera");
            foreach (Menu prefab in menuPrefabs)
            {
                if (prefab != null)
                {
                    Menu menuInstance = Instantiate(prefab, _menuParent);
                    //if (prefab != mainMenuPrefab)
                    {
                        menuInstance.gameObject.SetActive(false);
                    }
                    else
                    {
                        StartCoroutine(EndOfFrameMenu(menuInstance));
                        //OpenMenu(menuInstance);
                    }
                }
            }
            */
        }

        public void OpenMenu(Menu menuInstance)
        {
            if (menuInstance == null)
            {
                Debug.LogWarning("MENUMANAGER OpenMenu ERROR: invalid menu");
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
            //TODO: Find and set the Main Camera once per scene instead of here
            Camera = GameObject.Find("Main Camera");
            horizontalCanvas.eulerAngles = new Vector3(0, Camera.transform.rotation.eulerAngles.y, 0);
            menuInstance.gameObject.transform.position = Camera.transform.position + Vector3.ProjectOnPlane(Camera.transform.forward, Vector3.up) * 1.75f - Vector3.up*0.3f;
            menuInstance.gameObject.transform.rotation = horizontalCanvas;
            _menuStack.Push(menuInstance);
        }

        public void CloseMenu()
        {
            if (_menuStack.Count == 0)
            {
                Debug.LogWarning("MENUMANAGER CloseMenu ERROR: No menus in stack");
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

        IEnumerator EndOfFrameMenu(Menu _menuInstance)
        {
            yield return new WaitForEndOfFrame();
            OpenMenu(_menuInstance);
        }
    }
}
