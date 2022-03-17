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
                D365Connector d365Connector = new D365Connector("HarutSafaryan@PROFLLC796.onmicrosoft.com", "Aa_%5842bnm_", "https://org395b328f.crm4.dynamics.com/");
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
                            Console.ReadLine();
                        }
                        break;
                    default:
                        Console.WriteLine("Wrong operation was input");
                        Console.ReadLine();
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
