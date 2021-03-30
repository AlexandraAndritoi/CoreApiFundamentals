using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile 
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(_ => _.VenueName, _ => _.MapFrom(_ => _.Location.VenueName))
                .ForMember(_ => _.Address1, _ => _.MapFrom(_ => _.Location.Address1))
                .ForMember(_ => _.Address2, _ => _.MapFrom(_ => _.Location.Address2))
                .ForMember(_ => _.Address3, _ => _.MapFrom(_ => _.Location.Address3))
                .ForMember(_ => _.CityTown, _ => _.MapFrom(_ => _.Location.CityTown))
                .ForMember(_ => _.StateProvince, _ => _.MapFrom(_ => _.Location.StateProvince))
                .ForMember(_ => _.PostalCode, _ => _.MapFrom(_ => _.Location.PostalCode))
                .ForMember(_ => _.Country, _ => _.MapFrom(_ => _.Location.Country));
            this.CreateMap <Talk, TalkModel>().ReverseMap();
            this.CreateMap<Speaker, SpeakerModel>().ReverseMap();
        }
    }
}
