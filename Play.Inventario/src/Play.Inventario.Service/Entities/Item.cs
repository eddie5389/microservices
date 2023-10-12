using System;
using Play.Common;
using Play.Inventario.Service.Dtos;

namespace Play.Inventario.Service.Entities
{

    public class Item : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        internal static ItemDto AsDto()
        {
            throw new NotImplementedException();
        }
    }
}