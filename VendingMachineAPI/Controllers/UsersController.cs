using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using VendingMachineAPI.Data;
using VendingMachineAPI.Dtos;
using VendingMachineAPI.Models;

namespace VendingMachineAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly ApiContext _context;

        public UsersController(ApiContext context) 
        {
            _context = context;
        }

        [HttpPost("CreateNewUser")]
        public async Task<ActionResult<User>> CreateNewUser(NewUserDto user)
        {
            var check = _context.Users.FirstOrDefault(x => x.Username == user.Username);
            if (check != null)
            {
                return BadRequest("User already Exists");
            }

            _context.Users.Add(new User
            {
                Username = user.Username,
                Password = user.Password,
                Role = user.Role.ToUpper(),
                Deposit = user.Deposit
            });

            _context.SaveChanges();
            return Ok("New user added successfully");
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            if (users == null)
            {
                return NotFound("No Users Found.");
            }
            return Ok(users);
        }

        [Authorize]
        [HttpGet("GetUserById")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return BadRequest("User doesn't Exist!");
            }
            
            return Ok(user);
        }

        [Authorize]
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<User>> UpdateUser(int id, NewUserDto updatedUser)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserId == id);

            if (user == null)
            {
                return BadRequest("User doesn't Exist!");
            }

            if(User.Identity.Name != user.Username) 
            {
                return Forbid("You are not allowed to update other users info!");   
            }

            foreach (var propertyInfo in typeof(NewProductDto).GetProperties())
            {
                var newValue = propertyInfo.GetValue(updatedUser);
                var currentValue = user.GetType().GetProperty(propertyInfo.Name)?.GetValue(user);

                if (newValue != null && !newValue.Equals(currentValue))
                {
                    user.GetType().GetProperty(propertyInfo.Name)?.SetValue(user, newValue);
                }
            }

            _context.SaveChanges();
            
            return Ok("User Updated successfully");
        }

        [Authorize]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return BadRequest("User doesn't Exist!");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            return Ok("User Deleted");
        }

        [Authorize]
        [HttpPost("DepositCoins")]
        public async Task<ActionResult<User>> DepositCoins(int id, int coins)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserId == id);
            int[] acceptableCoins = { 5, 10, 20, 50, 100 };

            if (user == null)
            {
                return BadRequest("User doesn't Exist");
            }
            if(User.Identity.Name != user.Username)
            {
                return Forbid("Can't Deposit Coins for other Users");
            }

            if(!acceptableCoins.Any(x => x == user.Deposit))
            {
                return BadRequest("unacceptable coin value.");
            }

            user.Deposit += coins;
            _context.SaveChanges();

            return Ok("Money Deposited");
        }

        [Authorize]
        [HttpPost("{id}/BuyProduct")]
        public async Task<ActionResult<BuyResponseDto>> BuyProduct(int id, [FromBody] BuyRequestDto buyRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            int totalCost = 0;

            if (user == null)
            {
                return BadRequest("User doesn't Exist");
            }
            else if(user.Role != "Buyer")
            {
                return Forbid("Only users with a 'Buyer' role can buy products");
            }

            foreach (var productItem in buyRequest.Products)
            {
                var product = await _context.Products.FirstOrDefaultAsync(x=>x.ProductId == productItem.ProductId);
                if (product == null)
                {
                    return NotFound($"Product with ID {productItem.ProductId} not found");
                }
                totalCost += product.Price * productItem.Quantity;
            }

            if (user.Deposit < totalCost)
            {
                return BadRequest("Insufficient funds");
            }

            user.Deposit -= totalCost;
            _context.SaveChanges();

            var response = new BuyResponseDto
            {
                TotalSpent = totalCost,
                ProductsPurchased = buyRequest.Products.Sum(x => x.Quantity),
                Change = (int)user.Deposit
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPost("{id}/ResetDeposit")]
        public async Task<ActionResult<User>> ResetDeposit(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserId == id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            else if (user.Role != "Buyer")
            {
                return Forbid("Only users with a 'Buyer' role can reset their deposit");
            }
            else if (User.Identity.Name != user.Username)
            {
                return Forbid("Can't Reset Deposits for other Users");
            }

            user.Deposit = 0;
            _context.SaveChanges();

            return Ok("Deposit has been reset.");
        }

    }
}
