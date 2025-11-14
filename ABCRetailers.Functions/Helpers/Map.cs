using ABCRetailers.Functions.Entities;
using ABCRetailers.Functions.Models;
using System;

namespace ABCRetailers.Functions.Helpers
{
    public static class Map
    {
        // CUSTOMER MAPPING
        public static CustomerDto ToDto(CustomerEntity entity)
        {
            if (entity == null) return new CustomerDto();
            return new CustomerDto
            {
                CustomerId = entity.RowKey,
                Name = entity.Name,
                Surname = entity.Surname,
                Username = entity.Username,
                Email = entity.Email,
                ShippingAddress = entity.ShippingAddress
            };
        }

        public static CustomerEntity ToEntity(CustomerDto dto)
        {
            if (dto == null) return new CustomerEntity();
            return new CustomerEntity
            {
                PartitionKey = "Customer",
                RowKey = string.IsNullOrEmpty(dto.CustomerId) ? Guid.NewGuid().ToString() : dto.CustomerId,
                Name = dto.Name,
                Surname = dto.Surname,
                Username = dto.Username,
                Email = dto.Email,
                ShippingAddress = dto.ShippingAddress
            };
        }

        // PRODUCT MAPPING
        public static ProductDto ToDto(ProductEntity entity)
        {
            if (entity == null) return new ProductDto();
            return new ProductDto
            {
                ProductId = entity.RowKey,
                ProductName = entity.ProductName,
                Description = entity.Description,
                Price = entity.Price,
                StockAvailable = entity.StockAvailable,
                ImageUrl = entity.ImageUrl
            };
        }

        public static ProductEntity ToEntity(ProductDto dto)
        {
            if (dto == null) return new ProductEntity();
            return new ProductEntity
            {
                PartitionKey = "Product",
                RowKey = string.IsNullOrEmpty(dto.ProductId) ? Guid.NewGuid().ToString() : dto.ProductId,
                ProductName = dto.ProductName,
                Description = dto.Description,
                Price = dto.Price,
                StockAvailable = dto.StockAvailable,
                ImageUrl = dto.ImageUrl
            };
        }

        // ORDER MAPPING
        public static OrderDto ToDto(OrderEntity entity)
        {
            if (entity == null) return new OrderDto();
            return new OrderDto
            {
                OrderId = entity.RowKey,
                CustomerId = entity.CustomerId,
                Username = entity.Username,
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                OrderDate = entity.OrderDate,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                TotalPrice = entity.TotalPrice,
                Status = entity.Status
            };
        }

        public static OrderEntity ToEntity(OrderDto dto)
        {
            if (dto == null) return new OrderEntity();
            return new OrderEntity
            {
                PartitionKey = "Order",
                RowKey = string.IsNullOrEmpty(dto.OrderId) ? Guid.NewGuid().ToString() : dto.OrderId,
                CustomerId = dto.CustomerId,
                Username = dto.Username,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                OrderDate = dto.OrderDate == default ? DateTime.UtcNow : dto.OrderDate,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                TotalPrice = dto.TotalPrice,
                Status = string.IsNullOrEmpty(dto.Status) ? "Submitted" : dto.Status
            };
        }
    }
}
