using D365_Connector.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365_Connector.Utilities
{
    class D365Connector
    {
        private string D365username;
        private string D365password;
        private string D365URL;

        private CrmServiceClient service;

        public D365Connector(string D365username, string D365password, string D365URL)
        {
            string authType = "OAuth";
            string appId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
            string reDirectURI = "app://58145B91-0C36-4500-8554-080854F2AC97";
            string loginPrompt = "Auto";

            this.D365username = D365username;
            this.D365password = D365password;
            this.D365URL = D365URL;

            string ConnectionString = string.Format("AuthType = {0};Username = {1};Password = {2}; Url = {3}; AppId={4}; RedirectUri={5};LoginPrompt={6}",
                                         authType, D365username, D365password, D365URL, appId, reDirectURI, loginPrompt);

            this.service = new CrmServiceClient(ConnectionString);
        }

        
        public Guid GetProductId(string name)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "cre2b_product",
                Criteria =
                {
                    Filters =
                    {
                        new FilterExpression
                        {
                            Conditions= {new ConditionExpression("cre2b_name", ConditionOperator.Equal, name)}
                        }
                    }
                }
            };

            EntityCollection products = service.RetrieveMultiple(query);
            if (products.Entities.Count > 0)
                return products.Entities[0].Id;
            else
                return Guid.Empty;
        }

        public Guid GetInventoryId(string name)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "cre2b_inventory",
                Criteria =
                {
                    Filters =
                    {
                        new FilterExpression
                        {
                           Conditions =
                            {
                            new ConditionExpression("cre2b_name", ConditionOperator.Equal, name)
                            }
                        }
                    }
                }
            };

            EntityCollection inventories = service.RetrieveMultiple(query);
            if (inventories.Entities.Count > 0)
                return inventories.Entities[0].Id;
            else
                return Guid.Empty;
        }

        public InventoryProduct GetProductQty(string inventoryName, string productName)
        {
            Guid productId = GetProductId(productName);
            Guid inventoryId = GetInventoryId(inventoryName);

            QueryExpression query = new QueryExpression
            {
                EntityName = "cre2b_inventory_product",
                ColumnSet = new ColumnSet("cre2b_fk_inventory", "cre2b_fk_product", "cre2b_int_quantity", "cre2b_name"),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("cre2b_fk_inventory", ConditionOperator.Equal, inventoryId),
                        new ConditionExpression("cre2b_fk_product", ConditionOperator.Equal, productId)
                    }
                }
            };

            EntityCollection inventoryProducts = service.RetrieveMultiple(query);
            if (inventoryProducts.Entities.Count > 0)
            {
                InventoryProduct inventoryProduct = new InventoryProduct
                {
                    Id = inventoryProducts.Entities[0].Id,
                    InventoryId = inventoryId,
                    ProductId = productId,
                    ProductName = inventoryProducts.Entities[0].GetAttributeValue<string>("cre2b_name"),
                    Quantity = inventoryProducts.Entities[0].GetAttributeValue<int>("cre2b_int_quantity")
                };
                return inventoryProduct;
            }
            else
                return null;
        }

        public bool isProductExistInInventory(string inventoryName, string productName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "cre2b_inventory_product",
                ColumnSet = new ColumnSet("cre2b_fk_inventory", "cre2b_fk_product", "cre2b_int_quantity")
            };

            bool contains = false;
            EntityCollection inventoryProducts = service.RetrieveMultiple(query);
            foreach (Entity item in inventoryProducts.Entities)
            {
                EntityReference inventoryLookup = item.GetAttributeValue<EntityReference>("cre2b_fk_inventory");

                if (inventoryLookup != null && inventoryLookup.Name == inventoryName)
                {
                    EntityReference productLookup = item.GetAttributeValue<EntityReference>("cre2b_fk_product");
                    if (productName == productLookup.Name)
                        contains = true;
                }
            }
            return contains;
        }

        public void SetProductQty(InventoryProduct inventoryProduct, int qty)
        {

            Entity inventoryProductItem = service.Retrieve("cre2b_inventory_product", inventoryProduct.Id, new ColumnSet("cre2b_price_per_unit", "cre2b_int_quantity", "cre2b_total_amount"));

            Money price = inventoryProductItem.GetAttributeValue<Money>("cre2b_price_per_unit");
            Money totalAmount = new Money(price.Value * qty);

            inventoryProductItem["cre2b_int_quantity"] = qty;
            inventoryProductItem["cre2b_total_amount"] = totalAmount;

            service.Update(inventoryProductItem);
        }

        /// <summary>
        /// Create Product for given Inventory with given quantity
        /// </summary>
        /// <param name="inventoryName">Inventory name</param>
        /// <param name="productName">Product name</param>
        /// <param name="qty">Quantity</param>
        public void CreateInventoryProduct(string inventoryName, string productName, int qty)
        {
            Entity inventoryProductItem = new Entity("cre2b_inventory_product");

            Guid productId = GetProductId(productName);
            Guid inventoryId = GetInventoryId(inventoryName);

            Entity inventory = service.Retrieve("cre2b_inventory", inventoryId, new ColumnSet("cre2b_fk_price_list", "transactioncurrencyid"));
            EntityReference currency = inventory.GetAttributeValue<EntityReference>("transactioncurrencyid");

            EntityReference priceListRef = inventory.GetAttributeValue<EntityReference>("cre2b_fk_price_list");

            if (priceListRef != null)
            {
                QueryExpression query = new QueryExpression
                {
                    EntityName = "cre2b_price_list_item",
                    ColumnSet = new ColumnSet("cre2b_fk_product", "transactioncurrencyid", "cre2b_mon_price_per_unit"),
                    Criteria =
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("cre2b_fk_product", ConditionOperator.Equal, productId),
                            new ConditionExpression("transactioncurrencyid", ConditionOperator.Equal, currency.Id)
                        }
                    }
                };

                EntityCollection priceLists = service.RetrieveMultiple(query);

                if (productId != Guid.Empty && inventoryId != Guid.Empty)
                {
                    Money pricePerUnit = priceLists.Entities[0].GetAttributeValue<Money>("cre2b_mon_price_per_unit");
                    EntityReference transactioncurrencyRef = priceLists.Entities[0].GetAttributeValue<EntityReference>("transactioncurrencyid");

                    inventoryProductItem["cre2b_fk_inventory"] = new EntityReference("cre2b_inventory", inventoryId);
                    inventoryProductItem["cre2b_fk_product"] = new EntityReference("cre2b_product", productId);
                    inventoryProductItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", transactioncurrencyRef.Id);
                    inventoryProductItem["cre2b_price_per_unit"] = pricePerUnit;
                    inventoryProductItem["cre2b_int_quantity"] = qty;

                    service.Create(inventoryProductItem);
                }
            }
        }
    }
}
