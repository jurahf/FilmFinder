using EsConsultationApi.Contracts;
using EsConsultationServices.Workflow;

namespace EsConsultationApi.Mapping;

public static class ConsultationResponseMapper
{
    public static ConsultationResponse ToResponse(ConsultationFlowResult result)
    {
        return new ConsultationResponse
        {
            SessionId = result.SessionId,
            ExpertSystemId = result.ExpertSystemId,
            ExpertSystemName = result.ExpertSystemName,
            Status = result.Status.ToString(),
            Question = result.Step.Question,
            Result = result.Step.Result,
            Film = result.Film == null ? null : FilmMapper.ToDto(result.Film)
        };
    }
}
