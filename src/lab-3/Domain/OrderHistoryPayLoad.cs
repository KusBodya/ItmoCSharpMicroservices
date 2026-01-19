using Domain.PayLoads;
using System.Text.Json.Serialization;

namespace Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OrderCreatedPayLoad), "created")]
[JsonDerivedType(typeof(OrderItemAddedPayLoad), "item_added")]
[JsonDerivedType(typeof(OrderItemRemovedPayLoad), "item_removed")]
[JsonDerivedType(typeof(OrderStateChangedPayLoad), "state_changed")]
public abstract class OrderHistoryPayLoad { }
