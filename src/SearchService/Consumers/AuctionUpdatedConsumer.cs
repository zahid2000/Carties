using System;
using AutoMapper;
using Contratcs;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        System.Console.WriteLine($"------> Consuming auction updated: {context.Message.Id}");
        var item = _mapper.Map<Item>(context.Message);
        var result = await DB.Update<Item>()
                  .Match(x => x.ID == item.ID)
                  .ModifyOnly(x => new
                  {
                      x.Color,
                      x.Make,
                      x.Model,
                      x.Year,
                      x.Mileage,
                  }, item)
                  .ExecuteAsync();
        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
    }
}
