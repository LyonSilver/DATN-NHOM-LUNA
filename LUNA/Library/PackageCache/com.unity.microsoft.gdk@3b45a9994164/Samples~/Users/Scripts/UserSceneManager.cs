using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_Users
{
    public class UserSceneManager : MonoBehaviour
    {
        public class UserUIData
        {
            public XUserHandle userObject;
            public GameObject gameObject;
            public Transform objTransform;
            public RawImage UserImage;
            public Text UserDetails;
            public Text UserPermissions;
            public Button ButtonMoreInfo;
            public Text MuteList;
            public Text AvoidList;
            public Text Number;
            public bool done;
        }

        public GameObject UserDash;
        public List<GameObject> UserUIDashGO = new List<GameObject>();
        public ScrollRect UserUIScrollRect;

        [NonSerialized]
        public List<UserUIData> UserUIDataList = new List<UserUIData>();
        public Text AboutSceneText;
        public Text TextSandboxId;
        public Text TextScid;
        private UserManager m_UserManager;

        public void Start()
        {
            AboutSceneText.text = k_AboutSceneText;

            if (!GDKGameRuntime.TryInitialize())
            {
                return;
            }

            TextSandboxId.text = GDKGameRuntime.GameConfigSandbox;
            TextScid.text = GDKGameRuntime.GameConfigScid;

            if (m_UserManager == null)
            {
                m_UserManager = new UserManager();
            }

            m_UserManager.UsersChanged += UserManager_UsersChanged;
        }

        public void Update()
        {
            if (m_UserManager != null)
                m_UserManager.Update();

            if (m_UsersChanged)
            {
                m_UsersChanged = false;
                int numUsers = m_UserManager.UserDataList.Count;
                for (int i = 0; i < numUsers; ++i)
                {
                    UserManager.UserData currentUserData = m_UserManager.UserDataList[i];
                    if (i > UserUIDataList.Count - 1)
                    {
                        UserUIData newUserData = new UserUIData();
                        UserUIDataList.Add(newUserData);
                        GameObject tempObject = Instantiate(UserDash);
                        UserUIDashGO.Add(tempObject);
                        tempObject.transform.SetParent(UserUIScrollRect.content.gameObject.transform, false);
                        tempObject.SetActive(true);

                        UserUIDataList[i].gameObject = tempObject;

                        CreateUserDash(UserUIDataList[i], currentUserData);

                        UserUIDataList[i].Number.text = i.ToString();

                        UpdateUserDash(UserUIDataList[i], currentUserData);
                    }
                    else if (UserUIDataList[i].userObject != currentUserData.userHandle)
                        UpdateUserDash(UserUIDataList[i], currentUserData);
                }

                if (UserUIDataList.Count > numUsers)
                {
                    // remove the extra elements in the list if any
                    for (int i = UserUIDataList.Count - 1; i >= numUsers; --i)
                    {
                        UserUIDataList[i].gameObject.SetActive(false);
                        UserUIDashGO.Remove(UserUIDataList[i].gameObject);
                        Destroy(UserUIDataList[i].gameObject);
                        UserUIDataList.RemoveAt(i);
                    }
                }

                UserUIScrollRect.verticalNormalizedPosition = 0;
            }
        }

        public void AddUserWithUI()
        {
            // We attempt to add the first user as the default one, the others need to be explicitely selected
            if (m_UserManager.UserDataList.Count == 0)
                m_UserManager.AddDefaultUserSilently(AddUserCompleted);
            else
                m_UserManager.AddUserWithUI(AddUserCompleted);
        }

        private const string k_AboutSceneText =
            "This demo demonstrates how to use the XUser* Game Core APIs.\nSelect the Login User button to get started.";

        private bool m_UsersChanged;

        private void UserManager_UsersChanged(object sender, XUserChangeEvent e)
        {
            m_UsersChanged = true;
        }

        private void AddUserCompleted(UserManager.UserOpResult result)
        {
            switch (result)
            {
                case UserManager.UserOpResult.Success:
                    {
                        m_UsersChanged = true;
                        break;
                    }
                case UserManager.UserOpResult.NoDefaultUser:
                    {
                        m_UserManager.AddUserWithUI(AddUserCompleted);
                        break;
                    }
                case UserManager.UserOpResult.UnknownError:
                    {
                        Debug.Log("Error adding user.");
                        break;
                    }
                default:
                    break;
            }
        }

        private void CreateUserDash(UserUIData useruiData, UserManager.UserData userData)
        {
            Debug.Log("Displaying UI with information for User " + userData.userXUID);

            useruiData.objTransform = useruiData.gameObject.transform;
            useruiData.UserImage = useruiData.gameObject.GetComponent<UserAttributes>().UserImage;
            useruiData.UserDetails = useruiData.gameObject.GetComponent<UserAttributes>().UserDetails;
            useruiData.UserPermissions = useruiData.gameObject.GetComponent<UserAttributes>().UserPermissions;
            useruiData.ButtonMoreInfo = useruiData.gameObject.GetComponent<UserAttributes>().ButtonMoreInfo;
            useruiData.MuteList = useruiData.gameObject.GetComponent<UserAttributes>().MuteList;
            useruiData.AvoidList = useruiData.gameObject.GetComponent<UserAttributes>().AvoidList;
            useruiData.Number = useruiData.gameObject.GetComponent<UserAttributes>().Number;
            useruiData.userObject = userData.userHandle;
        }

        private void UpdateUserDash(UserUIData currentUserInformation, UserManager.UserData currentUserData)
        {
            currentUserInformation.UserDetails.text =
                "GamerTag: \n" + currentUserData.userGamertag + "\n\n" +
                "XUID: \n" + currentUserData.userXUID + "\n\n" +
                "Is Guest: \n" + currentUserData.userIsGuest;

            string avoidListUIText = "Avoid Total: ";
            string muteListUIText = "Mute Total: ";

            if (currentUserData.avoidList != null)
            {
                avoidListUIText = avoidListUIText + currentUserData.avoidList.Length;

                foreach (var item in currentUserData.avoidList)
                {
                    avoidListUIText = avoidListUIText + "\n" + item;
                }
            }
            else
            {
                avoidListUIText = avoidListUIText + " 0";
            }

            if (currentUserData.muteList != null)
            {
                muteListUIText = muteListUIText + currentUserData.muteList.Length;

                foreach (var item in currentUserData.muteList)
                {
                    muteListUIText = muteListUIText + "\n" + item;
                }
            }
            else
            {
                muteListUIText = muteListUIText + " 0";
            }

            currentUserInformation.AvoidList.text = avoidListUIText;
            currentUserInformation.MuteList.text = muteListUIText;

            string permissionssUIText =
                "Name: Can Play Multiplayer \nState: " + currentUserData.canPlayMultiplayer.IsAllowed;
            if (currentUserData.canPlayMultiplayer.Reasons.Length > 0)
            {
                for (int j = 0; j < currentUserData.canPlayMultiplayer.Reasons.Length; j++)
                {
                    permissionssUIText = permissionssUIText + "\nState: " +
                                         currentUserData.canPlayMultiplayer.Reasons[0].Reason;
                }
            }

            currentUserInformation.UserPermissions.text = permissionssUIText;

            if (currentUserInformation.UserImage.texture != null)
            {
                Texture2D myTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                myTexture.filterMode = FilterMode.Point;
                myTexture.LoadImage(currentUserData.imageBuffer);
                currentUserInformation.UserImage.texture = myTexture;
            }
        }
    }
} // namespace GdkSample_Users
