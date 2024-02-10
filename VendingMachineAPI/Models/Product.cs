namespace VendingMachineAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int AmountAvailable { get; set; }
        public int Price { get; set; }
        public string ProductName { get; set; }
        public int SellerId { get; set; }
    }
}
