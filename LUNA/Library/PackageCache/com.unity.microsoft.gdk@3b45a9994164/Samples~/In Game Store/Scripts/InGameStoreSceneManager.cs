using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GdkSample_InGameStore
{
    public class InGameStoreSceneManager : MonoBehaviour
    {
        public GameObject inGamePurchasableItemPrefab;
        public Transform canvasTransform;

        public Text output;

        public Text textGamertag;
        public Text textSandboxId;
        public Text textTitleId;
        public Text textScid;

        public Button showDLC;

        private XUserHandle _userHandle;
        private XblContextHandle _xblContextHandle;
        private XStoreContext _storeContext = null;
        private XUserChangeRegistrationToken _registrationToken;
        private List<XStoreProduct> _associatedProducts;
        private List<GameObject> _newInGamePurchasableItems = new List<GameObject>();
        private GetAssociatedProductsCallback _queryAssociatedProductsCallback;

        private const XStoreProductKind _addOnProducts = XStoreProductKind.Durable;
        private const int _MaxAssociatedProductsToRetrieve = 25;

        public delegate void GetAssociatedProductsCallback(Int32 hresult, List<XStoreProduct> associatedProducts);

        // Start is called before the first frame update
        private void Start()
        {
            showDLC.interactable = false;

            // Do initialization
            if (!GDKGameRuntime.TryInitialize())
            {
                return;
            }

            textSandboxId.text = GDKGameRuntime.GameConfigSandbox;
            textTitleId.text = string.Format($"0x{GDKGameRuntime.GameConfigTitleId}");
            textScid.text = GDKGameRuntime.GameConfigScid;

            InitializeAndAddUser();
            SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);
        }

        private void OnDestroy()
        {
            if (!GDKGameRuntime.Initialized)
            {
                return;
            }

            if (_xblContextHandle != null)
            {
                SDK.XBL.XblContextCloseHandle(_xblContextHandle);
                _xblContextHandle = null;
            }

            if (_userHandle != null)
            {
                SDK.XUserCloseHandle(_userHandle);
                _userHandle = null;
            }

            if (_storeContext != null)
            {
                SDK.XStoreCloseContextHandle(_storeContext);
                _storeContext = null;
            }

            SDK.XUserUnregisterForChangeEvent(_registrationToken);
        }

        private void InitializeAndAddUser()
        {
            SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
        }

        private void AddUserComplete(int hResult, XUserHandle userHandle)
        {
            if (HR.FAILED(hResult))
            {
                Debug.LogWarning($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            _userHandle = userHandle;

            CompletePostSignInInitialization();
        }

        private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
        {
            if (eventType == XUserChangeEvent.SignedOut)
            {
                foreach (var item in _newInGamePurchasableItems)
                {
                    GameObject.Destroy(item);
                }
                _newInGamePurchasableItems.Clear();
                output.text = "";

                Debug.LogWarning("User logging out");
                textGamertag.text = "User logged out";

                if (_xblContextHandle != null)
                {
                    SDK.XBL.XblContextCloseHandle(_xblContextHandle);
                    _xblContextHandle = null;
                }

                if (_userHandle != null)
                {
                    SDK.XUserCloseHandle(_userHandle);
                    _userHandle = null;
                }

                if (_storeContext != null)
                {
                    SDK.XStoreCloseContextHandle(_storeContext);
                    _storeContext = null;
                }

                InitializeAndAddUser();
            }
        }

        private void CompletePostSignInInitialization()
        {
            string gamertag = string.Empty;

            if (textGamertag == null)
            {
                Debug.LogError($"textGamertag is null, set Game Object.");
                return;
            }

            int hResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out gamertag);
            if (HR.FAILED(hResult))
            {
                Debug.LogWarning($"FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            Debug.Log($"SUCCESS: XUserGetGamertag() returned: '{gamertag}'");
            textGamertag.text = gamertag;

            hResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
            if (HR.FAILED(hResult))
            {
                Debug.LogError($"FAILED: Could not create context handle, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            }

#if UNITY_GAMECORE && !UNITY_EDITOR
            hResult = SDK.XStoreCreateContext(_userHandle, out _storeContext);
#else
            // We do not need to pass a user on standalone PC as the currently signed in store user will be used.
            hResult = SDK.XStoreCreateContext(out _storeContext);
#endif
            if (HR.FAILED(hResult))
            {
                Debug.Log($"FAILED: Store create context, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            SDK.XStoreQueryGameAndDlcPackageUpdatesAsync(_storeContext, HandleQueryForUpdatesComplete);

            FocusUIShowDLC();
        }

        public void OnClicked_ShowDLC()
        {
            // Get the list of In game purchasable items.
            GetAssociatedProductsAsync(GetAssociatedProductsAsyncCallback);
            showDLC.interactable = false;
        }

        private void HandleQueryForUpdatesComplete(int hResult, XStorePackageUpdate[] packageUpdates)
        {
            List<string> _packageIdsToUpdate = new List<string>();
            if (hResult >= 0)
            {
                if (packageUpdates != null &&
                    packageUpdates.Length > 0)
                {
                    foreach (XStorePackageUpdate packageUpdate in packageUpdates)
                    {
                        _packageIdsToUpdate.Add(packageUpdate.PackageIdentifier);
                    }

                    // What do we do?
                    SDK.XStoreDownloadAndInstallPackageUpdatesAsync(
                        _storeContext,
                        _packageIdsToUpdate.ToArray(),
                        DownloadFinishedCallback);
                }
            }
            else
            {
                // No-op
            }
        }

        private void DownloadFinishedCallback(int hResult)
        {
            if (HR.FAILED(hResult))
            {
                Debug.Log($"FAILED: Store download and install, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
                return;
            }

            Debug.Log("SUCCESS: DownloadAndInstallPackageUpdates callback");
        }

        public void GetAssociatedProductsAsyncCallback(Int32 hresult, List<XStoreProduct> associatedProducts)
        {
            if (HR.FAILED(hresult))
            {
                output.text = "hResult: 0x" + hresult.ToString("X");
                return;
            }

            output.text = "Count: " + associatedProducts.Count.ToString();

            foreach (var item in _newInGamePurchasableItems)
            {
                Destroy(item);
            }
            _newInGamePurchasableItems.Clear();

            for (int i = 0; i < associatedProducts.Count; i++)
            {
                XStoreProduct product = associatedProducts[i];
                GameObject newInGamePurchasableItemPrefab = Instantiate(inGamePurchasableItemPrefab);

                newInGamePurchasableItemPrefab.transform.SetParent(canvasTransform, false);
                newInGamePurchasableItemPrefab.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(((i % 5) * 100) - 300, (-80 * Mathf.Floor(i / 5) + 130));

                SampleInGamePurchasableItem sampleInGamePurchasableItemScript =
                    newInGamePurchasableItemPrefab.GetComponent<SampleInGamePurchasableItem>();

                sampleInGamePurchasableItemScript.inGameStoreSceneManager = this;
                sampleInGamePurchasableItemScript.UpdateUI(product, _storeContext);

                _newInGamePurchasableItems.Add(newInGamePurchasableItemPrefab);
            }
        }

        public void GetAssociatedProductsAsync(GetAssociatedProductsCallback callback)
        {
            if (callback == null)
            {
                Debug.LogError("Callback cannot be null.");
            }

            _associatedProducts = new List<XStoreProduct>();

            _queryAssociatedProductsCallback = callback;

            SDK.XStoreQueryAssociatedProductsAsync(
                _storeContext,
                _addOnProducts,
                _MaxAssociatedProductsToRetrieve,
                ProcessAssociatedProductsResults
            );

            FocusUIShowDLC();
        }

        private void ProcessAssociatedProductsResults(Int32 hResult, XStoreQueryResult result)
        {
            FocusUIShowDLC();

            if (HR.FAILED(hResult))
            {
                Debug.Log($"FAILED: Process associated products results, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");

                if (_queryAssociatedProductsCallback != null)
                {
                    _queryAssociatedProductsCallback(hResult, _associatedProducts);
                }

                return;
            }

            _associatedProducts.AddRange(result.PageItems);

            if (result.HasMorePages)
            {
                SDK.XStoreProductsQueryNextPageAsync(result, ProcessAssociatedProductsResults);
            }
            else
            {
                if (_queryAssociatedProductsCallback != null)
                {
                    _queryAssociatedProductsCallback(hResult, _associatedProducts);
                }
            }
        }

        public void FocusUIShowDLC()
        {
            showDLC.interactable = true;
            EventSystem.current.SetSelectedGameObject(showDLC.gameObject);
        }
    }
} // namespace GdkSample_InGameStore
