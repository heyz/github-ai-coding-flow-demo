using Moq;
using SJ.BackEnd.Template.IRepository;
using SJ.BackEnd.Template.Services;

namespace SJ.BackEnd.Template.Tests.Services;

public class SysPermissionServiceTests
{
    private readonly Mock<IBaseRepository<SysPermission>> _mockRepo;
    private readonly SysPermissionService _service;

    public SysPermissionServiceTests()
    {
        _mockRepo = new Mock<IBaseRepository<SysPermission>>();
        _service = new SysPermissionService(_mockRepo.Object);
    }

    [Fact]
    public async Task Create_UniqueCode_ReturnsPermission()
    {
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, bool>>>()))
                 .ReturnsAsync(false);
        _mockRepo.Setup(r => r.Insert(It.IsAny<SysPermission>()))
                 .ReturnsAsync(1L);
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPermission { Id = 1, Name = "用户管理", Code = "sys:user" });

        var result = await _service.Create(new CreatePermissionRequest { Name = "用户管理", Code = "sys:user" });

        Assert.NotNull(result);
        Assert.Equal("用户管理", result.Name);
    }

    [Fact]
    public async Task Create_DuplicateCode_ReturnsNull()
    {
        _mockRepo.Setup(r => r.Exist(It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, bool>>>()))
                 .ReturnsAsync(true);

        var result = await _service.Create(new CreatePermissionRequest { Name = "用户管理", Code = "sys:user" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsFalse()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync((SysPermission?)null);

        var result = await _service.Delete(999);

        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithChildren_ReturnsFalse()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPermission { Id = 1, Name = "系统管理" });
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysPermission> { new SysPermission { Id = 2, ParentId = 1 } });

        var result = await _service.Delete(1);

        Assert.False(result);
        _mockRepo.Verify(r => r.DeleteById(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NoChildren_ReturnsTrue()
    {
        _mockRepo.Setup(r => r.GetById(It.IsAny<object>()))
                 .ReturnsAsync(new SysPermission { Id = 1, Name = "用户管理" });
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(new List<SysPermission>());
        _mockRepo.Setup(r => r.DeleteById(It.IsAny<object>()))
                 .ReturnsAsync(true);

        var result = await _service.Delete(1);

        Assert.True(result);
    }

    [Fact]
    public async Task GetTree_ReturnsAll()
    {
        var list = new List<SysPermission>
        {
            new() { Id = 1, Name = "系统管理" },
            new() { Id = 2, Name = "用户管理", ParentId = 1 }
        };
        _mockRepo.Setup(r => r.QueryByExpression(
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<SysPermission, object>>>(),
            It.IsAny<bool>()))
                 .ReturnsAsync(list);

        var result = await _service.GetTree();

        Assert.Equal(2, result.Count);
    }
}