using Moq;
using SJ.BackEnd.Template.IRepository;
using SJ.BackEnd.Template.Services;

namespace SJ.BackEnd.Template.Tests.Services;

public class SysPositionServiceTests
{
    private readonly Mock<IBaseRepository<SysPosition>> _mockRepo;
    private readonly Mock<IBaseRepository<SysUserPosition>> _mockUserPositionRepo;
    private readonly ISysPositionService _service;

    public SysPositionServiceTests()
    {
        _mockRepo = new Mock<IBaseRepository<SysPosition>>();
        _mockUserPositionRepo = new Mock<IBaseRepository<SysUserPosition>>();
        _service = new SysPositionService(_mockRepo.Object, _mockUserPositionRepo.Object);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsPosition()
    {
        // Arrange
        var request = new CreatePositionRequest { Name = "软件工程师", Code = "SE", Description = "软件开发岗位" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.Insert(It.IsAny<SysPosition>()))
                 .ReturnsAsync(1L);
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPosition { Id = 1, Name = "软件工程师", Code = "SE" });

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("软件工程师", result.Name);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsNull()
    {
        // Arrange
        var request = new CreatePositionRequest { Name = "软件工程师", Code = "SE" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsNull()
    {
        // Arrange
        // Setup: first Exist (name check) returns false, second Exist (code check) returns true
        _mockRepo.SetupSequence(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(false)
                 .ReturnsAsync(true);

        var request = new CreatePositionRequest { Name = "软件工程师", Code = "SE" };

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsTrue()
    {
        // Arrange
        var request = new UpdatePositionRequest { Name = "高级软件工程师", Code = "SSE", Description = "高级开发岗位" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPosition { Id = 1, Name = "软件工程师", Code = "SE", Description = "" });
        _mockRepo.Setup(r => r.Update(It.IsAny<SysPosition>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Update(1, request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Update_WithNameUsedByOther_ReturnsFalse()
    {
        // Arrange
        var request = new UpdatePositionRequest { Name = "高级软件工程师", Code = "SSE" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Update(1, request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Update_NonExistentPosition_ReturnsFalse()
    {
        // Arrange
        var request = new UpdatePositionRequest { Name = "高级软件工程师", Code = "SSE" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync((SysPosition?)null);

        // Act
        var result = await _service.Update(999, request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Delete_NonSystemPosition_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPosition { Id = 1, Name = "测试岗位", IsSystem = false });
        _mockUserPositionRepo.Setup(r => r.Exist(
                It.IsAny<System.Linq.Expressions.Expression<Func<SysUserPosition, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.DeleteById(It.IsAny<object>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Delete(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Delete_WithUsers_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPosition { Id = 1, Name = "测试岗位", IsSystem = false });
        _mockUserPositionRepo.Setup(r => r.Exist(
                It.IsAny<System.Linq.Expressions.Expression<Func<SysUserPosition, bool>>>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Delete(1);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_SystemPosition_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPosition { Id = 2, Name = "系统内置岗位", IsSystem = true });

        // Act
        var result = await _service.Delete(2);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NonExistentPosition_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync((SysPosition?)null);

        // Act
        var result = await _service.Delete(999);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetPagedList_WithKeyword_FiltersByNameOrCode()
    {
        // Arrange
        var positions = new List<SysPosition>
        {
            new() { Id = 1, Name = "软件工程师", Code = "SE" },
            new() { Id = 2, Name = "高级软件工程师", Code = "SSE" }
        };
        var pageModel = new PageModel<SysPosition>(1, 2, 10, positions);
        _mockRepo.Setup(r => r.QueryPagedByExpression(
                It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
                 .ReturnsAsync(pageModel);

        // Act
        var result = await _service.GetPagedList(1, 10, "工程师");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.DataCount);
        Assert.Equal(2, result.Data?.Count);
    }

    [Fact]
    public async Task GetPagedList_WithEmptyKeyword_ReturnsAll()
    {
        // Arrange
        var positions = new List<SysPosition>
        {
            new() { Id = 1, Name = "软件工程师", Code = "SE" },
            new() { Id = 2, Name = "产品经理", Code = "PM" },
            new() { Id = 3, Name = "设计师", Code = "DES" }
        };
        var pageModel = new PageModel<SysPosition>(1, 3, 10, positions);
        _mockRepo.Setup(r => r.QueryPagedByExpression(
                It.IsAny<System.Linq.Expressions.Expression<Func<SysPosition, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
                 .ReturnsAsync(pageModel);

        // Act
        var result = await _service.GetPagedList(1, 10, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.DataCount);
        Assert.Equal(3, result.Data?.Count);
    }
}