# Enum Vs String Request Parameter Demo

##### Sunday 22nd June, 2025, 11:OO AM WAT

I'm curious about the trade-offs between using an enum type and using a string type in the request model 
of a RESTful API endpoint for a parameter that accepts a discrete set of values. I'm wondering which is better
in the context of an ASP.NET Web API (using good 'ol controllers), with respect to type safety, ease of use for
the client, validation and documentation. Let's go!

##### 1 hour 30 minutes later: I realized that I do not understand enums!

First of all, for a parameter that accepts a discrete set of values, enums are the way to go. With what I have 
learnt in the last hour and half, here's how I would leverage them for an ASP.NET Core Web API application with an 
endpoint that needs to accept a discrete set of values:
- I'd define my enum type, e.g.
```csharp
    public enum AuthorizationCodeDeliveryMode
    {
        Sms,
        Email,
        MailBox
    }
```
- Then I'd define my request model and its validator, e.g.
```csharp
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
```
- Now with this setup, my client has to provide `0` for `Sms`, `1` for `Email`, or `2` for `MailBox` 
when making a request. This is type-safe and ensures that only valid values are accepted but it relies on my client
knowing what `0`, `1` and `2` means which is not ideal. To enable my client to use the meaningful values `Sms`, `Email` 
and `MailBox`, I'd register a `JsonStringEnumConverter` in my DI container as follows:
```csharp
builder.Services.AddControllers().AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     }); 
```
This will allow my client to provide string values matching my enum values, e.g.
```json
{
	"UserIdentifier": "12345",
	"IdentifierType": "USERID",
	"DeliveryMode": "Email"
}
```
Moreso, there is no restriction on the casing of the values: my client can pass `email`, or `EMAIL`, or `eMaiL`, my application
will receive it as my defined value `Email`. My client can also pass `0` or `1` or `2` and my application will receive the 
corresponding value.
- Now if I don't want my client to be able to pass integer values, I would have to amend the registration of the `JsonStringEnumConverter` in my DI container as follows:
```csharp
builder.Services.AddControllers().AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
     });
```
and if I also want to to restrict my client to a particular casing, I'll provided a value for the `namingPolicy` parameter to
`JsonStringEnumConverter` but I probably would never do this.
- With the setup in place so far, my client can pass meaningful values in their payload to me, they don't need to worry about casing
and I am not allowing integer values. What could go wrong?
- Well, there's something I don't like about my enum as it currently is:
```csharp
public enum AuthorizationCodeDeliveryMode
{
    Sms,
    Email,
    MailBox
}
```
and its the fact that `Sms` has a value of `0`, `Email` has a value of `1` and `MailBox` has a value of `2`, by
default. What this means for me is that my client can leave out `DeliveryMode` in their payload and .NET will give it
a default value of `Sms` because it has a value of `0`. With this, I cannot ensure that my client provides a value for
`DeliveryMode` using any FluentValidation rule (or can I?) because if they violate the rule, .NET will cover up for them
by giving me `Sms` as the value they passed, and I don't want that (I might want that sometimes though). Well, to prevent
this, all I have to do is:
```csharp
public enum AuthorizationCodeDeliveryMode
{
    Sms = 1,
    Email,
    MailBox
}
```
Why does this work? Well now, `Sms` has been assigned a value of `1` and because of that, `Email` now has a value
of `2` and `MailBox` now has a value of `3`. .NET will still try to cover up for my client when they do not provide
a value for `DeliveryMode`, but it'll always pick an underlying value of `0` (because my enum as it is has a int32 
as the underlying type and the default value for that is `0`) and since my enum no longer recognizes `0` as a valid
value, my FluentValidation rule works.

Comparing this with what I did for `IdentifierType` which is a string but has a validation rule that allows only one
of `USERID`, `EMAILADDESS` and `PHONNUMBER`. I think I like the use of enums more. I think the main disadvantage here
is in writing comparisons: I don't know, but I just like
```csharp
if (deliveryMode == DeliveryMode.Sms)
{
    // Do something
}
```
much more than
```csharp
if (identifierType == "USERID")
{
    // Do something
}
```
Also, I like that I can always rely on `IsInEnum()` to validate `DeliveryMode` if more modes are added later on without
any change to the rule, but `.Must(x => (x == "USERID" | x == "EMAILADDRESS" | x == "PHONENUMBER"))` will always need to
be updated whenever a new delivery mode is added. This is not open for extension and closed for modification.