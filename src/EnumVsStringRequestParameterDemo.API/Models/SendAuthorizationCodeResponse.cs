namespace EnumVsStringRequestParameterDemo.API.Models
{
    public record SendAuthorizationCodeResponse
    {
        public string UserIdentifier { get; init; } = string.Empty;

        public string ResponseMessage { get; init; } = "Authorization code sent successfully";
    }
}
