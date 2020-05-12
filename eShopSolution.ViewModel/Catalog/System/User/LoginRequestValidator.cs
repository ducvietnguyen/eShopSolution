using eShopSolution.ViewModel.Catalog.System.User;
using FluentValidation;

namespace eShopSolution.ViewModel.Catalog.System.User
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}