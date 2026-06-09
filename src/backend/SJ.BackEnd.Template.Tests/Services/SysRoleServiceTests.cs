using Moq;
using SJ.BackEnd.Template.IRepository;
using SJ.BackEnd.Template.Services;

namespace SJ.BackEnd.Template.Tests.Services;

public class SysRoleServiceTests
{
    private readonly Mock<IBaseRepository<SysRole>> _mockRepo;
    private readonly ISysRoleService _service;

    public SysRoleServiceTests()
    {
        _mockRepo = new Mock<IBaseRepository<SysRole>>();
        _service = new SysRoleService(_mockRepo.Object);
    }

    [Fact]
    public async Task Create_WithUniqueName_ReturnsRole()
    {
        // Arrange
        var request = new CreateRoleRequest { Name = "管理员", Code = "admin", Description = "管理员角色" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysRole, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.Insert(It.IsAny<SysRole>()))
                 .ReturnsAsync(1L);
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysRole { Id = 1, Name = "管理员", Code = "admin" });

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("管理员", result.Name);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsNull()
    {
        // Arrange
        var request = new CreateRoleRequest { Name = "管理员", Code = "admin" };
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysRole, bool>>>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Create(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_NonSystemRole_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysRole { Id = 1, Name = "测试角色", IsSystem = false });
        _mockRepo.Setup(r => r.DeleteById(It.IsAny<object>()))
                 .ReturnsAsync(true);

        // Act
        var result = await _service.Delete(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Delete_SystemRole_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysRole { Id = 2, Name = "系统角色", IsSystem = true });

        // Act
        var result = await _service.Delete(2);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NonExistentRole_ReturnsFalse()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync((SysRole?)null);

        // Act
        var result = await _service.Delete(999);

        // Assert
        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }
}