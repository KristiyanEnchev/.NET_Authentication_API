namespace Infrastructure.Identity
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    using Application.Interfaces.Post;
    using Application.Interfaces.Identity;
    using Application.Handlers.Identity.Commands.Password;

    using Domain.Entities.Identity;
    using Domain.Events.Identity.Password;

    using Shared;
    using Application.Interfaces.Account;

    public class PasswordService : IPasswordService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IUserActivityService _userActivityService;

        public PasswordService(
            UserManager<User> userManager,
            IEmailService emailService,
            INotificationService notificationService,
            SignInManager<User> signInManager,
            IUserActivityService userActivityService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _notificationService = notificationService;
            _signInManager = signInManager;
            _userActivityService = userActivityService;
        }

        public async Task<Result<string>> ForgotPassword(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.SuccessResult("If your email exists in our system, you will receive password reset instructions.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = $"{request.ResetPasswordUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            var emailSent = await _emailService.SendEmailAsync(
                user.Email,
                "Reset Your Password",
                $@"<p>You've requested to reset your password.</p>
                   <p>Please click <a href='{resetLink}'>here</a> to reset your password.</p>
                   <p>If you didn't request this, please ignore this email.</p>
                   <p>This link will expire in 24 hours.</p>",
                "Identity API");

            await _userActivityService.LogActivityAsync(
                user.Id,
                "ForgotPassword",
                emailSent,
                emailSent ? "Password reset email sent" : "Failed to send password reset email");

            if (!emailSent)
            {
                return Result<string>.Failure("Failed to send password reset email. Please try again later.");
            }

            return Result<string>.SuccessResult("If your email exists in our system, you will receive password reset instructions.");
        }

        public async Task<Result<string>> ResetPassword(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.Failure("Password reset failed.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                await _userActivityService.LogActivityAsync(
                    user.Id,
                    "ResetPassword",
                    false,
                    $"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            await _userActivityService.LogActivityAsync(
                user.Id,
                "ResetPassword",
                true,
                "Password reset successfully");

            user.AddDomainEvent(new PasswordResetEvent(user.Id));

            await _emailService.SendEmailAsync(
                user.Email,
                "Password Reset Successful",
                "<p>Your password has been reset successfully.</p>",
                "Identity API");

            await _notificationService.SendNotificationAsync(
                user.Id,
                "Password Reset",
                "Your password has been reset successfully.",
                "password_reset");

            return Result<string>.SuccessResult("Password has been reset successfully.");
        }

        public async Task<Result<string>> ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<string>.Failure("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                await _userActivityService.LogActivityAsync(
                    userId,
                    "ChangePassword",
                    false,
                    $"Failed to change password: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                return Result<string>.Failure(result.Errors.Select(e => e.Description).ToList());
            }

            await _userActivityService.LogActivityAsync(
                userId,
                "ChangePassword",
                true,
                "Password changed successfully");

            user.AddDomainEvent(new PasswordChangedEvent(user.Id));

            await _signInManager.RefreshSignInAsync(user);

            return Result<string>.SuccessResult("Password changed successfully.");
        }

        public async Task<Result<string>> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<string>.Failure("User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<string>.SuccessResult(token);
        }
    }
}