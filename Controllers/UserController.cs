using Microsoft.AspNetCore.Mvc;
using systrack_api.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SystrackApi.Data;
using Microsoft.AspNetCore.Authorization;

namespace systrack_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllAccounts()
        {
            var users = await _context.Accounts
                                      .Include(u => u.Customer)
                                      .ToListAsync();
            return users;
        }

        // GET api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {

            var user = await _context.Accounts
                                     .Include(u => u.Customer)
                                     .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Accounts.Any(u => u.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Accounts.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Ein Benutzer mit dieser E-Mail existiert bereits.");

            var user = new User
            {
                Email = model.Email,
                Password_Hash = HashPassword(model.Password),
                Role = Role.Customer
            };

            _context.Accounts.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrierung erfolgreich" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !VerifyPasswordHash(model.Password, user.Password_Hash))
            {
                return Unauthorized("Anmeldeinformationen sind ungültig.");
            }
            var token = GenerateJwtToken(user);
            return Ok(new { token, userId = user.Id, role = user.Role.ToString() });
        }

        [Authorize]
        [HttpPost("personal")]
        public async Task<IActionResult> AddOrUpdateCustomerData([FromBody] Customer customerData)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Benutzer ist nicht autorisiert.");
                }

                int userId = int.Parse(userIdClaim.Value);
                var user = await _context.Accounts.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User nicht gefunden.");
                }

                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (existingCustomer != null)
                {
                    existingCustomer.FirstName = customerData.FirstName;
                    existingCustomer.LastName = customerData.LastName;
                    existingCustomer.Dob = customerData.Dob;
                }
                else
                {
                    customerData.UserId = userId;
                    _context.Customers.Add(customerData);
                }

                await _context.SaveChangesAsync();
                return Ok("Daten erfolgreich gespeichert.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("address")]
        public async Task<IActionResult> AddOrUpdateAddressData([FromBody] Customer addressData)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Benutzer ist nicht autorisiert.");
                }

                int userId = int.Parse(userIdClaim.Value);
                var user = await _context.Accounts.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User nicht gefunden.");
                }

                var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (existingCustomer != null)
                {
                    existingCustomer.Country = addressData.Country;
                    existingCustomer.City = addressData.City;
                    existingCustomer.Zip = addressData.Zip;
                    existingCustomer.Street = addressData.Street;
                    existingCustomer.StreetNo = addressData.StreetNo;
                }
                else
                {
                    var newCustomer = new Customer
                    {
                        UserId = userId,
                        Country = addressData.Country,
                        City = addressData.City,
                        Zip = addressData.Zip,
                        Street = addressData.Street,
                        StreetNo = addressData.StreetNo
                    };
                    _context.Customers.Add(newCustomer);
                }

                await _context.SaveChangesAsync();
                return Ok("Adressdaten erfolgreich gespeichert.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ein Fehler ist aufgetreten: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("data/{userId}")]
        public async Task<IActionResult> GetUserData(int userId)
        {
            var user = await _context.Accounts.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User nicht gefunden.");
            }

            var customerData = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customerData == null)
            {
                return NotFound("Kundendaten nicht gefunden.");
            }

            return Ok(customerData);
        }

        private static string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("1z/uLrPhxqSBMfgArAQpslMwlbOAUVdVU3PB1onVDKc=");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string HashPassword(string? password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPasswordHash(string? password, string? storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
