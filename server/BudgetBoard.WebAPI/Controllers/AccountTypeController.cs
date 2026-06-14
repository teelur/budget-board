using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.WebAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace BudgetBoard.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountTypeController(
    ILogger<AccountTypeController> logger,
    UserManager<ApplicationUser> userManager,
    IAccountTypeService accountTypeService,
    IStringLocalizer<ApiLogStrings> logLocalizer,
    IStringLocalizer<ApiResponseStrings> responseLocalizer
) : ApiControllerBase<AccountTypeController>(logger, logLocalizer, responseLocalizer)
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] AccountTypeCreateRequest newAccountType)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountTypeService.CreateAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                newAccountType
            );
            return Ok();
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Read()
    {
        return await HandleRequestAsync(async () =>
        {
            return Ok(
                await accountTypeService.ReadAccountTypesAsync(
                    new Guid(userManager.GetUserId(User) ?? string.Empty)
                )
            );
        });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AccountTypeUpdateRequest updatedAccountType)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountTypeService.UpdateAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                updatedAccountType
            );
            return Ok();
        });
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid accountId)
    {
        return await HandleRequestAsync(async () =>
        {
            await accountTypeService.DeleteAccountTypeAsync(
                new Guid(userManager.GetUserId(User) ?? string.Empty),
                accountId
            );
            return Ok();
        });
    }
}
