using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class PurchasesWrapperiOS : PurchasesWrapper
{
    [DllImport("__Internal")]
    private static extern void _RCSetupPurchases(string gameObject, string apiKey, string appUserID);
    public void Setup(string gameObject, string apiKey, string appUserID)
    {
        _RCSetupPurchases(gameObject, apiKey, appUserID);
    }

	public class ProductsRequest
    {
        public string[] productIdentifiers;
    }

    [DllImport("__Internal")]
    private static extern void _RCGetProducts(string productIdentifiersJSON, string type);
    public void GetProducts(string[] productIdentifiers, string type = "subs")
    {
        ProductsRequest request = new ProductsRequest
        {
            productIdentifiers = productIdentifiers
        };

        _RCGetProducts(JsonUtility.ToJson(request), type);
    }

    [DllImport("__Internal")]
    private static extern void _RCMakePurchase(string productIdentifier, string type);
    public void MakePurchase(string productIdentifier, string type = "subs", string oldSku = null)
    {
        _RCMakePurchase(productIdentifier, type);
    }

    [DllImport("__Internal")]
    private static extern void _RCRestoreTransactions();
	public void RestoreTransactions()
	{
        _RCRestoreTransactions();
	}

	[DllImport("__Internal")]
	private static extern void _RCAddAttributionData(string network, string data);
	public void AddAttributionData(string network, string data)
	{
		_RCAddAttributionData(network, data);
	}

	[DllImport("__Internal")]
    private static extern void _RCCreateAlias(string newAppUserID);
    public void CreateAlias(string newAppUserID)
    {
        _RCCreateAlias(newAppUserID);
    }

    [DllImport("__Internal")]
    private static extern void _RCIdentify(string appUserID);
    public void Identify(string appUserID)
    {
        _RCIdentify(appUserID);
    }

    [DllImport("__Internal")]
    private static extern void _RCReset();
    public void Reset()
    {
        _RCReset();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetAllowSharingStoreAccount(bool allow);
    public void SetAllowSharingStoreAccount(bool allow)
    {
        _RCSetAllowSharingStoreAccount(allow);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetEntitlements();
    public void GetEntitlements()
    {
        _RCGetEntitlements();
    }

    [DllImport("__Internal")]
    private static extern void _RCGetAppUserID();
    public void GetAppUserID()
    {
        _RCGetAppUserID();
    }

    [DllImport("__Internal")]
    private static extern void _RCSetDebugLogsEnabled(bool enabled);
    public void SetDebugLogsEnabled(bool enabled)
    {
        _RCSetDebugLogsEnabled(enabled);
    }

    [DllImport("__Internal")]
    private static extern void _RCGetPurchaserInfo();
    public void GetPurchaserInfo()
    {
        _RCGetPurchaserInfo();
    }

}
