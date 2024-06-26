using DOTNETAPI;
using Microsoft.AspNetCore.Mvc;

namespace DOTNET.Controllers;


[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase{

   DataContextDapper _dapper;
   public UserController(IConfiguration config){
        _dapper = new DataContextDapper(config);
    }
  
    [HttpGet("TestConnection")]

    public DateTime TestConnection(){
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }


   [HttpGet("GetUsers")]
   public IEnumerable<User> GetUsers(){

        string sql = @"SELECT [UserId],
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active] FROM TutorialAppSchema.Users";

        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    
   } 

   [HttpGet("GetSingleUser/{userId}")]
   public User GetSingleUser(int userId){

    string sql = @"SELECT [UserId],
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active] FROM TutorialAppSchema.Users
                            WHERE UserId = " + userId.ToString();

        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    
   } 


   [HttpGet("GetSalaries")]
   public IEnumerable<UserSalary> GetSalaries(){

    string sql = @"SELECT Salary FROM TutorialAppSchema.UserSalary";

    IEnumerable<UserSalary> salaries = _dapper.LoadData<UserSalary>(sql);

    return salaries;
   }

   [HttpGet("UsersJobInfo")]

   public IEnumerable<UsersJobInfo> GetJobsInfos(){

    string sql = @"SELECT JobTitle, Department FROM TutorialAppSchema.UserJobInfo";

    IEnumerable<UsersJobInfo> jobs = _dapper.LoadData<UsersJobInfo>(sql);

    return jobs;
   }


    [HttpPut("EditUser")]
    public IActionResult EditUser(User user){
            string sql = @"
            UPDATE TutorialAppSchema.Users
                SET
                [FirstName] = '" + user.FirstName + 
                "', [LastName] =  '" + user.LastName + 
                "', [Email] = '" + user.Email +
                "',[Gender] = '" + user.Gender +
                 "',[Active] = '" + user.Active + 
            "' WHERE UserId = " + user.UserId;

            if(_dapper.ExecuteSql<User>(sql)){
                return Ok();
            }
         throw new Exception("Failed to update user record");
    } 

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserDto user){
            string sql = @"INSERT INTO TutorialAppSchema.Users(
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        )VALUES(" +
          "'"   + user.FirstName +
          "','" + user.LastName +
          "','" + user.Email +
          "','" + user.Gender +
          "','" + user.Active +
        "')";

         if(_dapper.ExecuteSql<User>(sql)){
                return Ok();
            }
         throw new Exception("Failed to update user record");
        
    } 


    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId){
            string sql = @"
            DELETE FROM TutorialAppSchema.Users
                WHERE UserId = " + userId.ToString();

            if(_dapper.ExecuteSql<User>(sql)){
                return Ok();
            }
         throw new Exception("Failed to delete user record");
    } 











}