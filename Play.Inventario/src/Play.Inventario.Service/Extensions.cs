using Play.Inventario.Service.Dtos;
using Play.Inventario.Service.Entities;

namespace Play.Inventario.Service
{
    public static class Extensions
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
}