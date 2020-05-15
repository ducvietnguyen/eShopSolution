using FluentValidation;
using System;

namespace eShopSolution.ViewModel.Catalog.System.User
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required")
                 .MaximumLength(200).WithMessage("First name can not over 200 characters");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required")
                .MaximumLength(200).WithMessage("Last name can not over 200 characters");

            RuleFor(x => x.Dob).GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Birthday cannot greater than 100 years");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format not match");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");
        }
    }
}