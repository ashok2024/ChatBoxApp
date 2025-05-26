using System.Text.Json;
using ChatApp.Api.Services;
using ChatApp.API.Data;
using ChatApp.API.Dtos;
using ChatApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IGrouupService _groupService;

    public GroupsController(AppDbContext context, IGrouupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGroup([FromForm] string groupName, [FromForm] string members, [FromForm] IFormFile? image)
    {
        CreateGroupDto dto = new CreateGroupDto();
        if (string.IsNullOrWhiteSpace(groupName) || members == null)
            return BadRequest("Group name and members are required.");

        List<string> memberList;
        try
        {
            memberList = JsonSerializer.Deserialize<List<string>>(members);
            dto.Members = memberList;
            dto.GroupName = groupName;
            

            return Ok(await _groupService.CreateGroup(dto, image));
        }
        catch
        {
            return BadRequest("Invalid members format.");
        }


    }
    [HttpGet("groups")]
    public async Task<IActionResult> GetUserGroups([FromQuery] string userEmail)
    {
        if (userEmail == null)
            return Unauthorized();

        var groups = await _groupService.GetGroupsForUserAsync(userEmail);
        var GroupsData = groups.Select(g => new
        {
            g.Id,
            g.GroupName,
            g.MembersEmails,
            g.CreatedAt,
            ImagePath = g.ImagePath
        }).ToList();

        return Ok(GroupsData);
    }
}
