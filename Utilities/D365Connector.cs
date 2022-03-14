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

        public bool GetProductId(string name, out Guid productId)
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
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("cre2b_name", ConditionOperator.Equal, name)
                            }
                        }
                    }
                }
            };

            EntityCollection products = service.RetrieveMultiple(query);
            if (products.Entities.Count > 0)
            {
                productId = products.Entities[0].Id;
                return true;
            }
            else
            {
                productId = Guid.NewGuid();
                return false;
            }
        }

        public bool GetInventoryId(string name, out Guid inventoryId)
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
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("cre2b_name", ConditionOperator.Equal, name)
                            }
                        }
                    }
                }
            };

            EntityCollection inventories = service.RetrieveMultiple(query);
            if (inventories.Entities.Count>0)
            {
                inventoryId = inventories.Entities[0].Id;
                return true;
            }
            else
            {
                inventoryId = Guid.NewGuid();
                return false;
            }
        }

        public InventoryProduct GetProductQty(string inventoryName, string productName)
        {
            Guid productId;
            Guid inventoryId;

            bool isProductExist = GetProductId(productName, out productId);
            bool isInventoryExist = GetInventoryId(inventoryName, out inventoryId);

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

            EntityCollection inventoryProducts = service.RetrieveMultiple(query);
            foreach (Entity item in inventoryProducts.Entities)
            {
                EntityReference inventoryLookup = item.GetAttributeValue<EntityReference>("cre2b_fk_inventory");

                if (inventoryLookup != null && inventoryLookup.Name == inventoryName)
                {
                    EntityReference productLookup = item.GetAttributeValue<EntityReference>("cre2b_fk_product");
                    return productName == productLookup.Name;
                }
            }
            return false;
        }

        public void SetProductQty(InventoryProduct inventoryProduct, int qty)
        {
            Entity inventoryProductItem = new Entity("cre2b_inventory_product");

            inventoryProductItem.Id = inventoryProduct.Id;
            inventoryProductItem["cre2b_int_quantity"] = qty;

            service.Update(inventoryProductItem);
        }

        public void CreateInventoryProduct(string inventoryName, string productName, int qty)
        {
            Entity inventoryProductItem = new Entity("cre2b_inventory_product");

            Guid productId;
            Guid inventoryId;

            bool isProductExist = GetProductId(productName, out productId);
            bool isInventoryExist = GetInventoryId(inventoryName, out inventoryId);

            inventoryProductItem["cre2b_fk_inventory"] = new EntityReference("cre2b_inventory", inventoryId);
            inventoryProductItem["cre2b_fk_product"] = new EntityReference("cre2b_product", productId);
            inventoryProductItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("24f919bf-3286-ec11-93b0-6045bd8e9731"));
            inventoryProductItem["cre2b_int_quantity"] = qty;

            service.Create(inventoryProductItem);
        }
    }
}
