using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Model;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForDetail>()
            .ForMember(dest => dest.PhotoUrl, opt =>
            opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.GetAge()));
            CreateMap<User, UserForList>()
             .ForMember(dest => dest.PhotoUrl, opt =>
            opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.GetAge()));
            CreateMap<Photo, PhotoForDetailed>();
            CreateMap<UserForUpdate, User>();
            CreateMap<Photo,PhotoForReturn>();
            CreateMap<PhotoForCreation,Photo>();
            CreateMap<UserForRegister,User>();
            CreateMap<MessageForCreation,Message>().ReverseMap();
            CreateMap<MessageForReturn,Message>().ReverseMap()
            .ForMember(u=>u.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p=>p.IsMain).Url))
            .ForMember(u=>u.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p=>p.IsMain).Url));
        }
    }
}