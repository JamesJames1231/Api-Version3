using Microsoft.AspNetCore.Mvc;
using Api_Version3.Models;
using Api_Version3.Structure.Users;

namespace Api_Version3.Controllers
{
    [ApiController]
    [Route("v1/user")]
    public class UserController : Controller
    {
        [HttpPost("add-user")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public IActionResult addUser([FromBody] CreateUser CU)
        {
            if (CU.ReadyStatus())
            {
                string response = UserManagment.UserCreate(CU);
                if(response != "Success: Account Created.")
                {
                    return BadRequest(response);
                }
                else { return Ok(response); }   
            }
            else { return BadRequest(); }
        }

        [HttpPost("login-user")]
        [ProducesResponseType<LoggedInUser>(StatusCodes.Status200OK)]
        public IActionResult loginUser([FromBody] LoginUser LU)
        {
            if (LU.ReadyStatus())
            {
                LoggedInUser response = UserManagment.UserLogin(LU);
                if (response.ID == -1) { return BadRequest("Username or Password Incorrect"); }
                else if(response.ID == -2) { return BadRequest("Database Error: Please Try Again."); }
                else { return Ok(response); }
            }
            else { return BadRequest(); }
        }

        [HttpDelete("delete-user")]
        public IActionResult deleteUser([FromBody] LoggedInUser LIU)
        {
            if (LIU.ReadyStatus())
            {
                LoginUser LU = new LoginUser();
                LU.Username = LIU.Username;
                LU.Password = LIU.Password;
                LoggedInUser response = UserManagment.UserLogin(LU);
                if(response.ID != -1 && response.ID != -2)
                {
                    int deleteResp = UserManagment.UserDelete(LIU);
                    if (deleteResp == 0) { return Ok("Deleted Succesfully."); }
                    else if (deleteResp == 1) { return BadRequest("Database Error: Please Try Again."); }
                    else { return BadRequest("System Error: ID Not Correct; Please Log Out and Try Again."); }
                }
                else { return BadRequest("Password Incorrect."); }
            }
            else { return BadRequest(); }
        }

        [HttpPatch("update-user")]
        public IActionResult updateUser([FromBody] UpdateDetails UD)
        {
            int resp = UD.UpdateCounter();
            if (UD.ReadyStatus() == true && resp > 0)
            {
                LoginUser LU = new LoginUser();
                LU.Username = UD.Username;
                LU.Password = UD.Password;
                LoggedInUser response = UserManagment.UserLogin(LU);
                if (response.ID != -1 && response.ID != -2)
                {
                    int updateResp = UserManagment.UserUpdate(UD);
                    if (updateResp == 0) { return Ok("Account Updated."); }
                    else { return BadRequest("Database Error: Please Try Again."); }
                }
                else { return BadRequest("Password Incorrect."); }
            }
            else { return BadRequest(); }
        }
    }
}
