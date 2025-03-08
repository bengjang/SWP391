//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Oauth2.v2;
//using Google.Apis.Oauth2.v2.Data;
//using Google.Apis.Services;
//using lamlai.Models; // Cập nhật namespace cho phù hợp

//namespace test2.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LoginGoogleController : ControllerBase
//    {
//        private readonly TestContext _context; // Cập nhật tên context
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<LoginGoogleController> _logger;

//        public LoginGoogleController(TestContext context, IConfiguration configuration, ILogger<LoginGoogleController> logger)
//        {
//            _context = context;
//            _configuration = configuration;
//            _logger = logger;
//        }

//        public class GoogleLoginRequest
//        {
//            public string? AccessToken { get; set; }
//        }

//        [HttpPost("google-login")]
//        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
//        {
//            try
//            {
//                if (string.IsNullOrEmpty(request.AccessToken))
//                {
//                    return BadRequest("Access token is required");
//                }
//                _logger.LogInformation($"Received Access token: {request.AccessToken}");

//                // Validate the access token and get user info
//                var userInfoClient = new Oauth2Service(new BaseClientService.Initializer
//                {
//                    HttpClientInitializer = GoogleCredential.FromAccessToken(request.AccessToken)
//                });
//                var userInfo = await userInfoClient.Userinfo.Get().ExecuteAsync();

//                // Tạo đối tượng GoogleUserInfo từ thông tin người dùng
//                var googleUser = new GoogleUserInfo
//                {
//                    Email = userInfo.Email,
//                    Name = userInfo.Name,
//                    Picture = userInfo.Picture,
//                    Id = userInfo.Id
//                };

//                // Tìm người dùng trong cơ sở dữ liệu
//                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == googleUser.Email);

//                if (user == null)
//                {
//                    // Người dùng không tồn tại, trả về thông tin để frontend xử lý
//                    return Ok(new
//                    {
//                        IsNewUser = true,
//                        Email = googleUser.Email,
//                        Name = googleUser.Name
//                    });
//                }

//                // Người dùng đã tồn tại, tạo token và trả về như bình thường
//                var token = GenerateJwtToken(user);
//                return Ok(new
//                {
//                    IsNewUser = false,
//                    Token = token,
//                    User = new { user.UserId, user.Name, user.Email, user.Role }
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred during Google authentication");
//                return StatusCode(500, $"An error occurred during authentication: {ex.Message}");
//            }
//        }

//        [HttpPost("complete-registration")]
//        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
//        {
//            try
//            {
//                var user = new User
//                {
//                    Email = request.Email,
//                    FullName = request.FullName,
//                    Name = request.Email.Split('@')[0],
//                    Role = "Costumer",
//                    Phone = request.Phone,
//                    Address = request.Address,
//                    Password = "GoogleLogin",
//                    RegistrationDate = DateTime.Now
//                };
//                _context.Users.Add(user);
//                await _context.SaveChangesAsync();

//                var token = GenerateJwtToken(user);
//                return Ok(new
//                {
//                    Token = token,
//                    User = new { user.UserId, user.FullName, user.Name, user.Email, user.Role }
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "An error occurred during user registration");
//                return StatusCode(500, $"An error occurred during registration: {ex.Message}");
//            }
//        }

//        public class CompleteRegistrationRequest
//        {
//            public string Email { get; set; }
//            public string FullName { get; set; }
//            public string Phone { get; set; }
//            public string Address { get; set; }
//        }

//        private string GenerateJwtToken(User user)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[] 
//                {
//                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
//                    new Claim(ClaimTypes.Email, user.Email),
//                    new Claim(ClaimTypes.Name, user.Name),
//                }),
//                Expires = DateTime.UtcNow.AddDays(7),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//            };
//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            return tokenHandler.WriteToken(token);
//        }
//    }
//} 