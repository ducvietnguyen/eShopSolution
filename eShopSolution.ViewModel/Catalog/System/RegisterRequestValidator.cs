using FluentValidation;
using System;

namespace eShopSolution.ViewModel.Catalog.System
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required")
                 .MaximumLength(200).WithMessage("First name can not over 200 characters");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required")
                .MaximumLength(200).WithMessage("Last name can not over 200 characters");

            RuleFor(x => x.Dob).GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Birthday cannot greater than 100 years");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format not match");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.UserName).NotEmpty().WithMessage("User name is required");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required")
                .Matches(@"(?=.{6,})(?=(.*\d){1,})(?=(.*\W){1,})")
                .WithMessage("Password must be at least 6 characters long and contain at least one number and one special character.");

            RuleFor(x => x).Custom((registerRequest, context) =>
            {
                if (registerRequest.Password != registerRequest.ConfirmedPassword)
                {
                    context.AddFailure("Confirm password is not match");
                }
            });
        }
    }
}