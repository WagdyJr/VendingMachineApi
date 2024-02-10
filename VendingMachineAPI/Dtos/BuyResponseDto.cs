namespace VendingMachineAPI.Dtos
{
    public class BuyResponseDto
    {
        public int TotalSpent { get; set; }
        public int ProductsPurchased { get; set; }
        public int Change { get; set; }
    }
}
