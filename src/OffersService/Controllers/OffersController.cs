using Microsoft.AspNetCore.Mvc;
using OffersService.DTOs;
using OffersService.Services;

namespace OffersService.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;
    private readonly IOfferImportService _importService;

    public OffersController(IOfferService offerService, IOfferImportService importService)
    {
        _offerService  = offerService;
        _importService = importService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<OfferDto>>> GetActiveOffers()
    {
        var offers = await _offerService.GetActiveOffersAsync();
        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OfferDto>> GetById(int id)
    {
        var offer = await _offerService.GetByIdAsync(id);
        if (offer is null)
            return NotFound();
        return Ok(offer);
    }

    [HttpPost("import")]
    public async Task<ActionResult> Import([FromBody] ImportRequest request)
    {
        var importedCount = await _importService.ImportAsync(request.Rows);
        return Accepted(new { importedCount });
    }
}
