using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PurchasesListener : Purchases.UpdatedPurchaserInfoListener
{

    public RectTransform parentPanel;
    public GameObject buttonPrefab;
    public Text purchaserInfoLabel;

    // Use this for initialization
    void Start()
    {
        CreateButton("Restore Purchases", RestoreClicked, 100);

        CreateButton("Switch Username", SwitchUser, 200);

        CreateButton("Send Attribution", SendAttribution, 300);

        Purchases purchases = GetComponent<Purchases>();
        purchases.GetEntitlements((entitlements, error) =>
        {
            if (error != null) {
                LogError(error);
            }
            else
            {
                Debug.Log("entitlements received " + entitlements);
                int yOffset = 0;

                foreach (Purchases.Entitlement entitlement in entitlements.Values)
                {
                    foreach (KeyValuePair<string, Purchases.Product> offering in entitlement.offerings)
                    {
                        Purchases.Product product = offering.Value;
                        Debug.Log(product);
                        Debug.Log(product.identifier);
                        if (product != null)
                        {
                            String label = product.identifier + " " + product.priceString;
                            CreateButton(label, () => ButtonClicked(product.identifier), 500 + yOffset);
                            yOffset += 70;
                        }

                    }
                }

            }
        });
    }

    private void CreateButton(string label, UnityAction action, float yPos)
    {
        GameObject button = (GameObject)Instantiate(buttonPrefab);

        button.transform.SetParent(parentPanel, false);
        button.transform.position = new Vector2(parentPanel.transform.position.x, yPos);

        Button tempButton = button.GetComponent<Button>();

        Text textComponent = tempButton.GetComponentsInChildren<Text>()[0];
        textComponent.text = label;

        tempButton.onClick.AddListener(action);
    }

    private void SwitchUser()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.Identify("newUser", (purchaserInfo, error) =>
        {
            if (error != null)
            {
                DisplayPurchaserInfo(purchaserInfo);
            }
            else
            {
                LogError(error);

            }
        });
    }

    void SendAttribution()
    {
        Purchases purchases = GetComponent<Purchases>();
        Purchases.AdjustData data = new Purchases.AdjustData
        {
            adid = "test",
            network = "network",
            adgroup = "adgroup",
            campaign = "campaign",
            creative = "creative",
            clickLabel = "clickLabel",
            trackerName = "trackerName",
            trackerToken = "trackerToken"
        };

        purchases.AddAttributionData(JsonUtility.ToJson(data), Purchases.AttributionNetwork.ADJUST);
    }

    void ButtonClicked(string product)
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.MakePurchase(product, (productIdentifier, purchaserInfo, userCancelled, error) =>
        {
            if (!userCancelled)
            {
                if (error != null)
                {
                    DisplayPurchaserInfo(purchaserInfo);
                }
                else
                {
                    LogError(error);

                }
            } else
            {
                Debug.Log("Subtester: User canceled, don't show an error");
            }
        });
    }

    void RestoreClicked()
    {
        Purchases purchases = GetComponent<Purchases>();
        purchases.RestoreTransactions((purchaserInfo, error) =>
        {
            if (error != null)
            {
                LogError(error);
            }
            else
            {
                DisplayPurchaserInfo(purchaserInfo);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void PurchaserInfoReceived(Purchases.PurchaserInfo purchaserInfo)
    {
        Debug.Log(string.Format("purchaser info received {0}", purchaserInfo.ActiveSubscriptions));

        DisplayPurchaserInfo(purchaserInfo);
    }

    private void LogError(Purchases.Error error)
    {
        Debug.Log("Subtester: " + JsonUtility.ToJson(error));
    }

    private void DisplayPurchaserInfo(Purchases.PurchaserInfo purchaserInfo)
    {
        string text = "";
        foreach (KeyValuePair<string, DateTime> entry in purchaserInfo.AllExpirationDates)
        {
            string active = (DateTime.UtcNow < entry.Value) ? "subscribed" : "expired";
            text += entry.Key + " " + entry.Value + " " + active + "\n";
        }
        text += purchaserInfo.LatestExpirationDate;

        purchaserInfoLabel.text = text;
    }

}
