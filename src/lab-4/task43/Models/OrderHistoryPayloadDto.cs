using System.Text.Json.Serialization;

namespace Task43.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(OrderCreatedPayloadDto), "created")]
[JsonDerivedType(typeof(OrderItemAddedPayloadDto), "itemAdded")]
[JsonDerivedType(typeof(OrderItemRemovedPayloadDto), "itemRemoved")]
[JsonDerivedType(typeof(OrderStateChangedPayloadDto), "stateChanged")]
public abstract record OrderHistoryPayloadDto;
