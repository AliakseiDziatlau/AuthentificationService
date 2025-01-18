using AuthentificationService.Application.DTOs;
using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Enum;
using AutoMapper;

namespace AuthentificationService.Infrastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterDTO, Accounts>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => GetRoleId(src.Role)))
            .ForMember(dest => dest.createdAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.isEmailVerified, opt => opt.MapFrom(_ => false));
        
        CreateMap<Accounts, AccountsDTO>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                Enum.GetName(typeof(RolesEnum), src.RoleId)));
    }

    private int GetRoleId(string role)
    {
        if (Enum.TryParse(typeof(RolesEnum), role, true, out var result) && result != null)
        {
            return (int)result;
        }
        throw new Exception("Invalid role.");
    }
}