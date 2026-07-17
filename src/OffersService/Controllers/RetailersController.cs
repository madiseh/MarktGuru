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

    public RetailersController(
        IRetailerService retailerService,
        IValidator<RetailerStatsRequest> retailerStatsRequestValidator)
    {
        _retailerService = retailerService;
        _retailerStatsRequestValidator = retailerStatsRequestValidator;
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
