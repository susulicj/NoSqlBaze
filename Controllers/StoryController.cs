namespace napredneBaze.Controllers;
using napredneBaze.Models;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;

[ApiController]
[Route("[controller]")]
public class StoryController : ControllerBase
{
    private readonly IGraphClient _client;

    public StoryController(IGraphClient client)
    {
        _client = client;
    }


    [Route("createStory/{userId}/{tekst}")]
    [HttpPost]
    public async Task<IActionResult> CreateStory(string userId, string tekst)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("Invalid userId");
        }

        
        string storyText = tekst; 
      
        if (string.IsNullOrEmpty(storyText))
        {
            return BadRequest("Story content is required");
        }

        
        Story story = new Story
        {
            Creator = userId,
            Url = storyText, 
            Id = Guid.NewGuid(),
            DateTimeCreated = DateTime.Now
        };
        story.Creator = userId;
        story.Url = tekst;

        story.Id = Guid.NewGuid();
        story.DateTimeCreated = DateTime.Now;

        await _client.Cypher
            .Merge("(s:Story { Id: $storyId })")
            .OnCreate()
            .Set("s = $story")
            .WithParams(new { storyId = story.Id, story })
            .ExecuteWithoutResultsAsync();

        await _client.Cypher
            .Match("(usr:User)", "(s:Story)")
            .Where((User usr) => usr.Id == userId)
            .AndWhere((Story s) => s.Id == story.Id)
            .Merge("(usr)-[:Published]->(s)")
            .ExecuteWithoutResultsAsync();

        return Ok(new { success = true, message = "Story created successfully" });
    }

    [Route("updateStory/{storyId}/{newText}")]
    [HttpPut]
    public async Task<IActionResult> UpdateStory(string storyId, string newText)
    {
        try
        {
            if (string.IsNullOrEmpty(storyId) || string.IsNullOrEmpty(newText))
            {
                return BadRequest("Invalid storyId or newText");
            }

            var storyExists = await _client.Cypher
                .Match("(s:Story { Id: $storyId })")
                .WithParam("storyId", storyId)
                .Return(s => s.As<Story>())
                .ResultsAsync;

            if (!storyExists.Any())
            {
                return NotFound("Story not found");
            }

            await _client.Cypher
                .Match("(s:Story { Id: $storyId })")
                .WithParam("storyId", storyId)
                .Set("s.Url = $newText")
                .WithParam("newText", newText)
                .ExecuteWithoutResultsAsync();

            return Ok(new { success = true, message = "Story updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }


    [Route("deleteStory/{storyId}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteStory(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            return BadRequest("Invalid storyId");
        }

        var session = _client.Cypher;

       
        await session
            .Match("(usr:User)-[p:Published]->(str:Story)")
            .Where((Story str) => str.Id.ToString() == storyId)
            .Delete("p")
            .ExecuteWithoutResultsAsync();

       
        await session
            .Match("(:User)-[likedRel:Liked]->(s:Story)")
            .Where((Story s) => s.Id.ToString() == storyId)
            .Delete("likedRel")
            .ExecuteWithoutResultsAsync();

      
        await session
            .Match("(:User)-[partOfRel:PART_OF_HIGHLIGHTS]->(str:Story)")
            .Where((Story str) => str.Id.ToString() == storyId)
            .Delete("partOfRel, str")
            .ExecuteWithoutResultsAsync();

      
        await session
            .Match("(str:Story)")
            .Where((Story str) => str.Id.ToString() == storyId)
            .Delete("str")
            .ExecuteWithoutResultsAsync();

        return Ok();
    }



    [Route("viewStory/{userId}/{storyId}")]
    [HttpPost]
    public async Task<IActionResult> ViewStory(string userId, string storyId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(storyId))
        {
            return BadRequest("Invalid userId or storyId");
        }

        var query = _client.Cypher
            .Match("(usr:User)", "(s:Story)")
            .Where((User usr) => usr.Id == userId)
            .AndWhere((Story s) => s.Id.ToString() == storyId)
            .Create("(usr)-[:Viewed]->(s)");

        await query.ExecuteWithoutResultsAsync();

        return Ok();
    }


    [Route("getLikesCount/{storyId}")]
    [HttpGet]
    public async Task<IActionResult> GetLikesCount(string storyId)
    {
        if (string.IsNullOrEmpty(storyId))
        {
            return BadRequest("Invalid storyId");
        }

        var likesCount = await _client.Cypher
            .Match("(s:Story)")
            .Where((Story s) => s.Id.ToString() == storyId)
            .Return(s => s.As<Story>().NumLikes)
            .ResultsAsync;

        if (likesCount.Any())
        {
            return Ok(likesCount.First());
        }

        return NotFound("Story not found");
    }




    [Route("getAllStories/{userId}")]
    [HttpGet]
    public async Task<IActionResult> GetAllStories(string userId)
    {
        try
        {
            var userStories = await _client.Cypher
                .Match("(u:User)-[:Published]->(s:Story)")
                .Where((User u) => u.Id == userId)
                .Return(s => s.CollectAs<Story>())
                .ResultsAsync;

            return Ok(userStories);
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }


    [Route("likeStory/{storyId}/{userId}")]
    [HttpPost]
    public async Task<IActionResult> LikeStory(string storyId, string userId)
    {
        if (string.IsNullOrEmpty(storyId) || string.IsNullOrEmpty(userId))
        {
            return BadRequest("Invalid storyId or userId");
        }

        var storyExists = await _client.Cypher
            .Match("(s:Story { Id: $storyId })")
            .WithParam("storyId", storyId)
            .Return(s => s.As<Story>())
            .ResultsAsync;

        if (!storyExists.Any())
        {
            return NotFound("Story not found");
        }

        var userExists = await _client.Cypher
            .Match("(u:User { Id: $userId })")
            .WithParam("userId", userId)
            .Return(u => u.As<User>())
            .ResultsAsync;

        if (!userExists.Any())
        {
            return NotFound("User not found");
        }

        var areFriends = await _client.Cypher
        .Match("(u1:User)-[:je_prijatelj]->(u2:User)")
        .Where((User u1) => u1.Id == userId)
        .OptionalMatch("(s:Story { Id: $storyId, Creator: u2.Id })<-[:Published]-(u2)")
        .WithParams(new { storyId })
        .With("COUNT(u2) AS friendCount")
        .Return<bool>("friendCount > 0")
        .ResultsAsync;

        bool areFriendsResult = areFriends.FirstOrDefault();

        if (!areFriendsResult)
        {
            return BadRequest("User and the creator of the story are not friends");
        }




        var likedRelationshipExists = await _client.Cypher
        .Match("(usr:User)-[:Liked]->(s:Story)")
        .Where((User usr) => usr.Id == userId)
        .AndWhere((Story s) => s.Id.ToString() == storyId)
        .Return(usr => usr.As<User>())
        .ResultsAsync;

        if (likedRelationshipExists.Any())
        {
            var response = new
            {
                errorMessage = "User already liked the story"
            };

           
            return new JsonResult(response)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        await _client.Cypher
            .Match("(usr:User { Id: $userId })", "(s:Story { Id: $storyId })")
            .WithParams(new { userId, storyId })
            .Merge("(usr)-[:Liked]->(s)")
            .ExecuteWithoutResultsAsync();

        await _client.Cypher
            .Match("(s:Story { Id: $storyId })")
            .WithParam("storyId", storyId)
            .Set("s.NumLikes = s.NumLikes + 1")
            .ExecuteWithoutResultsAsync();


        var likedStories = await _client.Cypher
            .Match("(s:Story { Id: $storyId })")
            .WithParam("storyId", storyId)
            .Return(s => s.As<Story>())
            .ResultsAsync;

        var likedStory = likedStories.FirstOrDefault(); 

        if (likedStory != null)
        {
            return Ok(likedStory);
        }
        else
        {
            return NotFound("Story not found after liking");
        }
    }


    [Route("unlikeStory/{storyId}/{userId}")]
    [HttpPost]
    public async Task<IActionResult> UnlikeStory(string storyId, string userId)
    {
        if (string.IsNullOrEmpty(storyId) || string.IsNullOrEmpty(userId))
        {
            return BadRequest("Invalid storyId or userId");
        }

        var storyExists = await _client.Cypher
            .Match("(s:Story)")
            .Where((Story s) => s.Id.ToString() == storyId)
            .Return(s => s.As<Story>())
            .ResultsAsync;

        if (!storyExists.Any())
        {
            return NotFound("Story not found");
        }

        var userExists = await _client.Cypher
            .Match("(u:User)")
            .Where((User u) => u.Id == userId)
            .Return(u => u.As<User>())
            .ResultsAsync;

        if (!userExists.Any())
        {
            return NotFound("User not found");
        }

        var likedRelationshipExists = await _client.Cypher
            .Match("(usr:User)-[:Liked]->(s:Story)")
            .Where((User usr) => usr.Id == userId)
            .AndWhere((Story s) => s.Id.ToString() == storyId)
            .Return(usr => usr.As<User>())
            .ResultsAsync;

        if (!likedRelationshipExists.Any())
        {
            return BadRequest("User has not liked the story");
        }
        await _client.Cypher
            .Match("(usr:User)-[r:Liked]->(s:Story)")
            .Where((User usr) => usr.Id == userId)
            .AndWhere((Story s) => s.Id.ToString() == storyId)
            .Delete("r")
            .ExecuteWithoutResultsAsync();

        await _client.Cypher
            .Match("(s:Story)")
            .Where((Story s) => s.Id.ToString() == storyId)
            .Set("s.NumLikes = CASE WHEN s.NumLikes > 0 THEN s.NumLikes - 1 ELSE 0 END")
            .ExecuteWithoutResultsAsync();

        var response = new
        {
            message = $"User {userId} unliked the story {storyId}"
        };

        
        return new JsonResult(response)
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    [Route("getStoriesByHighlightId/{highlightId}")]
    [HttpGet]
    public async Task<IActionResult> GetStoriesByHighlightId(string highlightId)
    {
        if (string.IsNullOrEmpty(highlightId))
        {
            return BadRequest("Invalid highlightId");
        }

        var storiesQuery = _client.Cypher
            .Match("(h:Highlight { Id: $highlightId })<-[:PART_OF_HIGHLIGHT]-(s:Story)")
            .WithParam("highlightId", highlightId)
            .Return(s => s.As<Story>());

        var stories = await storiesQuery.ResultsAsync;

        return Ok(stories);
    }


}