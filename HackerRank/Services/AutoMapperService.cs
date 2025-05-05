using AutoMapper;
using HackerRank.DataTransferObjects;
using HackerRank.Models;

namespace HackerRank.Services
{
    public class AutoMapperService : Profile
    {
        public AutoMapperService()
        {
            CreateMap<HackerRankItem, HackerRankItemDTO>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.By))
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.Time).LocalDateTime));
        }
    }
}
