using AuthentificationService.Application.DTOs;
using AuthentificationService.Core.Entities;
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
    }

    private int GetRoleId(string role)
    {
        return role switch
        {
            "Doctor" => 0,
            "Receptionist" => 1,
            "Patient" => 2,
            _ => throw new Exception("Invalid role.")
        };
    }
}