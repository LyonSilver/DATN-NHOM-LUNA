// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.UI;

namespace GdkSample_InGameStore
{
    public class SampleInGamePurchasableItem : MonoBehaviour
    {
        public Text titleUIElement;
        public Text priceUIElement;
        public Text ownedUIElement;

        public InGameStoreSceneManager inGameStoreSceneManager;

        private XStoreProduct _storeProduct;
        private XStoreContext _storeContext = null;

        public delegate void ShowPurchaseUICallback(int hresult, XStoreProduct storeProduct);

        public void UpdateUI(XStoreProduct product, XStoreContext storeContext)
        {
            _storeProduct = product;

            titleUIElement.text = _storeProduct.Title;
            priceUIElement.text = _storeProduct.Price.FormattedPrice;
            ownedUIElement.text = _storeProduct.IsInUserCollection ? "Owned" : "Not owned";

            _storeContext = storeContext;
        }

        public void Purchase()
        {
            ShowPurchaseUIAsync(_storeProduct, ShowPurchaseCallback);
        }

        public void ShowPurchaseUIAsync(XStoreProduct storeProduct, ShowPurchaseUICallback callback)
        {
            SDK.XStoreShowPurchaseUIAsync(
                _storeContext,
                storeProduct.StoreId,
                null,
                null,
                (int hresult) =>
                {
                    callback(hresult, storeProduct);
                });
        }

        public void ShowPurchaseCallback(int hresult, XStoreProduct product)
        {
            if (hresult >= 0)
            {
                inGameStoreSceneManager.GetAssociatedProductsAsync(inGameStoreSceneManager.GetAssociatedProductsAsyncCallback);
                Debug.Log("Purchased " + product.Title);
            }
            else
            {
                Debug.Log("Purchased failed. Error: " + hresult);
            }

            inGameStoreSceneManager.FocusUIShowDLC();
        }
    }
} // namespace GdkSample_InGameStore
