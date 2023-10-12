using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController
    {
        private readonly IRepository<InventoryItem> inventoryItemsRepository;
        // private readonly CatalogClient catalogClient;
        private readonly IRepository<CatalogItem> catalogItemsRepository;

        public ItemsController(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            this.inventoryItemsRepository = inventoryItemsRepository;
            this.catalogItemsRepository = catalogItemsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            // var items = (await inventoryItemsRepository.GetAllAsync(items => items.UserId == userId))
            //             .Select(items => items.AsDto());
            // var catalogItems = await catalogClient.GetCatalogItemsAsync();

            var inventoryitemsEntities = await inventoryItemsRepository.GetAllAsync(items => items.userId == userId);
            var itemsIds = inventoryitemsEntities.Select(itemsIds => items.CatalogItemId);
            var catalogItemEntities = await catalogItemsRepository.GetAllAsync(itemsIds => itemsIds.Contains(item.Id));

            var inventoryItemsDtos = inventoryitemsEntities.Select(InventoryItem =>
            {
                var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == InventoryItem.CatalogItemId);
                return InventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });
            resturn Ok(inventoryItemsDtos);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryItemsRepository.GetAsync(inventoryItemsRepository => item.UserId == grantItemsDto.UserId && inventoryItemsRepository.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.userId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffSet.UtcNow
                }

                await inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            return Ok();
        }
    }
}