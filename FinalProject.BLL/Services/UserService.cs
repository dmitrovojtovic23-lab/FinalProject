using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IUserService = FinalProject.BLL.Interfaces.IUserService;

namespace FinalProject.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserDto?> GetUserByTelegramIdAsync(string telegramId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new AppUser
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Username = createUserDto.Username,
                TelegramId = createUserDto.TelegramId,
                Email = createUserDto.Email,
                AvatarUrl = createUserDto.AvatarUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException($"User with id {id} not found");

            if (updateUserDto.FirstName != null) user.FirstName = updateUserDto.FirstName;
            if (updateUserDto.LastName != null) user.LastName = updateUserDto.LastName;
            if (updateUserDto.Email != null) user.Email = updateUserDto.Email;
            if (updateUserDto.AvatarUrl != null) user.AvatarUrl = updateUserDto.AvatarUrl;
            if (updateUserDto.IsActive.HasValue) user.IsActive = updateUserDto.IsActive.Value;

            await _context.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .ToListAsync();
            
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
    }
}
