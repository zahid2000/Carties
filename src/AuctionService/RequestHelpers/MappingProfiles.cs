﻿using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Auction>().ForMember(d => d.Item, opt => opt.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<UpdateAuctionDto, Auction>().ForMember(d => d.Item, opt => opt.MapFrom(s => s));
        CreateMap<UpdateAuctionDto, Item>();
    }
}
