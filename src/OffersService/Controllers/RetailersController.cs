using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OffersService.DTOs;
using OffersService.Services;
using OffersService.Validators;

namespace OffersService.Controllers;

[ApiController]
[Route("api/retailers")]
public class RetailersController : ControllerBase
{
    private readonly IRetailerService _retailerService;
    private readonly IValidator<RetailerStatsRequest> _retailerStatsRequestValidator;
    private readonly IValidator<CreateRetailerRequest> _createRetailerRequestValidator;

    public RetailersController(
        IRetailerService retailerService,
        IValidator<RetailerStatsRequest> retailerStatsRequestValidator,
        IValidator<CreateRetailerRequest> createRetailerRequestValidator)
    {
        _retailerService = retailerService;
        _retailerStatsRequestValidator = retailerStatsRequestValidator;
        _createRetailerRequestValidator = createRetailerRequestValidator;
    }

    [HttpGet("{id:int}", Name = "GetRetailer")]
    public async Task<ActionResult<RetailerDto>> GetById(int id)
    {
        var retailer = await _retailerService.GetByIdAsync(id);
        if (retailer is null)
        {
            return NotFound();
        }

        return Ok(retailer);
    }

    [HttpPost]
    public async Task<ActionResult<RetailerDto>> Create([FromBody] CreateRetailerRequest? request)
    {
        request ??= new CreateRetailerRequest();

        var validationResult = await _createRetailerRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var retailer = await _retailerService.CreateAsync(request);
        if (retailer is null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Retailer already exists",
                Detail = RetailerValidationMessages.NameAlreadyExists,
                Status = StatusCodes.Status409Conflict
            });
        }

        return CreatedAtRoute("GetRetailer", new { id = retailer.Id }, retailer);
    }

    [HttpGet("{id}/stats", Name = "GetRetailerStats")]
    public async Task<ActionResult<RetailerStatsDto>> GetStats([FromRoute] RetailerStatsRequest request)
    {
        var validationResult = await _retailerStatsRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var stats = await _retailerService.GetStatsAsync(request.ToRetailerId());
        if (stats is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Retailer not found",
                Detail = RetailerValidationMessages.NotFoundOrInactive,
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(stats);
    }
}
