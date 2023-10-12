using System;

namespace Play.Inventario.Contracts
{
    public record InventarioItemCreated(Guid ItemId, string Name, string Description);

    public record InventarioItemUpdated(Guid ItemId, string Name, string Description);

    public record InventarioItemDeleted(Guid ItemId);
}