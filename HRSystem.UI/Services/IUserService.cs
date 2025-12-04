using HRSystem.UI.DTOs;

namespace HRSystem.UI.Services
{
    public interface IUserService
    {
        Task<bool> AddAsync(AddUserRequestDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<List<UserDto>> GetAllAsync(int page = 1, int size = 10);
        Task<UserDto> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, UpdateUserRequestDto dto);
    }
}