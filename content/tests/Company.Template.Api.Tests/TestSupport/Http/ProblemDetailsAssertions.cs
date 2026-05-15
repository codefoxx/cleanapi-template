namespace Company.Template.Api.Tests.TestSupport.Http;

internal static class ProblemDetailsAssertions
{
    extension(HttpResponseMessage response)
    {
        public async Task<ApiProblemDetails> ShouldBeProblemAsync(HttpStatusCode expectedStatusCode,
            string expectedTitle,
            string expectedCode)
        {
            response.StatusCode.ShouldBe(expectedStatusCode);

            ApiProblemDetails problem = await response.ReadJsonAsync<ApiProblemDetails>();

            problem.Status.ShouldBe((int)expectedStatusCode);
            problem.Title.ShouldBe(expectedTitle);
            problem.Code.ShouldBe(expectedCode);

            return problem;
        }

        public async Task<ApiProblemDetails> ShouldBeValidationProblemAsync(string expectedCode)
        {
            ApiProblemDetails problem = await response.ShouldBeProblemAsync(
                HttpStatusCode.UnprocessableEntity,
                "Validation failed.",
                expectedCode);

            problem.Errors.ShouldNotBeNull();
            problem.Errors.ShouldContainKey("request");

            return problem;
        }

        public Task<ApiProblemDetails> ShouldBeNotFoundProblemAsync(string expectedCode)
        {
            return response.ShouldBeProblemAsync(
                HttpStatusCode.NotFound,
                "Resource not found.",
                expectedCode);
        }

        public Task<ApiProblemDetails> ShouldBeConflictProblemAsync(string expectedCode)
        {
            return response.ShouldBeProblemAsync(
                HttpStatusCode.Conflict,
                "Conflict.",
                expectedCode);
        }
    }
}
