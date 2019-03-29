using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using System.Collections.Generic;

#pragma warning disable CS0649

public class Purchases : MonoBehaviour
{

    public delegate void GetProductsFunc(List<Product> products, Error error);

    public delegate void MakePurchaseFunc(string productIdentifier, PurchaserInfo purchaserInfo, bool userCancelled, Error error);

    public delegate void PurchaserInfoFunc(PurchaserInfo purchaserInfo, Error error);

    public delegate void GetEntitlementsFunc(Dictionary<string, Entitlement> entitlements, Error error);

    public abstract class UpdatedPurchaserInfoListener : MonoBehaviour
    {
        public abstract void PurchaserInfoReceived(PurchaserInfo purchaserInfo);
    }

    private class PurchasesWrapperNoop : PurchasesWrapper
    {
        public void Setup(string gameObject, string apiKey, string appUserID)
        {

        }

        public void AddAttributionData(int network, string data)
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

        public void SetFinishTransactions(bool finishTransactions)
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

    public class Entitlement
    {

        public Dictionary<string, Product> offerings;

        public Entitlement(EntitlementResponse response)
        {
            this.offerings = new Dictionary<string, Product>();
            foreach (Offering offering in response.offerings)
            {
                Debug.Log("offering " + offering.product);
                if (offering.product.identifier != null)
                {
                    this.offerings.Add(offering.offeringId, offering.product);
                }
            }
        }

    }


    [Tooltip("Your RevenueCat API Key. Get from https://app.revenuecat.com/")]
    public string revenueCatAPIKey;

    [Tooltip(
        "App user id. Pass in your own ID if your app has accounts. If blank, RevenueCat will generate a user ID for you.")]
    public string appUserID;

    [Tooltip("List of product identifiers.")]
    public string[] productIdentifiers;

    [Tooltip("A subclass of Purchases.Listener component. Use your custom subclass to define how to handle events.")]
    //public Listener listener;

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
        GetProducts(productIdentifiers, null);
    }

    // Call this if you want to reset with a new user id
    public void Setup(string newUserID)
    {
        wrapper.Setup(gameObject.name, revenueCatAPIKey, newUserID);
    }

    private GetProductsFunc productsCallback { get; set; }

    // Optionally call this if you want to fetch more products,
    // called automatically with pre-configured products
    public void GetProducts(string[] products, GetProductsFunc callback)
    {
        productsCallback = callback;
        wrapper.GetProducts(products);
    }

    private MakePurchaseFunc makePurchaseCallback { get; set; }

    // Call this to initiate a purchase
    public void MakePurchase(string productIdentifier, MakePurchaseFunc callback,
        string type = "subs", string oldSku = null)
    {
        makePurchaseCallback = callback;
        wrapper.MakePurchase(productIdentifier, type, oldSku);
    }

    private PurchaserInfoFunc restoreTransactionsCallback { get; set; }

    public void RestoreTransactions(PurchaserInfoFunc callback)
    {
        restoreTransactionsCallback = callback;
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

    public enum AttributionNetwork
    {
        APPLE_SEARCH_ADS = 0,
        ADJUST = 1,
        APPSFLYER = 2,
        BRANCH = 3,
        TENJIN = 4
    };

    public void AddAdjustAttributionData(AdjustData data)
    {
        wrapper.AddAttributionData((int)AttributionNetwork.ADJUST, JsonUtility.ToJson(data));
    }

    public void AddAttributionData(string dataJSON, AttributionNetwork network)
    {
        wrapper.AddAttributionData((int)network, dataJSON);
    }

    private PurchaserInfoFunc createAliasCallback { get; set; }

    public void CreateAlias(string newAppUserID, PurchaserInfoFunc callback)
    {
        createAliasCallback = callback;
        wrapper.CreateAlias(newAppUserID);
    }

    private PurchaserInfoFunc identifyCallback { get; set; }

    public void Identify(string appUserID, PurchaserInfoFunc callback)
    {
        identifyCallback = callback;
        wrapper.Identify(appUserID);
    }

    private PurchaserInfoFunc resetCallback { get; set; }

    public void Reset(PurchaserInfoFunc callback)
    {
        resetCallback = callback;
        wrapper.Reset();
    }

    public void SetFinishTransactions(bool finishTransactions)
    {
        wrapper.SetFinishTransactions(finishTransactions);
    }

    public void SetAllowSharingStoreAccount(bool allow)
    {
        wrapper.SetAllowSharingStoreAccount(allow);
    }

    public void SetDebugLogsEnabled(bool enabled)
    {
        wrapper.SetDebugLogsEnabled(enabled);
    }

    private PurchaserInfoFunc getPurchaserInfoCallback { get; set; }

    public void GetPurchaserInfo(PurchaserInfoFunc callback)
    {
        getPurchaserInfoCallback = callback;
        wrapper.GetPurchaserInfo();
    }

    private GetEntitlementsFunc getEntitlementsCallback { get; set; }

    public void GetEntitlements(GetEntitlementsFunc callback)
    {
        getEntitlementsCallback = callback;
        wrapper.GetEntitlements();
    }

    private void _receiveProducts(string productsJSON)
    {
        Debug.Log("_receiveProducts " + productsJSON);
        var response = JsonUtility.FromJson<ProductResponse>(productsJSON);
        var error = (response.error.message != null) ? response.error : null;

        if (productsCallback != null)
        {
            if (error != null)
            {
                productsCallback(null, error);
            }
            else
            {
                productsCallback(response.products, null);
            }
            productsCallback = null;
        }
    }

    private void _getPurchaserInfo(string purchaserInfoJSON)
    {
        Debug.Log("_getPurchaserInfo " + purchaserInfoJSON);
        var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(purchaserInfoJSON);
        var error = (response.error.message != null) ? response.error : null;
        var info = (response.purchaserInfo.activeSubscriptions != null)
                    ? new PurchaserInfo(response.purchaserInfo)
                    : null;
        if (getPurchaserInfoCallback != null)
        {
            if (error != null)
            {
                getPurchaserInfoCallback(null, error);
            }
            else
            {
                getPurchaserInfoCallback(info, null);
            }
            getPurchaserInfoCallback = null;
        }
    }

    private void _makePurchase(string makePurchaseResponseJSON)
    {
        Debug.Log("_makePurchase " + makePurchaseResponseJSON);
        var response = JsonUtility.FromJson<MakePurchaseResponse>(makePurchaseResponseJSON);

        var error = (response.error.message != null) ? response.error : null;
        var info = (response.purchaserInfo.activeSubscriptions != null)
            ? new PurchaserInfo(response.purchaserInfo)
            : null;
        var userCancelled = response.userCancelled;

        if (makePurchaseCallback != null)
        {
            if (error != null)
            {
                makePurchaseCallback(null, null, userCancelled, error);
            }
            else
            {
                makePurchaseCallback(response.productIdentifier, info, false, null);
            }
            makePurchaseCallback = null;

        }
    }

    private void _createAlias(string purchaserInfoJSON)
    {
        Debug.Log("_createAlias " + purchaserInfoJSON);
        ReceivePurchaserInfoMethod(purchaserInfoJSON, createAliasCallback);
    }

    private void _receivePurchaserInfo(string purchaserInfoJSON)
    {
        Debug.Log("_receivePurchaserInfo " + purchaserInfoJSON);
        ReceivePurchaserInfoMethod(purchaserInfoJSON, getPurchaserInfoCallback);
    }

    private void _restoreTransactions(string purchaserInfoJSON)
    {
        Debug.Log("_restoreTransactions " + purchaserInfoJSON);
        ReceivePurchaserInfoMethod(purchaserInfoJSON, restoreTransactionsCallback);
    }

    private void _identify(string purchaserInfoJSON)
    {
        Debug.Log("_identify " + purchaserInfoJSON);
        ReceivePurchaserInfoMethod(purchaserInfoJSON, identifyCallback);
    }

    private void _reset(string purchaserInfoJSON)
    {
        Debug.Log("_reset " + purchaserInfoJSON);
        ReceivePurchaserInfoMethod(purchaserInfoJSON, resetCallback);
    }

    private void _getEntitlements(string entitlementsJSON)
    {
        Debug.Log("_getEntitlements " + entitlementsJSON);
        if (getEntitlementsCallback != null)
        {
            EntitlementsResponse response = JsonUtility.FromJson<EntitlementsResponse>(entitlementsJSON);
            var error = (response.error.message != null) ? response.error : null;
            if (error != null)
            {
                getEntitlementsCallback(null, error);
            }
            else
            {
                Dictionary<string, Entitlement> entitlements = new Dictionary<string, Entitlement>();
                foreach (EntitlementResponse entitlementResponse in response.entitlements)
                {
                    Debug.Log(entitlementResponse.entitlementId);
                    entitlements.Add(entitlementResponse.entitlementId, new Entitlement(entitlementResponse));
                }
                getEntitlementsCallback(entitlements, null);
            }
            getEntitlementsCallback = null;
        }
    }

    private void ReceivePurchaserInfoMethod(string arguments, PurchaserInfoFunc callback)
    {
        if (callback != null)
        {
            var response = JsonUtility.FromJson<ReceivePurchaserInfoResponse>(arguments);

            var error = (response.error.message != null) ? response.error : null;
            var info = (response.purchaserInfo.activeSubscriptions != null)
                        ? new PurchaserInfo(response.purchaserInfo)
                        : null;
            if (error != null)
            {
                callback(null, error);
            }
            else
            {
                callback(info, null);
            }
            callback = null;
            // TODO: test it nullifies it
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
        public bool userCancelled;
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
        public List<EntitlementResponse> entitlements;
        public Error error;
    }

    [System.Serializable]
    public class Offering
    {
        public string offeringId;
        public Product product;
    }

    [System.Serializable]
    public class EntitlementResponse
    {
        public string entitlementId;
        public List<Offering> offerings;
    }

}