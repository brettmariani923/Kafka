using System;
using System.Text.Json;
using Common.Models;

public class Validator
{
    public bool TryValidate(string json, out PurchaseEvent? purchaseEvent)
    {//takes a JSON string and tries to deserialize it into a PurchaseEvent object.
        purchaseEvent = null;
        try
        {
            purchaseEvent = JsonSerializer.Deserialize<PurchaseEvent>(json);
            if (purchaseEvent == null) return false;

            if (string.IsNullOrWhiteSpace(purchaseEvent.UserId) ||
                string.IsNullOrWhiteSpace(purchaseEvent.Item))
            {
                return false;
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
// if output is valid, it returns true and sets the out parameter to the deserialized PurchaseEvent object.
//if false it fails
