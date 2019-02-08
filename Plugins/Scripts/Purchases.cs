using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using System.Collections.Generic;

#pragma warning disable CS0649

public class Purchases : MonoBehaviour
{
    public abstract class Listener : MonoBehaviour
    {
        public abstract void ProductsReceived(List<Product> products);
        public abstract void ProductsReceiveFailed(Error error);

        public abstract void PurchaseSucceeded(string productIdentifier, PurchaserInfo purchaserInfo);
        public abstract void PurchaseFailed(string productIdentifier, Error error, bool userCanceled);

        public abstract void PurchaserInfoReceived(PurchaserInfo purchaserInfo);
        public abstract void PurchaserInfoReceiveFailed(Error error);

        public abstract void EntitlementsReceived(Dictionary<String, Entitlement> entitlements);
        public abstract void EntitlementsReceiveFailed(Error error);
    }

    private class PurchasesWrapperNoop : PurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserID)
        {
            
        }

        public void AddAttributionData(string network, string data)
        {
			
        }

        public void GetProducts(string[] productIdentifiers, string type = "subs")
        {
            
        }

        public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
        {
            
        }

        public void RestoreTransactions()
        {
            
        }

        public void CreateAlias(string newAppUserID)
        {

        }

        public void Identify(string appUserID)
        {

        }

        public void Reset()
        {

        }

        public void SetAllowSharingStoreAccount(bool allow)
        {

        }

        public void GetAppUserID()
        {

        }

        public void SetDebugLogsEnabled(bool enabled)
        {

        }

        public void GetPurchaserInfo()
        {

        }

        public void GetEntitlements()
        {

        }
    }

    /*
     * PurchaserInfo encapsulate the current status of subscriber. 
     * Use it to determine which entitlements to unlock, typically by checking 
     * ActiveSubscriptions or via LatestExpirationDate. 
     * 
     * Note: All DateTimes are in UTC, be sure to compare them with 
     * DateTime.UtcNow
     */
    public class PurchaserInfo
    {
        private PurchaserInfoResponse response;

        public PurchaserInfo(PurchaserInfoResponse response)
        {
            this.response = response;
        }


        public List<string> ActiveSubscriptions
        {
            get { return response.activeSubscriptions; }
        }

        public List<string> AllPurchasedProductIdentifiers
        {
            get { return response.allPurchasedProductIdentifiers; }
        }

        public DateTime LatestExpirationDate
        {
            get { return FromUnixTime(response.latestExpirationDate); }
        }

        private static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public Dictionary<string, DateTime> AllExpirationDates
        {
            get
            {
                Dictionary<string, DateTime> allExpirations = new Dictionary<string, DateTime>();
                for (int i = 0; i < response.allExpirationDateKeys.Count; i++)
                {
                    var date = FromUnixTime(response.allExpirationDateValues[i]);
                    if (date != null)
                    {
                        allExpirations[response.allExpirationDateKeys[i]] = date;
                    }
                }

                return allExpirations;
            }
        }
    }

    [Serializable]
    public class Error
    {
        public string message;
        public int code;
        public string domain;
    }

    [Serializable]
    public class Product
    {
        public string title;
        public string identifier;
        public string description;
        public float price;
        public string priceString;
    }

    [Serializable]
    public class Entitlement
    {
        public Dictionary<String, Product> offerings;
    }

    [Tooltip("Your RevenueCat API Key. Get from https://app.revenuecat.com/")]
    public string revenueCatAPIKey;

    [Tooltip(
        "App user id. Pass in your own ID if your app has accounts. If blank, RevenueCat will generate a user ID for you.")]
    public string appUserID;

    [Tooltip("List of product identifiers.")]
    public string[] productIdentifiers;

    [Tooltip("A subclass of Purchases.Listener component. Use your custom subclass to define how to handle events.")]
    public Listener listener;

    private PurchasesWrapper wrapper;

    void Start()
    {
        string appUserID = (string.IsNullOrEmpty(this.appUserID)) ? null : this.appUserID;

#if UNITY_ANDROID && !UNITY_EDITOR
        wrapper = new PurchasesWrapperAndroid();
#elif UNITY_IPHONE && !UNITY_EDITOR
        wrapper = new PurchasesWrapperiOS();
#else
        wrapper = new PurchasesWrapperNoop();
#endif

        Setup(appUserID);
        GetProducts(productIdentifiers);
    }

    // Call this if you want to reset with a new user id
    public void Setup(string newUserID)
    {
        wrapper.Setup(gameObject.name, revenueCatAPIKey, newUserID);
    }

    // Optionally call this if you want to fetch more products, 
    // called automatically with pre-configured products
    public void GetProducts(string[] products)
    {
        wrapper.GetProducts(products);
    }

    // Call this to initiate a purchase
    public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
    {
        wrapper.MakePurchase(productIdentifier, type, oldSku);
    }

    public void RestoreTransactions()
    {
        wrapper.RestoreTransactions();
    }

    [Serializable]
    public class AdjustData
    {
        public string adid;
        public string network;
        public string adgroup;
        public string campaign;
        public string creative;
        public string clickLabel;
        public string trackerName;
        public string trackerToken;
    }

    public void AddAdjustAttributionData(AdjustData data)
    {
        wrapper.AddAttributionData("adjust", JsonUtility.ToJson(data));
    }

    public void CreateAlias(string newAppUserID)
    {
        wrapper.CreateAlias(newAppUserID);
    }

    public void Identify(string appUserID)
    {
        wrapper.Identify(appUserID);
    }

    public void Reset()
    {
        wrapper.Reset();
    }

    public void SetAllowSharingStoreAccount(bool allow)
    {
        wrapper.SetAllowSharingStoreAccount(allow);
    }

    public void SetDebugLogsEnabled(bool enabled)
    {
        wrapper.SetDebugLogsEnabled(enabled);
    }

    public void GetPurchaserInfo()
    {
        wrapper.GetPurchaserInfo();
    }

    public void GetEntitlements()
    {
        wrapper.GetEntitlements();
    }

    [Serializable]
    private class ReceiveProductResponse
    {
        public ProductResponse productResponse;
        public Error error;
    }

    [Serializable]
    private class ProductResponse
    {
        public List<Product> products;
        public Error error;
    }

    [Serializable]
    private class ReceivePurchaserInfoResponse
    {
        public PurchaserInfoResponse purchaserInfo;
        public Error error;
    }

    [Serializable]
    private class MakePurchaseResponse
    {
        public string productIdentifier;
        public PurchaserInfoResponse purchaserInfo;
        public Error error;
    }

    [Serializable]
    public class PurchaserInfoResponse
    {
        public List<string> activeSubscriptions;
        public List<string> allPurchasedProductIdentifiers;
        public long latestExpirationDate;
        public List<string> allExpirationDateKeys;
        public List<long> allExpirationDateValues;
    }

    [Serializable]
    public class EntitlementsResponse
    {
        public Dictionary<String, Entitlement> entitlements;
        public Error error;
    }

    private void _receiveProducts(string productsJSON)
    {
        Debug.Log(productsJSON);
        var response = JsonUtility.FromJson<ProductResponse>(productsJSON);
        var error = (response.error.message != null) ? response.error : null;

        if (error != null)
        {
            listener.ProductsReceiveFailed(error);
        }
        else
        {
            listener.ProductsReceived(response.products);
        }
    }

    private void _getPurchaserInfo(string arguments)
    {
        var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(arguments);
        var error = (response.error.message != null) ? response.error : null;
        var info = (response.purchaserInfo.activeSubscriptions != null)
                    ? new PurchaserInfo(response.purchaserInfo)
                    : null;
        if (error != null)
        {
            listener.PurchaserInfoReceiveFailed(error);
        }
        else
        {
            listener.PurchaserInfoReceived(info);
        }
    }

    private void _makePurchase(string arguments)
    {
        var response = JsonUtility.FromJson<MakePurchaseResponse>(arguments);

        var error = (response.error.message != null) ? response.error : null;
        var info = (response.purchaserInfo.activeSubscriptions != null)
            ? new PurchaserInfo(response.purchaserInfo)
            : null;

    #if UNITY_ANDROID
        bool userCanceled = (error != null && error.domain.Equals("1") && error.code == 1);
    #else
        bool userCanceled = (error != null && error.domain == "SKErrorDomain" && error.code == 2);
    #endif

        if (error != null)
        {
            listener.PurchaseFailed(response.productIdentifier, error, userCanceled);
        } else {
            listener.PurchaseSucceeded(response.productIdentifier, info);
        }
    }

    private void _createAlias(string arguments)
    {
        ReceivePurchaserInfoMethod(arguments);
    }

    private void _receivePurchaserInfo(string arguments)
    {
        ReceivePurchaserInfoMethod(arguments);
    }

    private void _restoreTransactions(string arguments)
    {
        ReceivePurchaserInfoMethod(arguments);
    }

    private void _identify(string arguments)
    {
        ReceivePurchaserInfoMethod(arguments);
    }

    private void _reset(string arguments)
    {
        ReceivePurchaserInfoMethod(arguments);
    }

    private void _getEntitlements(string entitlementsJSON)
    {
        Debug.Log("entitlements " + entitlementsJSON);
        var response = JsonUtility.FromJson<EntitlementsResponse>(entitlementsJSON);
        var error = (response.error.message != null) ? response.error : null;

        if (error != null)
        {
            listener.EntitlementsReceiveFailed(error);
        }
        else
        {
            listener.EntitlementsReceived(response.entitlements);
        }
    }

    private void ReceivePurchaserInfoMethod(string arguments)
    {
        var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(arguments);

        var error = (response.error.message != null) ? response.error : null;
        var info = (response.purchaserInfo.activeSubscriptions != null)
                    ? new PurchaserInfo(response.purchaserInfo)
                    : null;
        if (error != null)
        {
            listener.PurchaserInfoReceiveFailed(error);
        }
        else
        {
            listener.PurchaserInfoReceived(info);
        }
    }
}