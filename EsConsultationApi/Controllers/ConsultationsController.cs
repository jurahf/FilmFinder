using EsConsultationApi.Contracts;
using EsConsultationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsConsultationApi.Controllers;

[ApiController]
[Route("api/consultations")]
[Produces("application/json")]
public sealed class ConsultationsController : ControllerBase
{
    private readonly ConsultationWorkflow workflow;

    public ConsultationsController(ConsultationWorkflow workflow)
    {
        this.workflow = workflow;
    }

    /// <summary>Начинает консультацию и возвращает её первый вопрос.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Start(
        [FromBody] StartConsultationRequest? request,
        CancellationToken cancellationToken)
    {
        return Ok(await workflow.StartAsync(request?.ExpertSystemId, cancellationToken));
    }

    /// <summary>Возвращает текущий вопрос или результат консультации.</summary>
    [HttpGet("{sessionId}")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> GetCurrent(
        string sessionId,
        CancellationToken cancellationToken)
    {
        return Ok(await workflow.GetCurrentAsync(sessionId, cancellationToken));
    }

    /// <summary>Отвечает на текущий вопрос и возвращает следующий шаг или результат с фильмом.</summary>
    [HttpPost("{sessionId}/answers")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Answer(
        string sessionId,
        [FromBody] AnswerRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await workflow.AnswerAsync(
            sessionId,
            request.VariableName,
            request.Value,
            cancellationToken));
    }

    /// <summary>Возвращает другой фильм по тому же результату консультации.</summary>
    [HttpPost("{sessionId}/another")]
    [ProducesResponseType(typeof(ConsultationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsultationResponse>> Another(
        string sessionId,
        CancellationToken cancellationToken)
    {
        return Ok(await workflow.GetAnotherAsync(sessionId, cancellationToken));
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
