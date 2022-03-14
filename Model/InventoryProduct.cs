using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365_Connector.Model
{
    public class InventoryProduct
    {
        public Guid Id { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
