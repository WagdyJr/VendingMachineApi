namespace VendingMachineAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? Deposit { get; set; } = 0;
        public string Role { get; set; }
    }
}
