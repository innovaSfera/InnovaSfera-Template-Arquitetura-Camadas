using System.Text;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Interfaces.Storage;
using Microsoft.AspNetCore.Mvc;

namespace DomainDriveDesign.Presentation.Api.Controllers;

/// <summary>
/// Controller Sample
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class SampleController : ControllerBase
{
    private readonly ISampleDataAppService _service;
    private readonly IStorageContext _storageContext;
    private readonly ILogger<SampleController> _logger;

    public SampleController(
        ISampleDataAppService service,
        IStorageContext storageContext,
        ILogger<SampleController> logger)
    {
        _service = service;
        _storageContext = storageContext;
        _logger = logger;
    }

    [HttpGet(Name = "GetSampleData")]
    public async Task<ActionResult<IEnumerable<SampleDataDto>>> Get()
    {
        var result = await _service.GetAllAsync();

        if (result == null || !result.Any())
            return NoContent();

        return Ok(result);
    }

    [HttpPost(Name = "AddSampleData")]
    public async Task Add(SampleDataDto sampleData)
    {
        await _service.AddAsync(sampleData);
    }

    [HttpGet("files", Name = "GetFiles")]
    public async Task<ActionResult<IEnumerable<string>>> GetFiles()
    {
        var result = await _service.GetFilesAsync();

        if (result == null || !result.Any())
            return NoContent();

        return Ok(result);
    }

    [HttpGet("wizards", Name = "GetWizards")]
    public async Task<ActionResult<ICollection<CharacterDtoResponse>>> GetWizards()
    {
        var wizards = await _service.GetAllWizardsAsync();

        if (wizards == null || !wizards.Any())
            return NoContent();

        return Ok(wizards);
    }

    [HttpPost("storage/test-file")]
    public async Task<ActionResult<string>> CreateTestFile([FromBody] string content)
    {
        try
        {
            var fileName = $"test-{DateTime.UtcNow:yyyyMMddHHmmss}.txt";
            var fileContent = Encoding.UTF8.GetBytes(content ?? "Default test content");
            
            await _storageContext.WriteFileAsync(fileName, fileContent);
            
            var storageType = _storageContext.GetStorageType();
            
            return Ok(new { 
                Message = "File created successfully", 
                FileName = fileName,
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test file");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("storage/files")]
    public async Task<ActionResult<object>> GetStorageFiles([FromQuery] string path = "")
    {
        try
        {
            var files = await _storageContext.GetFilesAsync(path);
            var storageType = _storageContext.GetStorageType();
            
            return Ok(new { 
                Files = files,
                StorageType = storageType,
                Path = path
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage files");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("storage/file/{fileName}")]
    public async Task<ActionResult<string>> GetFileContent(string fileName)
    {
        try
        {
            if (!await _storageContext.FileExistsAsync(fileName))
                return NotFound($"File '{fileName}' not found");

            var fileContent = await _storageContext.ReadFileAsync(fileName);
            var content = Encoding.UTF8.GetString(fileContent);
            var storageType = _storageContext.GetStorageType();
            
            return Ok(new { 
                FileName = fileName,
                Content = content,
                StorageType = storageType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file {FileName}", fileName);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("storage/file/{fileName}")]
    public async Task<ActionResult> DeleteFile(string fileName)
    {
        try
        {
            if (!await _storageContext.FileExistsAsync(fileName))
                return NotFound($"File '{fileName}' not found");

            await _storageContext.DeleteFileAsync(fileName);
            
            return Ok(new { Message = $"File '{fileName}' deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
