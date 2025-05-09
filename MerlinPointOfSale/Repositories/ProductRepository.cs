using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MerlinPointOfSale.Properties;
using MerlinPointOfSale.Models;

namespace MerlinPointOfSale.Repositories
{
    public class ProductRepository
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();

        public ProductRepository(string connectionString)
        {
        }

        public List<Product> SearchProducts(string query)
        {
            string sql = @"
        SELECT SKU, ProductName, Price, CategoryID, IsBaseSKU
        FROM Catalog
        WHERE SKU LIKE @query OR ProductName LIKE @query";

            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString()?.Trim(),
                                IsBaseSKU = Convert.ToBoolean(reader["IsBaseSKU"])
                            });
                        }
                    }
                }
            }

            return products;
        }


        // Search for combos by combo SKU or name
        public List<Combo> SearchCombos(string query)
        {
            List<Combo> combos = new List<Combo>();

            string sql = @"SELECT ComboSKU, ComboName, ComboPrice 
                           FROM [Combos] 
                           WHERE ComboSKU LIKE @query OR ComboName LIKE @query";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            combos.Add(new Combo
                            {
                                ComboSKU = reader["ComboSKU"].ToString(),
                                ComboName = reader["ComboName"].ToString(),
                                ComboPrice = Convert.ToDecimal(reader["ComboPrice"])
                            });
                        }
                    }
                }
            }

            return combos;
        }
        public List<ComboItem> GetComboItems(string comboSKU)
        {
            List<ComboItem> comboItems = new List<ComboItem>();

            string sql = @"
        SELECT ci.ComboSKU, ci.ProductSKU, ci.Quantity, c.ProductName, c.Price, ci.CategoryID
        FROM ComboItems ci
        LEFT JOIN Catalog c ON ci.ProductSKU = c.SKU
        WHERE ci.ComboSKU = @comboSKU";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@comboSKU", comboSKU);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productSKU = reader["ProductSKU"].ToString();
                            if (string.IsNullOrEmpty(productSKU))  // Placeholder for CategoryID
                            {
                                comboItems.Add(new ComboItem
                                {
                                    ComboSKU = reader["ComboSKU"].ToString(),
                                    SKU = null, // No SKU for placeholder
                                    CategoryID = reader["CategoryID"].ToString(),
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                            else  // Standard product in combo
                            {
                                comboItems.Add(new ComboItem
                                {
                                    ComboSKU = reader["ComboSKU"].ToString(),
                                    SKU = productSKU,
                                    ProductName = reader["ProductName"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    Quantity = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }
                    }
                }
            }

            return comboItems;
        }






        public InventoryItem GetProductBySKU(string sku)
        {
            string query = @"SELECT * FROM Catalog WHERE SKU = @SKU";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SKU", sku);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new InventoryItem
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString(),
                                IsBaseSKU = Convert.ToBoolean(reader["IsBaseSKU"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public Combo GetComboBySKU(string comboSKU)
        {
            string query = @"SELECT * FROM Combos WHERE ComboSKU = @ComboSKU";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Combo
                            {
                                ComboSKU = reader["ComboSKU"].ToString(),
                                ComboName = reader["ComboName"].ToString(),
                                ComboPrice = Convert.ToDecimal(reader["ComboPrice"]),
                            };
                        }
                    }
                }
            }
            return null;
        }


        public List<Product> GetProductsByCategory(string categoryID)
        {
            List<Product> products = new List<Product>();

            string sql = "SELECT SKU, ProductName, Price, CategoryID, IsBaseSKU, FROM [Catalog] WHERE CategoryID = @categoryID";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@categoryID", categoryID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString(),
                                IsBaseSKU = Convert.ToBoolean(reader["IsBaseSKU"])
                            });
                        }
                    }
                }
            }

            return products;
        }

        public string GetCategoryNameByID(string categoryID)
        {
            string categoryName = "";

            string sql = @"SELECT CategoryName FROM CategoryMap WHERE CategoryID = @categoryID";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@categoryID", categoryID);
                    categoryName = (string)cmd.ExecuteScalar();
                }
            }

            return categoryName;
        }



        // Update inventory quantities after a sale
        public void UpdateInventoryAfterSale(string sku, int quantitySold)
        {
            string sql = "UPDATE Inventory SET QuantityOnHandSellable = QuantityOnHandSellable - @quantitySold WHERE SKU = @sku";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sku", sku);
                    cmd.Parameters.AddWithValue("@quantitySold", quantitySold);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<InventoryItem> SearchInventory(string query)
        {
            string sql = @"
        SELECT c.SKU, c.ProductName, c.Price, c.CategoryID, cm.CategoryName, c.IsBaseSKU,
               i.QuantityOnHandSellable, i.QuantityOnHandDefective
        FROM Catalog c
        LEFT JOIN Inventory i ON c.SKU = i.SKU AND i.LocationID = @locationID
        LEFT JOIN CategoryMap cm ON c.CategoryID = cm.CategoryID
        WHERE (c.SKU LIKE @query OR c.ProductName LIKE @query)
    ";

            List<InventoryItem> items = new List<InventoryItem>();

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");
                    cmd.Parameters.AddWithValue("@locationID", Settings.Default.LocationID); // Correct LocationID

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new InventoryItem
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString()?.Trim(),
                                CategoryName = reader["CategoryName"].ToString()?.Trim(),
                                IsBaseSKU = Convert.ToBoolean(reader["IsBaseSKU"]),
                                QuantityOnHandSellable = reader["QuantityOnHandSellable"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QuantityOnHandSellable"]),
                                QuantityOnHandDefective = reader["QuantityOnHandDefective"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QuantityOnHandDefective"]),
                            });
                        }
                    }
                }
            }

            return items;
        }



        public List<QuickSelectItem> GetQuickSelectItems(string locationID)
        {
            var quickSelectItems = new List<QuickSelectItem>();

            string query = @"
                SELECT 
                    lq.SKU, 
                    c.ProductName, 
                    c.Price,
                    c.CategoryID,
                    cm.CategoryName
                FROM LocationQuickSelect lq
                INNER JOIN Catalog c ON lq.SKU = c.SKU
                INNER JOIN CategoryMap cm ON c.CategoryID = cm.CategoryID
                WHERE lq.LocationID = @LocationID
                ORDER BY cm.CategoryName, c.ProductName";

            using (var connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationID);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            quickSelectItems.Add(new QuickSelectItem
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = (decimal)reader["Price"],
                                CategoryID = reader["CategoryID"].ToString()?.Trim(),
                                CategoryName = reader["CategoryName"].ToString(),
                                ProductDisplayText = $"{reader["ProductName"]} ({reader["SKU"]}) - {reader["Price"]:C}"
                            });
                        }
                    }
                }
            }

            return quickSelectItems;
        }


        public Product GetBestTradeCandidate(string input)
        {
            Product originalProduct = null;

            string query = @"
        SELECT TOP 1 SKU, ProductName, Price, CategoryID, UPC, IsBaseSKU
        FROM Catalog
        WHERE SKU = @Input OR UPC = @Input OR ProductName LIKE @InputLike";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Input", input);
                    cmd.Parameters.AddWithValue("@InputLike", $"%{input}%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            originalProduct = new Product
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString(),
                                IsBaseSKU = Convert.ToBoolean(reader["IsBaseSKU"])
                            };
                        }
                    }
                }

                if (originalProduct == null)
                    return null;

                string usedCategoryID = "9" + originalProduct.CategoryID.Substring(1);
                string cleanName = originalProduct.ProductName.Replace(" (Used)", "").Trim();

                string usedQuery = @"
            SELECT TOP 1 SKU, ProductName, Price, TradeValue, CategoryID, UPC, IsBaseSKU
            FROM Catalog
            WHERE CategoryID = @UsedCategoryID AND 
                  (ProductName LIKE @UsedName OR UPC = @OriginalUPC)
            ORDER BY ProductName DESC";

                using (SqlCommand cmdUsed = new SqlCommand(usedQuery, conn))
                {
                    cmdUsed.Parameters.AddWithValue("@UsedCategoryID", usedCategoryID);
                    cmdUsed.Parameters.AddWithValue("@UsedName", $"%{cleanName} (Used)%");
                    cmdUsed.Parameters.AddWithValue("@OriginalUPC", input);

                    using (SqlDataReader readerUsed = cmdUsed.ExecuteReader())
                    {
                        if (readerUsed.Read())
                        {
                            return new Product
                            {
                                SKU = readerUsed["SKU"].ToString(),
                                ProductName = readerUsed["ProductName"].ToString(),
                                Price = Convert.ToDecimal(readerUsed["TradeValue"]),
                                CategoryID = readerUsed["CategoryID"].ToString(),
                                IsBaseSKU = Convert.ToBoolean(readerUsed["IsBaseSKU"]),
                                SupplierID = "", // optional
                                TradeValue = readerUsed["TradeValue"] != DBNull.Value ? Convert.ToDecimal(readerUsed["TradeValue"]) : 0
                            };

                        }
                    }
                }

                return null; // Do NOT return new version if used isn't found
            }
        }



    }
}