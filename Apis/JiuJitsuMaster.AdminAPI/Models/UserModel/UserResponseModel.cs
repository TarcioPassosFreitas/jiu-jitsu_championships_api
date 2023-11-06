namespace JiuJitsuMaster.AdminAPI.Models.UserModel;

public class UserResponseModel
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UpdatedAt { get; set; } = null!;
}