using Moq;
using SJ.BackEnd.Template.IRepository;
using SJ.BackEnd.Template.Services;
using System.Linq.Expressions;

namespace SJ.BackEnd.Template.Tests.Services;

public class SysUserPositionServiceTests
{
    private readonly Mock<IBaseRepository<SysUserPosition>> _mockRepo;
    private readonly Mock<IBaseRepository<SysPosition>> _mockPositionRepo;
    private readonly Mock<IBaseRepository<SysUser>> _mockUserRepo;
    private readonly ISysUserPositionService _service;

    public SysUserPositionServiceTests()
    {
        _mockRepo = new Mock<IBaseRepository<SysUserPosition>>();
        _mockPositionRepo = new Mock<IBaseRepository<SysPosition>>();
        _mockUserRepo = new Mock<IBaseRepository<SysUser>>();
        _service = new SysUserPositionService(_mockRepo.Object, _mockPositionRepo.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task Bind_NewRelation_ReturnsTrue()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetById(It.IsAny<object>()))
                     .ReturnsAsync(new SysUser { Id = 1 });
        _mockPositionRepo.Setup(r => r.GetById(It.IsAny<object>()))
                         .ReturnsAsync(new SysPosition { Id = 2 });
        _mockRepo.Setup(r => r.Exist(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.Insert(It.IsAny<SysUserPosition>()))
                 .ReturnsAsync(1L);

        // Act
        var result = await _service.Bind(1, 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Bind_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetById(It.IsAny<object>()))
                     .ReturnsAsync((SysUser?)null);

        // Act
        var result = await _service.Bind(999, 2);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.Insert(It.IsAny<SysUserPosition>()), Times.Never);
    }

    [Fact]
    public async Task Bind_NonExistentPosition_ReturnsFalse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetById(It.IsAny<object>()))
                     .ReturnsAsync(new SysUser { Id = 1 });
        _mockPositionRepo.Setup(r => r.GetById(It.IsAny<object>()))
                         .ReturnsAsync((SysPosition?)null);

        // Act
        var result = await _service.Bind(1, 999);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.Insert(It.IsAny<SysUserPosition>()), Times.Never);
    }

    [Fact]
    public async Task Bind_AlreadyBound_ReturnsFalse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetById(It.IsAny<object>()))
                     .ReturnsAsync(new SysUser { Id = 1 });
        _mockPositionRepo.Setup(r => r.GetById(It.IsAny<object>()))
                         .ReturnsAsync(new SysPosition { Id = 2 });
        _mockRepo.Setup(r => r.Exist(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Bind(1, 2);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.Insert(It.IsAny<SysUserPosition>()), Times.Never);
    }

    [Fact]
    public async Task Unbind_ExistingRelation_ReturnsTrue()
    {
        // Arrange
        var relation = new SysUserPosition { Id = 1, UserId = 1, PositionId = 2 };
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition> { relation });
        _mockRepo.Setup(r => r.Delete(It.IsAny<SysUserPosition>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Unbind(1, 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Unbind_NonExistingRelation_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition>());

        // Act
        var result = await _service.Unbind(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPositionsByUserId_WithRelations_ReturnsPositions()
    {
        // Arrange
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition>
                 {
                     new SysUserPosition { PositionId = 1 },
                     new SysUserPosition { PositionId = 2 }
                 });
        _mockPositionRepo.Setup(r => r.GetByIds(It.IsAny<object[]>()))
                         .ReturnsAsync(new List<SysPosition>
                         {
                             new SysPosition { Id = 1, Name = "软件工程师" },
                             new SysPosition { Id = 2, Name = "产品经理" }
                         });

        // Act
        var positions = await _service.GetPositionsByUserId(1);

        // Assert
        Assert.Equal(2, positions.Count);
    }

    [Fact]
    public async Task GetPositionsByUserId_NoRelations_ReturnsEmpty()
    {
        // Arrange
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition>());

        // Act
        var positions = await _service.GetPositionsByUserId(1);

        // Assert
        Assert.Empty(positions);
    }

    [Fact]
    public async Task GetUsersByPositionId_WithRelations_ReturnsUsers()
    {
        // Arrange
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition>
                 {
                     new SysUserPosition { UserId = 1 },
                     new SysUserPosition { UserId = 2 }
                 });
        _mockUserRepo.Setup(r => r.GetByIds(It.IsAny<object[]>()))
                     .ReturnsAsync(new List<SysUser>
                     {
                         new SysUser { Id = 1, RealName = "张三" },
                         new SysUser { Id = 2, RealName = "李四" }
                     });

        // Act
        var users = await _service.GetUsersByPositionId(1);

        // Assert
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetUsersByPositionId_NoRelations_ReturnsEmpty()
    {
        // Arrange
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<Expression<Func<SysUserPosition, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<SysUserPosition, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysUserPosition>());

        // Act
        var users = await _service.GetUsersByPositionId(1);

        // Assert
        Assert.Empty(users);
    }
}