namespace VendingMachineAPI.Dtos
{
    public class NewProductDto
    {
        public int? AmountAvailable { get; set; }
        public int? Price { get; set; }
        public string? ProductName { get; set; }
    }
}
