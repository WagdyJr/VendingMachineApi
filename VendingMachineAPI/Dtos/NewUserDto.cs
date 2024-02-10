namespace VendingMachineAPI.Dtos
{
    public class NewUserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int? Deposit { get; set; }
    }
}
