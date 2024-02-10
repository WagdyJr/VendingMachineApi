namespace VendingMachineAPI.Models
{
    public class LoginResponse
    {
        public LoginResponse()
        {
            this.Token = String.Empty;
        }
        public string Token { get; set; }
    }
}
