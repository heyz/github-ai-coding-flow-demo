namespace SJ.BackEnd.Template.Tests.Model;

public class SysUserTests
{
    [Fact]
    public void CreateFrom_WithValidRequest_SetsAllProperties()
    {
        var request = new CreateUserRequest
        {
            Nickname = "test_nick",
            RealName = "张三",
            Gender = 1,
            BirthDate = new DateTime(1990, 1, 1)
        };

        var user = SysUser.CreateFrom(request);

        Assert.Equal(0, user.Id);
        Assert.Equal("test_nick", user.Nickname);
        Assert.Equal("张三", user.RealName);
        Assert.Equal(1, user.Gender);
        Assert.Equal(new DateTime(1990, 1, 1), user.BirthDate);
        Assert.NotEqual(default, user.CreatedTime);
    }
}

public class SysRoleTests
{
    [Fact]
    public void CreateFrom_WithValidRequest_SetsAllProperties()
    {
        var request = new CreateRoleRequest
        {
            Name = "管理员",
            Code = "admin",
            Description = "管理员角色",
            SortOrder = 1
        };

        var role = SysRole.CreateFrom(request);

        Assert.Equal("管理员", role.Name);
        Assert.Equal("admin", role.Code);
        Assert.Equal("管理员角色", role.Description);
        Assert.Equal(1, role.SortOrder);
        Assert.NotEqual(default, role.CreatedAt);
        Assert.NotEqual(default, role.UpdatedAt);
    }
}

public class SysUserRoleTests
{
    [Fact]
    public void CreateRelation_WithValidIds_SetsProperties()
    {
        var relation = SysUserRole.CreateRelation(1, 2);

        Assert.Equal(1, relation.UserId);
        Assert.Equal(2, relation.RoleId);
        Assert.NotEqual(default, relation.CreatedAt);
    }
}
