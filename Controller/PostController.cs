using DotnetAPI.Dtos;
using DotnetAPI.Models;
using DOTNETAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class PostController : ControllerBase {

        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }
        
        [Authorize]
        [HttpGet("Posts")]

        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
            [PostUpdated] FROM TutorialAppSchema.Posts";

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]

        public IEnumerable<Post> GetPostsSingle(int postId)
        {
            string sql = @"SELECT [PostId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
            [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE PostId = " + postId.ToString();

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("GetPostsByUser/{userId}")]

        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = @"SELECT [userId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
            [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE PostId = " + userId.ToString();

            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"SELECT [userId],
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
            [PostUpdated] FROM TutorialAppSchema.Posts
                WHERE PostId = " + this.User.FindFirst("userId")?.Value;

            return _dapper.LoadData<Post>(sql);
        }


        [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sql = @"
            INSERT INTO TutorialAppSchema.Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]) VALUES (" + this.User.FindFirst("userId")?.Value
                + ",'" + postToAdd.PostTitle
                + "','" + postToAdd.PostContent
                + "', GETDATE(), GETDATE() )";
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to create new post!");
        }

        [HttpPut("Post")]

        public IActionResult EditPost(PostToAddDto postToEdit)
        {
            string sql =@"
            UPDATE TutorialAppSchema.Post 
                SET PostContent = '" + postToEdit.PostContent + "', PostTitle = '" + postToEdit.PostTitle + @"', PostUpdated = GETDATE()
                WHERE PostId = " + postToEdit.PostId.ToString() + 
                "AND UserId = " + this.User.FindFirst("userId")?.Value;
               
                if(_dapper.ExecuteSql(sql))
                {
                    return Ok();
                }         
                throw new Exception("Failed to update new post!");            
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Posts 
                WHERE PostId = " + postId.ToString()+
                    "AND UserId = " + this.User.FindFirst("userId")?.Value;

            
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }
    }
}