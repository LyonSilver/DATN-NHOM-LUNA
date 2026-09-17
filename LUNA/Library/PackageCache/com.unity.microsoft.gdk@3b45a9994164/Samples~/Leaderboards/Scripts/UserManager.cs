using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;

namespace GdkSample_LeaderboardSample
{
    public class UserManager
    {
        public enum UserOpResult
        {
            Success,
            NoDefaultUser,
            ResolveUserIssueRequired,
            UnclearedVetoes,

            UnknownError
        }

        private enum State
        {
            Initializing,
            GetContext,
            WaitForAddingUser,
            WaitForNextTask,
            Error,
            Idle,
            End
        }

        public struct UserData
        {
            public XUserHandle userHandle;
            public XUserLocalId m_localId;
            public ulong userXUID;
            public string userGamertag;
            public bool userIsGuest;
            public XblPermissionCheckResult canPlayMultiplayer;
            public ulong[] avoidList;
            public ulong[] muteList;
            public byte[] imageBuffer;
            public XblContextHandle m_context;
        }

        public delegate void AddUserCompletedDelegate(UserOpResult result);
        public List<UserData> UserDataList = new List<UserData>();
        public event EventHandler<XUserChangeEvent> UsersChanged;
        public LeaderboardSceneManager leaderboardSceneManager;

        private State m_State = State.Idle;
        private UserData m_CurrentUserData;
        private AddUserCompletedDelegate m_CurrentCompletionDelegate;
        private XUserChangeRegistrationToken m_CallbackRegistrationToken;

        // Constructor
        public UserManager()
        {
            // Register for the user change event with the GDK
            SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out m_CallbackRegistrationToken);
        }

        ~UserManager()
        {
            if (UserDataList != null)
            {
                foreach (UserData user in UserDataList)
                {
                    if (user.m_context != null)
                    {
                        SDK.XBL.XblContextCloseHandle(user.m_context);
                    }
                    if (user.userHandle != null)
                    {
                        SDK.XUserCloseHandle(user.userHandle);
                    }
                }
                UserDataList.Clear();
            }

            SDK.XUserUnregisterForChangeEvent(m_CallbackRegistrationToken);
        }

        public void Update()
        {
            switch (m_State)
            {
                case State.GetContext:
                    GetUserContext();
                    break;

                case State.Error:
                    m_CurrentCompletionDelegate(UserOpResult.UnknownError);
                    m_CurrentCompletionDelegate = null;
                    m_State = State.Idle;
                    break;

                case State.End:
                    UserDataList.Add(m_CurrentUserData);
                    m_CurrentCompletionDelegate(UserOpResult.Success);
                    m_CurrentCompletionDelegate = null;
                    m_State = State.Idle;
                    break;

                default:
                    break;
            }
        }

        //Adding User Silently
        public bool AddDefaultUserSilently(AddUserCompletedDelegate completionDelegate)
        {
            if (m_State != State.Idle)
            {
                // busy adding a user already
                return false;
            }
            m_State = State.WaitForAddingUser;
            m_CurrentUserData = new UserData();
            m_CurrentCompletionDelegate = completionDelegate;
            SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserSilently, (Int32 hResult, XUserHandle userHandle) =>
            {
                if (hResult == 0 && userHandle != null)
                {
                    Debug.Log("User added successfully.");

                    // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                    UserOpResult result = GetUserId(userHandle);
                    if (result == UserOpResult.ResolveUserIssueRequired)
                    {
                        ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
                    }
                    else if (result != UserOpResult.Success)
                    {
                        m_CurrentCompletionDelegate(result);
                    }
                    else
                        m_State = State.GetContext;
                }
                else if (hResult == HR.E_GAMEUSER_NO_DEFAULT_USER)
                {
                    Debug.Log($"WARNING: XUserAddAsync failed with hResult: 0x{hResult:x} ({HR.NameOf(hResult)}).");
                    m_State = State.Idle;
                    m_CurrentCompletionDelegate(UserOpResult.NoDefaultUser);
                }
                else
                {
                    Debug.Log($"WARNING: XUserAddAsync failed with hResult: 0x{hResult:x} ({HR.NameOf(hResult)}).");
                    m_State = State.Idle;
                    m_CurrentCompletionDelegate(UserOpResult.UnknownError);
                }
            });

            return true;
        }

        public bool AddUserWithUI(AddUserCompletedDelegate completionDelegate)
        {
            if (m_State != State.Idle)
            {
                // busy adding a user already
                return false;
            }

            m_State = State.WaitForAddingUser;
            m_CurrentUserData = new UserData();
            m_CurrentCompletionDelegate = completionDelegate;

            SDK.XUserAddAsync(XUserAddOptions.None, (Int32 hResult, XUserHandle userHandle) =>
            {
                if (hResult == 0 && userHandle != null)
                {
                    Debug.Log($"User logged in successfully.");

                    // Call XUserGetId here to ensure all vetos (privacy consent, gametag banned, etc) have passed
                    UserOpResult result = GetUserId(userHandle);
                    if (result == UserOpResult.ResolveUserIssueRequired)
                    {
                        ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
                    }
                    else if (result != UserOpResult.Success)
                    {
                        m_CurrentCompletionDelegate(result);
                    }
                    else
                        m_State = State.GetContext;
                }
                else if (userHandle != null)
                {
                    // Failed to log in, try to resolve issue
                    ResolveSigninIssueWithUI(userHandle, m_CurrentCompletionDelegate);
                }
                else
                {
                    Debug.Log($"FAILED: XUserAddAsync failed with hResult: 0x{hResult:x} ({HR.NameOf(hResult)}).");
                    m_State = State.Idle;
                    m_CurrentCompletionDelegate(UserOpResult.UnknownError);
                }
            });

            return true;
        }

        //Resolve sign in issue - lauches  UI to sign in users
        private void ResolveSigninIssueWithUI(XUserHandle userHandle, AddUserCompletedDelegate completionDelegate)
        {
            SDK.XUserResolveIssueWithUiUtf16Async(userHandle, null, (Int32 resolveHResult) =>
            {
                if (resolveHResult == 0)
                {
                    GetUserId(userHandle);
                    m_State = State.GetContext;
                }
                else
                {
                    // User has uncleared vetoes.  The game should decide how to handle this,
                    // either by gracefully continuing or dropping user back to title screen to
                    // with "Press 'A' or 'Enter' to continue" to select a new user.
                    completionDelegate(UserOpResult.UnclearedVetoes);
                    m_State = State.Idle;
                }
            });
        }

        // Get User ID
        private UserOpResult GetUserId(XUserHandle userHandle)
        {
            ulong xuid;
            int hr = SDK.XUserGetId(userHandle, out xuid);
            if (hr == 0)
            {
                m_CurrentUserData.userHandle = userHandle;
                m_CurrentUserData.userXUID = xuid;
                return UserOpResult.Success;
            }
            else if (hr == HR.E_GAMEUSER_RESOLVE_USER_ISSUE_REQUIRED)
            {
                return UserOpResult.ResolveUserIssueRequired;
            }

            return UserOpResult.UnknownError;
        }

        // Get the user's live context
        private void GetUserContext()
        {
            int hr = SDK.XBL.XblContextCreateHandle(m_CurrentUserData.userHandle, out m_CurrentUserData.m_context);
            if (hr == 0 && m_CurrentUserData.m_context != null)
            {
                Debug.Log("Success XBL and Context");
                m_State = State.End;
            }
            else
            {
                Debug.Log("Error creating context");
                m_State = State.Error;
            }
        }

        private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
        {
            if (eventType == XUserChangeEvent.SignedOut)
            {
                Debug.LogWarning("User logging out");

                foreach (UserData userData in UserDataList)
                    if (userData.m_localId.Value == userLocalId.Value)
                    {
                        // we already have this user
                        UserDataList.Remove(userData);
                        break;
                    }

                leaderboardSceneManager._UserSignedIn = false;
                leaderboardSceneManager.UpdateIntefaceButtons();
                leaderboardSceneManager.ClearLeaderboardData();
            }

            if (eventType != XUserChangeEvent.SignedInAgain)
            {
                EventHandler<XUserChangeEvent> handler = UsersChanged;
                handler?.Invoke(this, eventType);
            }
        }
    }
}
