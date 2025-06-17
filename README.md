# Event-Driven Kafka Pipeline (C# + Confluent Kafka)

This project demonstrates a **Kafka event streaming pipeline** built with `.NET 8`, using **producers**, **validators**, and **analytics consumers** to simulate real-time purchase events and track analytics.

It showcases:
- ✅ Event modeling with `PurchaseEvent`
- ✅ Real-time validation and message filtering
- ✅ Asynchronous Kafka production and consumption
- ✅ In-memory analytics on streaming data
- ✅ Clean separation of concerns across 3 projects

---

## Project Structure

```bash
.
├── Common/                 # Shared models and topic names
│   ├── Models/PurchaseEvent.cs
│   └── KafkaTopics.cs
├── Producer/              # Sends random purchase events to Kafka
│   ├── producer.cs
│   └── analyticsproducer.cs (demo only)
├── Consumer/              # Two consumers:
│   ├── Validator.cs       # Validates and forwards messages
│   └── Analytics.cs       # Performs in-memory analytics

How It Works 
🟢 1. Producer.cs — Simulates Purchases

    Randomly generates users and items.

    Creates a PurchaseEvent object.

    Serializes to JSON.

    Sends the event to Kafka topic: purchases.

Key Technologies:
ProducerBuilder, JsonSerializer, KafkaTopics.Purchases
🟡 2. Validator.cs — Cleans the Data

    Subscribed to the purchases topic.

    Validates each message:

        Must contain UserId and Item.

        Must be valid JSON.

    Forwards only valid messages to processed-purchases topic.

Key Technologies:
ConsumerBuilder, ProduceAsync, JsonSerializer, error handling
🔵 3. Analytics.cs — Real-Time Insights

    Subscribed to processed-purchases.

    Deserializes events and updates:

        📊 Purchase count per user

        📦 Total count of items sold

    Prints a simple snapshot every 5 messages.

Key Technologies:
Dictionary<string, int>, live counters, analytics snapshot display
Topics Used
Kafka Topic	Purpose
purchases	Raw user purchase events
processed-purchases	Validated purchase events
analytics	(Optional) direct messages for demo
