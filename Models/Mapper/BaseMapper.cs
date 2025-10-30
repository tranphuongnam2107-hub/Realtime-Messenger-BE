using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Models.DTO.Response;
using Models.Model;

namespace Models.Mapper
{
    public class BaseMapper : Profile
    {
        public BaseMapper()
        {
            CreateMap<Account, LoginResponseDTO>();
            CreateMap<Account, ProfileResponseDTO>();
            CreateMap<Message, MessageResponseDTO>()
                .ForMember(mess => mess.SenderInfor, opt => opt.Ignore())
                .ForMember(mess => mess.ReplyToMessage, opt => opt.Ignore())
                .ForMember(dest => dest.IsMine,
                    opt => opt.MapFrom((src, dest, _, ctx) =>
                        ctx.Items.ContainsKey("CurrentUserId") &&
                        ctx.Items["CurrentUserId"].ToString() == src.SenderId));
            CreateMap<Friend, FriendResponseDTO>();
        }

    }
}
