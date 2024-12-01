using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthenticationService.dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{

    private string _jwtKey;
    private string _jwtIssuer;
    
    public AuthenticationController(Settings config)
    {

        _jwtKey = config.JwtKey ?? throw new ArgumentNullException(nameof(config.JwtKey));
        _jwtIssuer = config.JwtIssuer ?? throw new ArgumentNullException(nameof(config.JwtIssuer));
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> RegisterUser([FromBody] UserCredentials userCredentials)
    {
        
        // For development purposes the user credentials will not be checked - we just return a valid token
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            _jwtIssuer,
            _jwtIssuer,
            null,
            expires: DateTime.Now.AddHours(12),
            signingCredentials: credentials
        );
    
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        return Ok(token);
    }
}
