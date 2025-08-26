using System.ComponentModel.DataAnnotations;
using DomainDrivenDesign.Application.Entities;
using DomainDrivenDesign.Application.Interfaces;
using DomainDriveDesign.Presentation.Api.Controllers;
using FluentValidation;
using FluentValidation.Results;
using InnovaSfera.Template.Application.Dto.Request;
using InnovaSfera.Template.Application.Dto.Response;
using InnovaSfera.Template.Domain.Entities.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace InnovaSfera.Template.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for SampleController following AAA pattern (Arrange, Act, Assert)
/// Targeting 80%+ code coverage with mocked dependencies
/// </summary>
public class SampleControllerTest
{
    private readonly Mock<ISampleDataAppService> _sampleDataAppServiceMock;
    private readonly Mock<IStorageAppService> _storageAppServiceMock;
    private readonly Mock<IMessagingAppService> _messagingAppServiceMock;
    private readonly Mock<ILogger<SampleController>> _loggerMock;
    private readonly Mock<IValidator<SampleDataDto>> _validatorMock;
    private readonly SampleController _controller;

    public SampleControllerTest()
    {
        // Arrange - Setup mocks
        _sampleDataAppServiceMock = new Mock<ISampleDataAppService>();
        _storageAppServiceMock = new Mock<IStorageAppService>();
        _messagingAppServiceMock = new Mock<IMessagingAppService>();
        _loggerMock = new Mock<ILogger<SampleController>>();
        _validatorMock = new Mock<IValidator<SampleDataDto>>();

        _controller = new SampleController(
            _sampleDataAppServiceMock.Object,
            _storageAppServiceMock.Object,
            _messagingAppServiceMock.Object,
            _loggerMock.Object,
            _validatorMock.Object);

        // Setup controller context for ModelState
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region Sample Data Operations Tests

    [Fact]
    public async Task GetAsync_WhenSampleDataExists_ShouldReturnOkWithData()
    {
        // Arrange
        var expectedData = new List<SampleDataDto>
        {
            new() { Id = Guid.NewGuid(), Message = "Test Message 1", TimeStamp = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Message = "Test Message 2", TimeStamp = DateTime.UtcNow }
        };

        _sampleDataAppServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualData = Assert.IsType<List<SampleDataDto>>(okResult.Value);
        Assert.Equal(expectedData.Count, actualData.Count);
        Assert.Equal(expectedData.First().Message, actualData.First().Message);
    }

    [Fact]
    public async Task GetAsync_WhenNoSampleDataExists_ShouldReturnNoContent()
    {
        // Arrange
        _sampleDataAppServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<SampleDataDto>());

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetAsync_WhenSampleDataIsNull_ShouldReturnNoContent()
    {
        // Arrange
        _sampleDataAppServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<SampleDataDto>());

        // Act
        var result = await _controller.GetAsync();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task PostAsync_WhenValidData_ShouldReturnOkWithEventResult()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var request = new SampleDataDto { Id = testId, Message = "Test Message", TimeStamp = DateTime.UtcNow };
        var addedData = new SampleDataDto { Id = testId, Message = "Test Message", TimeStamp = DateTime.UtcNow };
        var eventResult = MessageResult.Successful("123", "TestProvider");

        _validatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        _sampleDataAppServiceMock
            .Setup(x => x.AddAsync(request))
            .ReturnsAsync((addedData, eventResult));

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SendMessageDtoResponse>(okResult.Value);
        Assert.Equal("Sample data created successfully", response.Message);
        Assert.Equal(addedData.Id, response.SampleId);
        Assert.True(response.EventSent);
        Assert.Equal(eventResult.Provider, response.MessagingProvider);
    }

    [Fact]
    public async Task PostAsync_WhenValidationFails_ShouldReturnBadRequest()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var request = new SampleDataDto { Id = testId, Message = "", TimeStamp = DateTime.UtcNow };
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Message", "Message is required")
        });

        _validatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task PostAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var request = new SampleDataDto { Id = testId, Message = "Test", TimeStamp = DateTime.UtcNow };
        _controller.ModelState.AddModelError("TestError", "Test error message");

        _validatorMock
            .Setup(x => x.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _controller.PostAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetFilesAsync_WhenFilesExist_ShouldReturnOkWithFiles()
    {
        // Arrange
        var files = new List<string> { "file1.txt", "file2.txt" };
        var eventResult = MessageResult.Successful("msg123", "TestProvider");

        _sampleDataAppServiceMock
            .Setup(x => x.GetFilesAsync())
            .ReturnsAsync((files, eventResult));

        // Act
        var result = await _controller.GetFilesAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FilesDtoResponse>(okResult.Value);
        Assert.Equal(files.Count, response.Count);
        Assert.True(response.EventSent);
    }

    [Fact]
    public async Task GetFilesAsync_WhenNoFilesExist_ShouldReturnNoContent()
    {
        // Arrange
        var files = new List<string>();
        var eventResult = MessageResult.Successful("msg123", "TestProvider");

        _sampleDataAppServiceMock
            .Setup(x => x.GetFilesAsync())
            .ReturnsAsync((files, eventResult));

        // Act
        var result = await _controller.GetFilesAsync();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetWizardsAsync_WhenWizardsExist_ShouldReturnOkWithWizards()
    {
        // Arrange
        var wizards = new List<CharacterDtoResponse>
        {
            new() { Name = "Harry Potter", House = "Gryffindor" },
            new() { Name = "Hermione Granger", House = "Gryffindor" }
        };

        _sampleDataAppServiceMock
            .Setup(x => x.GetAllWizardsAsync())
            .ReturnsAsync(wizards);

        // Act
        var result = await _controller.GetWizardsAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualWizards = Assert.IsType<List<CharacterDtoResponse>>(okResult.Value);
        Assert.Equal(wizards.Count, actualWizards.Count);
        Assert.Equal(wizards.First().Name, actualWizards.First().Name);
    }

    [Fact]
    public async Task GetWizardsAsync_WhenNoWizardsExist_ShouldReturnNoContent()
    {
        // Arrange
        _sampleDataAppServiceMock
            .Setup(x => x.GetAllWizardsAsync())
            .ReturnsAsync(new List<CharacterDtoResponse>());

        // Act
        var result = await _controller.GetWizardsAsync();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    #endregion

    #region Storage Operations Tests

    [Fact]
    public async Task CreateTestFileAsync_WhenValidContent_ShouldReturnOkWithFileInfo()
    {
        // Arrange
        const string content = "Test file content";
        const string fileName = "test-20230826120000.txt";
        const string storageType = "Local";

        _storageAppServiceMock
            .Setup(x => x.CreateTestFileAsync(content, default))
            .ReturnsAsync((fileName, storageType));

        // Act
        var result = await _controller.CreateTestFileAsync(content);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FileCreateDtoResponse>(okResult.Value);
        Assert.Equal("File created successfully", response.Message);
        Assert.Equal(fileName, response.FileName);
        Assert.Equal(storageType, response.StorageType);
    }

    [Fact]
    public async Task CreateTestFileAsync_WhenNullContent_ShouldReturnOkWithDefaultContent()
    {
        // Arrange
        const string fileName = "test-20230826120000.txt";
        const string storageType = "Local";

        _storageAppServiceMock
            .Setup(x => x.CreateTestFileAsync(null, default))
            .ReturnsAsync((fileName, storageType));

        // Act
        var result = await _controller.CreateTestFileAsync(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FileCreateDtoResponse>(okResult.Value);
        Assert.Equal(fileName, response.FileName);
    }

    [Fact]
    public async Task GetStorageFilesAsync_WhenFilesExist_ShouldReturnOkWithFiles()
    {
        // Arrange
        const string path = "test/path";
        var files = new List<string> { "file1.txt", "file2.txt" };
        const string storageType = "Local";

        _storageAppServiceMock
            .Setup(x => x.GetStorageFilesAsync(path, default))
            .ReturnsAsync((files, storageType));

        // Act
        var result = await _controller.GetStorageFilesAsync(path);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FilesDtoResponse>(okResult.Value);
        Assert.Equal(files.Count, response.Count);
        Assert.Equal(storageType, response.StorageType);
        Assert.Equal(path, response.Path);
    }

    [Fact]
    public async Task GetFileContentAsync_WhenFileExists_ShouldReturnOkWithContent()
    {
        // Arrange
        const string fileName = "test.txt";
        const string content = "File content";
        const string storageType = "Local";

        _storageAppServiceMock
            .Setup(x => x.FileExistsAsync(fileName, default))
            .ReturnsAsync(true);

        _storageAppServiceMock
            .Setup(x => x.GetFileContentAsync(fileName, default))
            .ReturnsAsync((content, storageType));

        // Act
        var result = await _controller.GetFileContentAsync(fileName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FilesDtoResponse>(okResult.Value);
        Assert.Equal(fileName, response.FileName);
        Assert.Equal(content, response.Content);
        Assert.Equal(storageType, response.StorageType);
    }

    [Fact]
    public async Task GetFileContentAsync_WhenFileNotExists_ShouldReturnNotFound()
    {
        // Arrange
        const string fileName = "nonexistent.txt";

        _storageAppServiceMock
            .Setup(x => x.FileExistsAsync(fileName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetFileContentAsync(fileName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFoundResult.Value);
        Assert.Contains(fileName, notFoundResult.Value.ToString()!);
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileExists_ShouldReturnOkWithSuccessMessage()
    {
        // Arrange
        const string fileName = "test.txt";
        const string storageType = "Local";

        _storageAppServiceMock
            .Setup(x => x.FileExistsAsync(fileName, default))
            .ReturnsAsync(true);

        _storageAppServiceMock
            .Setup(x => x.DeleteFileAsync(fileName, default))
            .ReturnsAsync(storageType);

        // Act
        var result = await _controller.DeleteFileAsync(fileName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Contains("deleted successfully", okResult.Value.ToString()!);
    }

    [Fact]
    public async Task DeleteFileAsync_WhenFileNotExists_ShouldReturnNotFound()
    {
        // Arrange
        const string fileName = "nonexistent.txt";

        _storageAppServiceMock
            .Setup(x => x.FileExistsAsync(fileName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFileAsync(fileName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
        Assert.Contains(fileName, notFoundResult.Value.ToString()!);
    }

    #endregion

    #region Messaging Operations Tests

    [Fact]
    public async Task SendCustomMessageAsync_WhenMessageSentSuccessfully_ShouldReturnOk()
    {
        // Arrange
        var request = new SendMessageDtoRequest
        {
            Topic = "test.topic",
            Payload = new { Test = "Data" },
            CorrelationId = "test-correlation"
        };

        var messageResult = MessageResult.Successful("msg-123", "TestProvider");

        _messagingAppServiceMock
            .Setup(x => x.SendCustomMessageAsync(
                request.Topic,
                request.Payload,
                request.CorrelationId,
                request.Headers,
                default))
            .ReturnsAsync(messageResult);

        // Act
        var result = await _controller.SendCustomMessageAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(messageResult.MessageId, response.MessageId);
        Assert.Equal(messageResult.Provider, response.Provider);
    }

    [Fact]
    public async Task SendCustomMessageAsync_WhenMessageFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new SendMessageDtoRequest
        {
            Topic = "test.topic",
            Payload = new { Test = "Data" }
        };

        var messageResult = MessageResult.Failed("Failed to send message", null, "TestProvider");

        _messagingAppServiceMock
            .Setup(x => x.SendCustomMessageAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                default))
            .ReturnsAsync(messageResult);

        // Act
        var result = await _controller.SendCustomMessageAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal(messageResult.ErrorMessage, response.Error);
    }

    [Fact]
    public async Task SendBatchSampleMessagesAsync_WhenAllMessagesSucceed_ShouldReturnOkWithSuccessResult()
    {
        // Arrange
        var testId1 = Guid.NewGuid();
        var testId2 = Guid.NewGuid();
        var sampleDataList = new List<SampleDataDto>
        {
            new() { Id = testId1, Message = "Test 1", TimeStamp = DateTime.UtcNow },
            new() { Id = testId2, Message = "Test 2", TimeStamp = DateTime.UtcNow }
        };

        var messageResults = new List<MessageResult>
        {
            MessageResult.Successful("msg-1", "TestProvider"),
            MessageResult.Successful("msg-2", "TestProvider")
        };

        _messagingAppServiceMock
            .Setup(x => x.SendBatchSampleMessagesAsync(sampleDataList, default))
            .ReturnsAsync(messageResults);

        // Act
        var result = await _controller.SendBatchSampleMessagesAsync(sampleDataList);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.TotalMessages);
        Assert.Equal(2, response.SuccessCount);
        Assert.Equal(0, response.FailureCount);
    }

    [Fact]
    public async Task CheckMessagingHealthAsync_WhenHealthy_ShouldReturnOkWithHealthyStatus()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        _messagingAppServiceMock
            .Setup(x => x.CheckHealthAsync(default))
            .ReturnsAsync((true, "TestProvider", timestamp));

        // Act
        var result = await _controller.CheckMessagingHealthAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Healthy", response.Status);
        Assert.Equal("TestProvider", response.Provider);
    }

    [Fact]
    public async Task CheckMessagingHealthAsync_WhenUnhealthy_ShouldReturnOkWithUnhealthyStatus()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        _messagingAppServiceMock
            .Setup(x => x.CheckHealthAsync(default))
            .ReturnsAsync((false, "TestProvider", timestamp));

        // Act
        var result = await _controller.CheckMessagingHealthAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(okResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Unhealthy", response.Status);
    }

    [Fact]
    public async Task SimulateDlqWithSampleAsync_WhenSuccessful_ShouldReturnOkWithDlqResult()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sampleData = new SampleDataDto { Id = testId, Message = "Test", TimeStamp = DateTime.UtcNow };
        var messageResult = MessageResult.Successful("dlq-msg-123", "TestProvider");

        _messagingAppServiceMock
            .Setup(x => x.SimulateDlqWithSampleAsync(sampleData, default))
            .ReturnsAsync(messageResult);

        // Act
        var result = await _controller.SimulateDlqWithSampleAsync(sampleData);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task SendSampleEventWithRetryAsync_WhenSuccessful_ShouldReturnOkWithRetryResult()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var sampleData = new SampleDataDto { Id = testId, Message = "Test", TimeStamp = DateTime.UtcNow };
        var messageResult = MessageResult.Successful("retry-msg-123", "TestProvider");

        _messagingAppServiceMock
            .Setup(x => x.SendSampleEventWithRetryAsync(sampleData, default))
            .ReturnsAsync(messageResult);

        // Act
        var result = await _controller.SendSampleEventWithRetryAsync(sampleData);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResultDtoResponse>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(messageResult.MessageId, response.MessageId);
        Assert.Contains("successfully", response.Message);
    }

    #endregion

    #region ModelState Validation Tests

    [Fact]
    public async Task GetFilesAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("TestError", "Test error message");

        // Act
        var result = await _controller.GetFilesAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetWizardsAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("TestError", "Test error message");

        // Act
        var result = await _controller.GetWizardsAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateTestFileAsync_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("TestError", "Test error message");

        // Act
        var result = await _controller.CreateTestFileAsync("test content");

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    #endregion
}
