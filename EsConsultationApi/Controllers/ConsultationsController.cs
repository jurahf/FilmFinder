using EsConsultationApi.Contracts;
using EsConsultationApi.Mapping;
using EsConsultationServices.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace EsConsultationApi.Controllers;

[ApiController]
[Route("api/consultations")]
[Produces("application/json")]
public sealed class ConsultationsController : ControllerBase
{
    private readonly IConsultationWorkflow workflow;

    public ConsultationsController(IConsultationWorkflow workflow)
    {
        this.workflow = workflow;
    }

    /// <summary>
    /// Начинает консультацию и возвращает её первый вопрос.
    /// </summary>
    [HttpPost("Start")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Start(
        [FromBody] StartConsultationRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await workflow.StartAsync(request?.ExpertSystemId, cancellationToken);
        return Ok(ConsultationResponseMapper.ToResponse(result));
    }

    /// <summary>Возвращает текущий вопрос или результат консультации.</summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> GetCurrent(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var result = await workflow.GetCurrentAsync(sessionId, cancellationToken);
        return Ok(ConsultationResponseMapper.ToResponse(result));
    }

    /// <summary>Отвечает на текущий вопрос и возвращает следующий шаг или результат с фильмом.</summary>
    [HttpPost("{sessionId}/answers")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Answer(
        string sessionId,
        [FromBody] AnswerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await workflow.AnswerAsync(
            sessionId,
            request.VariableName,
            request.Value,
            cancellationToken);

        return Ok(ConsultationResponseMapper.ToResponse(result));
    }

    /// <summary>Возвращает другой фильм по тому же результату консультации.</summary>
    [HttpPost("{sessionId}/another")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Another(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var result = await workflow.GetAnotherAsync(sessionId, cancellationToken);
        return Ok(ConsultationResponseMapper.ToResponse(result));
    }

    /// <summary>Сохраняет оценку «понравилось / не понравилось».</summary>
    [HttpPost("{sessionId}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Review(
        string sessionId,
        [FromBody] ReviewRequest request,
        CancellationToken cancellationToken)
    {
        await workflow.ReviewAsync(sessionId, request.Liked, request.Comment, cancellationToken);
        return NoContent();
    }
}
