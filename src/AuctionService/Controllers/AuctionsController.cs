using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contratcs;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        System.Console.WriteLine("request comes" + " date: " + date);

        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            DateTime lastUpdatedAt = DateTime.Parse(date).ToUniversalTime();
            query = query.Where(x => x.UpdatedAt.CompareTo(lastUpdatedAt) > 0);
        }
        var mappedAuctions = await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        return mappedAuctions;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
                .Where(x => x.Id == id)
                .Include(x => x.Item)
                .FirstOrDefaultAsync();
        if (auction == null) return NotFound();

        var mappedAuction = _mapper.Map<AuctionDto>(auction);
        return mappedAuction;
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        Auction auction = _mapper.Map<Auction>(auctionDto);
        //TODO: add current user to seller
        auction.Seller = "test";

        await _context.Auctions.AddAsync(auction);
        var newAuction = _mapper.Map<AuctionDto>(auction);
        var auctionCreated = _mapper.Map<AuctionCreated>(newAuction);
        await _publishEndpoint.Publish(auctionCreated);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result)
            return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        Auction auction = await _context.Auctions.Where(x => x.Id == id).Include(x => x.Item).FirstOrDefaultAsync();
        if (auction == null) return NotFound();
        //TODO: check seller == current username
        _mapper.Map(auctionDto, auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Problem sacing changes");
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null) return NotFound();
        //TODO: check seller == current username
        _context.Auctions.Remove(auction);
        await _publishEndpoint.Publish<AuctionDeleted>(new{Id=auction.Id.ToString()});
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not update DB");
        return Ok(result);
    }
}

