namespace HotelOS.Contracts.Purchase;

public record VendorDto(
    long Id,
    string VendorCode,
    string VendorName,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? TaxNumber,
    int PaymentTermsDays,
    string Status
);

public record CreateVendorRequest(
    string VendorCode,
    string VendorName,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? TaxNumber,
    int PaymentTermsDays
);

public record UpdateVendorRequest(
    string VendorName,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? TaxNumber,
    int PaymentTermsDays,
    string? Status = null
);

public record PurchaseRequisitionItemDto(
    long Id,
    string ItemName,
    string? Description,
    decimal Quantity,
    string Unit,
    decimal EstimatedUnitPrice,
    decimal TotalEstimatedPrice
);

public record CreateRequisitionItemRequest(
    string ItemName,
    string? Description,
    decimal Quantity,
    string Unit,
    decimal EstimatedUnitPrice
);

public record PurchaseRequisitionDto(
    long Id,
    string RequisitionNumber,
    string DepartmentCode,
    long RequestedByUserId,
    string? RequestedByUserName,
    string RequiredByDate,
    string Priority,
    string Status,
    decimal TotalEstimatedAmount,
    string? Justification,
    DateTimeOffset CreatedAt,
    List<PurchaseRequisitionItemDto> Items
);

public record CreatePurchaseRequisitionRequest(
    string DepartmentCode,
    string RequiredByDate,
    string Priority,
    string? Justification,
    List<CreateRequisitionItemRequest> Items
);

public record UpdateRequisitionStatusRequest(
    string Status
);

public record PurchaseOrderItemDto(
    long Id,
    string ItemName,
    decimal QuantityOrdered,
    decimal QuantityReceived,
    string Unit,
    decimal UnitPrice,
    decimal TotalPrice
);

public record CreatePurchaseOrderItemRequest(
    string ItemName,
    decimal QuantityOrdered,
    string Unit,
    decimal UnitPrice
);

public record PurchaseOrderDto(
    long Id,
    string OrderNumber,
    long? PurchaseRequisitionId,
    long VendorId,
    string VendorName,
    string OrderDate,
    string ExpectedDeliveryDate,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string? PaymentTerms,
    string? Notes,
    DateTimeOffset CreatedAt,
    List<PurchaseOrderItemDto> Items
);

public record CreatePurchaseOrderRequest(
    long VendorId,
    long? PurchaseRequisitionId,
    string OrderDate,
    string ExpectedDeliveryDate,
    string? PaymentTerms,
    string? Notes,
    List<CreatePurchaseOrderItemRequest> Items
);

public record UpdatePurchaseOrderStatusRequest(
    string Status
);
