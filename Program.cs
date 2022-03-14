using D365_Connector.Model;
using D365_Connector.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365_Connector
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                D365Connector d365Connector = new D365Connector("HarutSafaryan@ARM300LLC.onmicrosoft.com", "Taylor2005%", "https://orgc513a061.crm4.dynamics.com/main.aspx?app=d365default&forceUCI=1&pagetype=search&searchText=");
                Console.WriteLine("Succesfully conected to D365");

                Console.Write("Inventory Name: ");
                string inventoryName = Console.ReadLine();
                Console.Write("Product Name: ");
                string productName = Console.ReadLine();
                Console.Write("Quantiy: ");
                string quantity = Console.ReadLine();
                int qty = int.Parse(quantity);
                Console.Write("Type of operation (addition or subtraction): ");
                string operation = Console.ReadLine();

                InventoryProduct result = d365Connector.GetProductQty(inventoryName, productName);

                Console.WriteLine();

                switch (operation)
                {
                    case "addition":
                        if (d365Connector.isProductExistInInventory(inventoryName, productName))
                        {
                            d365Connector.SetProductQty(result, result.Quantity + qty);
                        }
                        else
                        {
                            d365Connector.CreateInventoryProduct(inventoryName, productName, qty);
                        }
                        break;
                    case "subtraction":
                        if (d365Connector.isProductExistInInventory(inventoryName, productName)
                            && result.Quantity > qty)
                        {
                            d365Connector.SetProductQty(result, result.Quantity - qty);
                        }
                        else
                        {
                            Console.WriteLine("it’s not possible");
                        }
                        break;
                    default:
                        Console.WriteLine("Wrong operation was input");
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: {0}", ex.Message);
            }
        }
    }
}
