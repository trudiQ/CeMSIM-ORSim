using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.UI;
using UnityEditor;
using UnityEngine.Events;
using CEMSIM.GameLogic;
using CEMSIM.Network;

namespace CEMSIM
{
    [RequireComponent(typeof(UserHeightUtility))]
    public class AvatarSwapper : MonoBehaviour
    {
        public bool isSinglePlayer = false;
        [Header("Components")]
        public HVRManager manager;
        public HVRInputModule uiInputModule;
        public UserHeightUtility userHeightUtility;
        public AvatarNetworkedComponents avatarNetworkedComponents;

        [Header("Avatars")]
        public List<RoleAvatarList> avatarLists;
        public int defaultRole = 0;
        public int defaultAvatar = 0;
        public int selectedRole { get; private set; }
        //public CEMSIM.GameLogic.Roles selectedRole { get; private set; }
        public int selectedAvatar { get; private set; }
        public int activeRole { get; private set; }
        //public CEMSIM.GameLogic.Roles activeRole;
        public int activeAvatar { get; private set; }
        public GameObject spawnedAvatar { get; private set; }

        private AvatarComponents currentAvatarComponents;

        //public UnityEvent<CEMSIM.GameLogic.Roles, int> OnAvatarSwapped;
        public UnityEvent<int, int> OnAvatarSwapped;

        void Awake()
        {
            if (!userHeightUtility)
                userHeightUtility = GetComponent<UserHeightUtility>();

            if (!avatarNetworkedComponents)
                avatarNetworkedComponents = GetComponent<AvatarNetworkedComponents>();

            selectedRole = defaultRole;
            selectedAvatar = defaultAvatar;
        }

        private void Start()
        {
            SwapToSelectedAvatar();
        }

        public void ChooseRole(int index)
        {
            // selectedRole = (CEMSIM.GameLogic.Roles)index;
            selectedRole = index;
        }

        public void ChooseAvatar(int index)
        {
            selectedAvatar = index;
        }

        // Create the chosen avatar and remove the current avatar
        public void SwapAvatar(int roleIndex, int avatarIndex)
        {
            if (roleIndex >= 0 && roleIndex < avatarLists.Count)
            {
                if (avatarIndex >= 0 && avatarIndex < avatarLists[roleIndex].avatars.Length)
                {
                    currentAvatarComponents?.RemoveComponents();
                    Destroy(spawnedAvatar);

                    activeRole = roleIndex;
                    activeAvatar = avatarIndex;

                    GameObject newPrefab = avatarLists[roleIndex].avatars[avatarIndex].avatarPrefab;

                    spawnedAvatar = Instantiate(original: newPrefab,
                                                parent: gameObject.transform,
                                                position: transform.position,
                                                rotation: transform.rotation);

                    currentAvatarComponents = spawnedAvatar.GetComponent<AvatarComponents>();
                    avatarNetworkedComponents?.DeepCopy(spawnedAvatar.GetComponent<AvatarNetworkedComponents>());
                    currentAvatarComponents.SetHVRComponents(manager, uiInputModule);
                    currentAvatarComponents.SetUserHeightUtility(userHeightUtility);

                    if (!isSinglePlayer)
                    {
                        // modify the prefab to spawn
                        GameManager.instance.localPlayerVR = spawnedAvatar;
                        ClientInstance.instance.role = (Roles)roleIndex;
                        ClientInstance.instance.avatar_id = avatarIndex;
                    }

                    userHeightUtility.floor = currentAvatarComponents.floor;
                    userHeightUtility.camera = currentAvatarComponents.camera;
                    userHeightUtility.cameraRig = currentAvatarComponents.cameraRig;
                    userHeightUtility.avatarHeightCalibration = currentAvatarComponents.calibration;

                    OnAvatarSwapped.Invoke(activeRole, activeAvatar);
                }
                else
                    Debug.Log("Chosen avatar index out of bounds.");
            }
            else
                Debug.LogError("Chosen role index out of bounds.");
        }

        // Method to be called from UI elements without parameters
        public void SwapToSelectedAvatar()
        {
            SwapAvatar(selectedRole, selectedAvatar);
        }
    }

    [CustomEditor(typeof(AvatarSwapper))]
    public class AvatarSwapperEditor : Editor
    {
        private int roleIndex;
        private int avatarIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
            {
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

                roleIndex = EditorGUILayout.IntField(label: "Role Index", value: roleIndex);
                avatarIndex = EditorGUILayout.IntField(label: "Avatar Index", value: avatarIndex);

                if (GUILayout.Button("Swap"))
                    (target as AvatarSwapper).SwapAvatar(roleIndex, avatarIndex);
            }

        }
    }
}
