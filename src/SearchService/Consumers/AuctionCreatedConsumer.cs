using System;
using AutoMapper;
using Contratcs;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        System.Console.WriteLine($"---> consume: {context.Message.Id}");
        if (context.Message.Model == "Foo") throw new ArgumentException("Error with Model");
        var item = _mapper.Map<Item>(context.Message);
        await item.SaveAsync();
    }
}
