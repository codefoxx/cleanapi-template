using Company.Template.TestSupport.Contracts;

namespace Company.Template.TestSupport.Http;

public static class ProblemDetailsAssertions
{
    extension(HttpResponseMessage response)
    {
        public async Task<ApiProblemDetails> ReadProblemAsync(
            HttpStatusCode expectedStatusCode)
        {
            response.StatusCode.ShouldBe(expectedStatusCode);

            ApiProblemDetails problem = await response.ReadJsonAsync<ApiProblemDetails>();

            problem.Status.ShouldBe((int)expectedStatusCode);

            return problem;
        }

        public Task<ApiProblemDetails> ReadValidationProblemAsync()
        {
            return response.ReadProblemAsync(HttpStatusCode.UnprocessableEntity);
        }

        public Task<ApiProblemDetails> ReadNotFoundProblemAsync()
        {
            return response.ReadProblemAsync(HttpStatusCode.NotFound);
        }

        public Task<ApiProblemDetails> ReadConflictProblemAsync()
        {
            return response.ReadProblemAsync(HttpStatusCode.Conflict);
        }
    }
}
