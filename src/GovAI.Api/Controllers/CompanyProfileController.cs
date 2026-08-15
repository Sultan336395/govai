using GovAI.Api.Infrastructure;
using GovAI.Application.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovAI.Api.Controllers;

/// <summary>
/// <c>/api/company-profile</c> — firma kartı, ERP eşitleme, manuel düzeltme.
/// </summary>
[ApiController]
[Route("api/company-profile")]
[Authorize(Policy = Policies.Read)]
[Produces("application/json")]
public sealed class CompanyProfileController(CompanyProfileService service) : ControllerBase
{
    /// <summary>Kiracıya ait firmaların listesi.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanySummaryDto>>> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(cancellationToken));

    /// <summary>Firma kartının tüm detayları ve profil doluluk oranı.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDetailDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.ManageCompany)]
    [Audited("CompanyProfile.Created", "Company")]
    public async Task<ActionResult<CompanyDetailDto>> Create(
        [FromBody] UpsertCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.ManageCompany)]
    [Audited("CompanyProfile.Updated", "Company")]
    public async Task<ActionResult<CompanyDetailDto>> Update(
        Guid id,
        [FromBody] UpsertCompanyRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// ERP / İK / muhasebe sisteminden kısmi veri eşitler (Modül 2).
    /// Yalnızca gönderilen bölümler güncellenir; profil değişirse skorlar yeniden hesaplanmak üzere kuyruğa alınır.
    /// </summary>
    [HttpPost("erp-sync")]
    [Authorize(Policy = Policies.ManageCompany)]
    [Audited("CompanyProfile.ErpSynced", "Company")]
    public async Task<ActionResult<ErpSyncResult>> SyncFromErp(
        [FromBody] ErpSyncRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.SyncFromErpAsync(request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.ManageCompany)]
    [Audited("CompanyProfile.Deleted", "Company")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
