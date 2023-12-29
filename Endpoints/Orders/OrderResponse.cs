namespace IWantApp.Endpoints.Orders;

public record OrderResponse(Guid Id, decimal Total, string DeliveryAddress, List<OrderProduct> products);

public record OrderProduct(Guid Id, string Name, decimal Price, string Description, string Category);
