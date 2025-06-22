using FluentValidation;

namespace EnumVsStringRequestParameterDemo.API.Models
{
    public record SendAuthorizationCodeRequest
    {
        public string UserIdentifier { get; init; } = string.Empty;

        public string IdentifierType { get; init; } = string.Empty;

        public AuthorizationCodeDeliveryMode DeliveryMode { get; set; }
    }

    public class SendAuthorizationCodeRequestValidator : AbstractValidator<SendAuthorizationCodeRequest>
    {
        public SendAuthorizationCodeRequestValidator()
        {
            RuleFor(x => x.UserIdentifier).NotEmpty().WithMessage("User identifier is required.");
            RuleFor(x => x.IdentifierType).NotEmpty().WithMessage("Identifier type is required.")
                .Must(x => (x == "USERID" | x == "EMAILADDRESS" | x == "PHONENUMBER"))
                .WithMessage("Identifier type must be USERID, EMAILADDRESS or PHONENUMBER");
            RuleFor(x => x.DeliveryMode).IsInEnum().WithMessage("Delivery mode must be a valid value.");
        }
    }
}
