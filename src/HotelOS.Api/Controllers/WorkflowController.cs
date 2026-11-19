using HotelOS.Application.Common.Interfaces;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelOS.Api.Controllers;

[ApiController]
[Route("api/v1/workflows")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUser;

    public WorkflowController(IWorkflowService workflowService, ICurrentUserService currentUser)
    {
        _workflowService = workflowService;
        _currentUser = currentUser;
    }

    [HttpGet("definitions")]
    public async Task<IActionResult> GetDefinitions(CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetAllDefinitionsAsync(cancellationToken);
        return Ok(ApiResponse<List<WorkflowDefinitionDto>>.Ok(result));
    }

    [HttpGet("definitions/{code}")]
    public async Task<IActionResult> GetDefinition([FromRoute] string code, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetDefinitionAsync(code, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage($"Workflow definition '{code}' not found.", 404));

        return Ok(ApiResponse<WorkflowDefinitionDto>.Ok(result));
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartWorkflow(
        [FromQuery] string code,
        [FromQuery] string entityType,
        [FromQuery] long entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workflowService.StartWorkflowAsync(
                code,
                entityType,
                entityId,
                _currentUser.UserId ?? 1,
                cancellationToken);

            return Ok(ApiResponse<WorkflowInstanceDto>.Ok(result, "Workflow started successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [HttpPost("transition")]
    public async Task<IActionResult> TransitionStep(
        [FromBody] WorkflowTransitionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workflowService.TransitionStepAsync(
                request.InstanceId,
                request.ActionName,
                request.Comments,
                _currentUser.UserId ?? 1,
                cancellationToken);

            return Ok(ApiResponse<WorkflowInstanceDto>.Ok(result, $"Step transition '{request.ActionName}' completed."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailMessage(ex.Message, 400));
        }
    }

    [HttpGet("instances/{id}")]
    public async Task<IActionResult> GetInstance([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _workflowService.GetWorkflowInstanceAsync(id, cancellationToken);
        if (result == null)
            return NotFound(ApiResponse.FailMessage("Workflow instance not found.", 404));

        return Ok(ApiResponse<WorkflowInstanceDto>.Ok(result));
    }
}
