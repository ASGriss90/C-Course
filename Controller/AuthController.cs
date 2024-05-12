using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DOTNETAPI;
using DOTNETAPI.Dtos;
using DOTNETAPI.helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DOTNETAPI.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class AuthController : ControllerBase
    {

        private readonly DataContextDapper _dapper;

        private  readonly AuthHelpers _authHelper;


        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelpers(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDtos userForRegistration){

            if(userForRegistration.Password == userForRegistration.PasswordConfirm){

                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if(existingUsers.Count() == 0)
                {

                    byte[] passwordSalt = new byte[128/8];
                    using(RandomNumberGenerator rng = RandomNumberGenerator.Create())   
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }
                
                     byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);
                   
                    string sqlAddAuth = @"
                    INSERT INTO TutorialAppSchema.Auth ([Email], [PasswordHash],[PasswordSalt]) VALUES('" + userForRegistration.Email + 
                    "', @PasswordHash, @PasswordSalt)";

                    List<SqlParameter>  sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", System.Data.SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;


                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", System.Data.SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if(_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters)){
                         string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            )VALUES(" +
                            "'"   + userForRegistration.FirstName +
                            "','" + userForRegistration.LastName +
                            "','" + userForRegistration.Email +
                            "','" + userForRegistration.Gender +
                            "',1)";
                    
                    if(_dapper.ExecuteSql(sqlAddUser)){
                                return Ok();
                            }
                       throw new Exception("Failed to add user");
                    }
                   throw new Exception("Failed to register user");
                }
                 throw new Exception("User with this email already exist");
            }
            throw new Exception("Passwords do not match");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
         public IActionResult Login(UserForLoginDto userForLogin){

            string sqlForHashAndSalt = @"SELECT [PasswordHash],[PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = '" + userForLogin.Email + "'";
            UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for( int index = 0; index < passwordHash.Length; index++)
            {
                if(passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password");
                }
            }

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" + userForLogin.Email +
            "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string>{
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [AllowAnonymous]
        [HttpGet("RefreshToken")]

        public string RefreshToken(){

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" + User.FindFirst("UserId")?.Value +
            "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);
            
            return _authHelper.CreateToken(userId);



            
        }    

    }
}
