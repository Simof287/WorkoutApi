using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkoutApi.DTOs;
using WorkoutApi.Models;
using WorkoutApi.Options;

namespace WorkoutApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtOptions jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtOptions = jwtOptions;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponseDto>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                new RegisterResponseDto
                {
                    Message = "Account created. You can now log in."
                });
        }

        if (result.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return Problem(
                type: "urn:workout-api:problems:conflict",
                title: "Account already exists",
                detail: "The email address or username is already in use.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var errors = result.Errors
            .GroupBy(error => error.Code.StartsWith(
                "Password",
                StringComparison.Ordinal)
                ? "password"
                : error.Code.StartsWith(
                    "InvalidEmail",
                    StringComparison.Ordinal)
                    ? "email"
                    : "registration")
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());

        var validationProblem = new ValidationProblemDetails(errors)
        {
            Type = "urn:workout-api:problems:validation-error",
            Title = "Registration validation failed",
            Status = StatusCodes.Status400BadRequest,
            Instance = HttpContext.Request.Path
        };
        validationProblem.Extensions["traceId"] = HttpContext.TraceIdentifier;

        return BadRequest(validationProblem);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized();

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
            return Unauthorized();

        var expiresAt = DateTime.UtcNow.AddMinutes(
            _jwtOptions.TokenLifetimeMinutes);

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return Ok(new LoginResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        });
    }
}
