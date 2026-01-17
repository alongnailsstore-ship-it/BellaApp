using Firebase.Database.Query;
using BellaLink.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BellaLink.App.Services
{
	public partial class DatabaseService
	{
		// =================================================================================
		// GESTÃO DE PRODUTOS E FORNECEDORES
		// =================================================================================

		public async Task SaveProductAsync(Product product)
		{
			if (string.IsNullOrEmpty(product.Id)) product.Id = Guid.NewGuid().ToString();
			if (string.IsNullOrEmpty(product.DistributorId)) return;

			// Salva em products/distributorId/productId
			await _client.Child(ProductsNode).Child(product.DistributorId).Child(product.Id).PutAsync(product);
		}

		public async Task<List<Product>> GetProductsByDistributorAsync(string distributorId)
		{
			if (string.IsNullOrEmpty(distributorId)) return new List<Product>();
			try
			{
				var items = await _client.Child(ProductsNode).Child(distributorId).OnceAsync<Product>();
				return items.Select(x => x.Object).ToList();
			}
			catch { return new List<Product>(); }
		}

		public async Task DeleteProductAsync(string distributorId, string productId)
		{
			if (string.IsNullOrEmpty(distributorId) || string.IsNullOrEmpty(productId)) return;
			await _client.Child(ProductsNode).Child(distributorId).Child(productId).DeleteAsync();
		}

		public async Task<List<Partner>> GetSuppliersAsync()
		{
			try
			{
				var allUsers = await _client.Child(UsersNode).OnceAsync<User>();
				var supplierIds = allUsers.Where(u => u.Object.IsSupplier).Select(u => u.Key).ToList();

				var suppliers = new List<Partner>();
				foreach (var id in supplierIds)
				{
					var p = await GetPartnerAsync(id);
					if (p != null) suppliers.Add(p);
				}
				return suppliers;
			}
			catch { return new List<Partner>(); }
		}
	}
}