using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OffersService.DTOs;
using OffersService.Models.Pagination;
using OffersService.Services;

namespace OffersService.Controllers;

[ApiController]
[Route("api/offers")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;
    private readonly IOfferImportService _importService;
    private readonly IValidator<OfferListRequest> _offerListRequestValidator;

    public OffersController(
        IOfferService offerService,
        IOfferImportService importService,
        IValidator<OfferListRequest> offerListRequestValidator)
    {
        _offerService = offerService;
        _importService = importService;
        _offerListRequestValidator = offerListRequestValidator;
    }

    [HttpGet(Name = "GetOffers")]
    public async Task<ActionResult<PagedList<OfferListItemDto>>> GetAll([FromQuery] OfferListRequest request)
    {
        var validationResult = await _offerListRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var response = await _offerService.GetOffersAsync(request);
        return Ok(response);
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
