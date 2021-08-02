using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Business;
using IAutoM8.Domain.Models.Formula;
using IAutoM8.Domain.Models.Formula.Task;
using IAutoM8.Domain.Models.Project;
using IAutoM8.Domain.Models.Project.Task;
using IAutoM8.Domain.Models.Skill;
using IAutoM8.Domain.Models.User;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Global.Exceptions;
using IAutoM8.Global.Options;
using IAutoM8.InfusionSoft.Interfaces;
using IAutoM8.Repository;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.Infrastructure.Extensions;
using IAutoM8.Service.Notification.Dto;
using IAutoM8.Service.Notification.Interfaces;
using IAutoM8.Service.Scheduler.Interfaces;
using IAutoM8.Service.SendGrid.Interfaces;
using IAutoM8.Service.Teams.Dto;
using IAutoM8.Service.Users.Dto;
using IAutoM8.Service.Users.Interfaces;
using IAutoM8.WebSockets.Stores.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.Extensions.Options;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace IAutoM8.Service.Users
{
    public class UserService : IUserService
    {
        private readonly ClaimsPrincipal _principal;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AccountConfirmationSetting _accountConfirmationSetting;
        private readonly IRepo _repo;
        private readonly IEmailService _emailService;
        private readonly EmailTemplates _emailTemplates;
        private readonly ISendGridService _sendGridService;
        private readonly IScheduleService _scheduleService;
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IInvoiceService _invoiceService;
        private readonly IAffiliateService _affiliateService;
        private readonly IInfusionSoftConfiguration _infusionSoftConfiguration;
        private readonly IContactService _contactService;
        private readonly ILoginSocketStore _loginSocketStore;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public UserService(
            ClaimsPrincipal principal,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            IOptions<AccountConfirmationSetting> accountConfirmationSetting,
            IRepo repo,
            IEmailService emailService,
            IOptions<EmailTemplates> emailTemplates,
            IScheduleService scheduleService,
            ISendGridService sendGridService,
            INotificationSettingsService notificationSettingsService,
            IInvoiceService invoiceService,
            IAffiliateService affiliateService,
            IInfusionSoftConfiguration infusionSoftConfiguration,
            IContactService contactService,
            ILoginSocketStore loginSocketStore,
            IStorageService storageService,
            IMapper mapper)
        {
            _principal = principal;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _accountConfirmationSetting = accountConfirmationSetting.Value;
            _repo = repo;
            _emailService = emailService;
            _emailTemplates = emailTemplates.Value;
            _scheduleService = scheduleService;
            _notificationSettingsService = notificationSettingsService;
            _sendGridService = sendGridService;
            _invoiceService = invoiceService;
            _affiliateService = affiliateService;
            _infusionSoftConfiguration = infusionSoftConfiguration;
            _contactService = contactService;
            _loginSocketStore = loginSocketStore;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task AddRole(AddRoleDto data)
        {
            var user = await FindUserByEmailSafely(data.Email);
            await EnsureRoleIsExists(data.RoleName);
            await _userManager.AddToRoleAsync(user, data.RoleName);
        }

        public async Task DeleteUser(Guid userGuid)
        {
            if (_principal.GetUserId() == userGuid)
            {
                throw new ValidationException("Try to delete current account.");
            }
            using (var trx = _repo.Transaction())
            {
                var user = await trx.Track<User>()
                    .Include(i => i.ProccessingTasks)
                    .Include(i => i.RecepientNotifications)
                    .Include(i => i.SenderNotifications)
                    .Include(i => i.ReviewingTasks)
                    .Include(i => i.UserCreatedFormulaProjects)
                    .Include(i => i.UserCreatedFormulaTasks)
                    .Include(i => i.UserCreatedTasks)
                    .Include(i => i.UserCreatedProjects)
                    .FirstAsync(w => w.Id == userGuid);
                user.RecepientNotifications.Clear();
                user.SenderNotifications.Clear();
                var formulaprojects = await trx.Track<FormulaProject>()
                 .Where(w => w.OwnerGuid == userGuid).ToListAsync();
                var formulatasks = await trx.Track<FormulaTask>()
                   .Where(w => w.OwnerGuid == userGuid).ToListAsync();
                if (formulaprojects.Count != 0 || formulatasks.Count != 0)
                {
                    throw new ValidationException("This user has work in progress, can't delete user.");
                }
                var settings = await trx.Track<NotificationSetting>()
                    .Where(w => w.UserId == userGuid).ToListAsync();
                trx.RemoveRange(settings);
                var comments = await trx.Track<ProjectTaskComment>()
                    .Where(w => w.UserGuid == userGuid).ToListAsync();
                trx.RemoveRange(comments);
                await trx.SaveChangesAsync();
                foreach (var formulaProject in user.UserCreatedFormulaProjects)
                {
                    formulaProject.OwnerGuid = user.OwnerId.Value;
                }
                foreach (var formulaTask in user.UserCreatedFormulaTasks)
                {
                    formulaTask.OwnerGuid = user.OwnerId.Value;
                }
                foreach (var proccessingTask in user.ProccessingTasks)
                {
                    proccessingTask.ProccessingUserGuid = null;
                }
                foreach (var reviewingTask in user.ReviewingTasks)
                {
                    reviewingTask.ReviewingUserGuid = null;
                }
                foreach (var projectTask in user.UserCreatedTasks)
                {
                    projectTask.OwnerGuid = user.OwnerId.Value;
                }
                foreach (var project in user.UserCreatedProjects)
                {
                    project.OwnerGuid = user.OwnerId.Value;
                }
                await trx.SaveChangesAsync();
                trx.Remove(user);
                await _loginSocketStore.LogOff(user.Id);
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task SaveSignUpInfusionData(InfusionSignUpDto data)
        {
            var infusionSignUp = await _repo.Read<InfusionSignUp>()
                    .SingleOrDefaultAsync(t => t.Url == data.Url);

            if (infusionSignUp != null)
            {
                return;
            }

            infusionSignUp = _mapper.Map<InfusionSignUp>(data);

            await _repo.AddAsync(infusionSignUp);
            await _repo.SaveChangesAsync();
        }

        public async Task SignUp(SignUpDto data, string[] roles)
        {
            string LoggedInRoleName = roles[0];//Added Role Name here to use check of CompanyWorker.
            //var isVendor = roles.Any(r => r == UserRoles.Vendor);// added logic for new role CompanyWorker WRT Sprint 10B
            //var isVendor = roles.Any(r => r == UserRoles.Vendor || r == UserRoles.CompanyWorker);// added logic for new role CompanyWorker WRT Sprint 10B
            var isVendor = roles.Any(r => r == UserRoles.Vendor || r == UserRoles.CompanyWorker || r == UserRoles.Company);// added logic for new role Company WRT Sprint 10B
            await EnsureUserDataValid();

            var user = _mapper.Map<User>(data);
            user.UserName = user.Email;
            if (!isVendor)
                user.Business = new Business();

            await EnsureSuccessfulIdentityResult(
                () => _userManager.CreateAsync(user, data.Password));

            // await UpdateProfile(user, data);//Added Code Companyworker to pass rolename.
            await UpdateProfile(user, data, LoggedInRoleName);

            roles = roles ?? Array.Empty<string>();
            if (roles.Any())
            {
                try
                {
                    foreach (var role in roles)
                        await EnsureRoleIsExists(role);

                    await EnsureSuccessfulIdentityResult(
                        () => _userManager.AddToRolesAsync(user, roles));
                }
                catch (Exception)
                {
                    await _userManager.DeleteAsync(user);
                    throw new ValidationException("Cannot create user.");
                }
            }

            if (!isVendor)
            {
                await _scheduleService.DaySummary(user.Id);
                await _scheduleService.DailyToDoSummary(user.Id);
                //// PublishFormula
                //try
                //{
                //    var admin = await _repo.Track<User>()
                //       .Include(t => t.Profile)
                //       .Include(t => t.Roles)
                //       .ThenInclude(t => t.Role).Select(x => new UserProfileWithRoleDto
                //       {
                //           UserId = x.Id,
                //           Email = x.Email,
                //           FullName = x.Profile.FullName != null ? x.Profile.FullName : string.Empty,
                //           Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault(),
                //       }).Where(r => r.Role == "Admin").FirstOrDefaultAsync();

                //    var formulastatus = await _repo.ExecuteSql<PublishStatus>(_mapper, "usp_PublishFormulaForNewOnwer @UserGuid,@AdminGuid",
                //        new List<SqlParameter> {
                //            new SqlParameter { ParameterName = "@UserGuid", SqlDbType = SqlDbType.UniqueIdentifier, Value = user.Id },
                //            new SqlParameter { ParameterName = "@AdminGuid", SqlDbType = SqlDbType.UniqueIdentifier, Value = admin.UserId }
                //           }).ToListAsync();
                //}
                //catch (Exception ex)
                //{

                //}
            }



            await SendConfirmationEmail(user, _accountConfirmationSetting.ConfirmOwnerUrl);

            async Task EnsureUserDataValid()
            {
                if (data.Password != data.PasswordConfirm)
                    throw new ValidationException("Password confirmation does not match");

                try
                {
                    //Email address validation
                    new MailAddress(data.Email);
                }
                catch (Exception)
                {
                    throw new ValidationException("Email is not valid");
                }

                // Should be unique
                var existedUser = await _userManager.FindByEmailAsync(data.Email);
                if (existedUser != null)
                {
                    // await UpdateProfile(existedUser, data);//Commented code for to Pass RoleName.
                    await UpdateProfile(existedUser, data, LoggedInRoleName);
                    throw new ForbiddenException("User already exists", true);
                }
            }

            //async Task UpdateProfile(User userEntity, SignUpDto signUpDto)//Commented method name to handle login rolename
            async Task UpdateProfile(User userEntity, SignUpDto signUpDto, string rolename)
            {

                using (var trx = _repo.Transaction())
                {
                    var invoiceDetail = await _invoiceService.GetDetailAsync(signUpDto.OrderId);
                    var userFromDb = await trx.Track<User>().FirstAsync(x => x.Email == signUpDto.Email);
                    var profile = await trx.Track<UserProfile>().FirstAsync(x => x.UserId == userFromDb.Id);
                    userEntity.IsPayed = invoiceDetail.IsPayed;
                    profile.FullName = signUpDto.FullName;
                    profile.ContactId = invoiceDetail.ContactId;
                    if (rolename == "CompanyWorker")
                    {
                        profile.CompanyWorkerOwnerID = new Guid(signUpDto.CompanyWorkerOwnerId);
                    }
                    var infusionSignUp = await trx.Track<InfusionSignUp>()
                        .FirstOrDefaultAsync(t => t.OrderId == signUpDto.OrderId);

                    if (infusionSignUp != null)
                    {
                    infusionSignUp.ContactId = invoiceDetail.ContactId;
                    }

                    await _userManager.UpdateAsync(userEntity);
                    await _contactService.ApplyRegisterActionAsync(invoiceDetail.ContactId);
                    if (userEntity.EmailConfirmed)
                    {
                        await ConfinUserOnInfusionsoft(profile);
                    }
                    await trx.SaveAndCommitAsync(CancellationToken.None);
                }
            }
        }

        private async Task ConfinUserOnInfusionsoft(UserProfile profile)
        {
            await _contactService.ApplyConfirmActionAsync(profile.ContactId.Value);
            _mapper.Map(await _affiliateService.GetReferralAsync(profile.ContactId.Value), profile);
        }

        public async Task<ClaimsPrincipal> SignIn(SignInDto data)
        {
            var user = await FindUserByEmailSafely(data.Email);

            if (!user.EmailConfirmed)
                throw new ValidationException("Email is not confirmed.");

            if (user.IsLocked)
                throw new ValidationException("User is locked");

            var result = await _signInManager.CheckPasswordSignInAsync(user, data.Password, false);
            if (!result.Succeeded)
                throw new ValidationException("Wrong password");

            if (!await HasPayedAccount(user))
                throw new ValidationException("Sorry, you must have a paid account to proceed. Please contact support@i-autom8.com to activate your account.");

            return await _signInManager.CreateUserPrincipalAsync(user);
        }

        private async Task<bool> HasPayedAccount(User user)
        {
            Guid userId;
            if (await _userManager.IsInRoleAsync(user, UserRoles.Owner))
                userId = user.Id;
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Manager))
                userId = user.OwnerId.Value;
            else if (await _userManager.IsInRoleAsync(user, UserRoles.Worker))
                userId = user.OwnerId.Value;
            else return true;
            var profile = await _repo.Read<UserProfile>().FirstAsync(x => x.UserId == userId);

            //return profile.ContactId.HasValue && profile.ContactId.Value != 0 ? true : false;

            // Only allow the users who have a valid contactId
            // Don't allow the users to logic having contactId == 0 or contactId != 1
            return profile.ContactId.HasValue && profile.ContactId.Value > 0 ? true : false;
        }

        public async Task<ClaimsPrincipal> RefreshUser()
        {
            var user = await _userManager.GetUserAsync(_principal);
            return await _signInManager.CreateUserPrincipalAsync(user);
        }

        public async Task<ClaimsPrincipal> RefreshNewUser(ClaimsPrincipal claim)
        {
            var user = await _userManager.GetUserAsync(claim);
            return await _signInManager.CreateUserPrincipalAsync(user);
        }

        private async Task EnsureRoleIsExists(string roleName)
        {
            if (!UserRoles.IsRoleDefined(roleName))
                throw new ValidationException($"Invalid role name: {roleName}.");

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                await EnsureSuccessfulIdentityResult(
                    () => _roleManager.CreateAsync(new Role { Name = roleName }));
            }
        }

        public async Task<ProfileDto> GetUserProfile(Guid userId)
        {
            var userProfile = await _repo.Read<UserProfile>()
                .Include(up => up.User)
                    .ThenInclude(up => up.Business)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            var result = new ProfileDto
            {
                UserProfile = _mapper.Map<UserProfileDto>(userProfile),
                IsVendor = _principal.IsVendor()
            };

            // get the user profile image link, if exists
            if (!String.IsNullOrWhiteSpace(userProfile.Path))
            {
                result.UserProfile.ProfileImage = _storageService.GetProfileImageUri($"{userId}/{userProfile.Path}");
            }
            else
            {
                result.UserProfile.ProfileImage = String.Empty;
            }

            if (userProfile.User.Business != null && await _userManager.IsInRoleAsync(userProfile.User, UserRoles.Owner))
            {
                result.BusinessProfile = _mapper.Map<BusinessProfileDto>(userProfile.User.Business);
                _mapper.Map(userProfile, result.BusinessProfile,
                opts =>
                {
                    opts.Items.Add("GoldAffUrl", _infusionSoftConfiguration.GoldAffiliateUrl);
                    opts.Items.Add("SilverAffUrl", _infusionSoftConfiguration.SilverAffiliateUrl);
                    opts.Items.Add("AffLoginUrl", _infusionSoftConfiguration.AffiliateProgramUrl);
                });
                result.BusinessProfile.NotificationSettings = _mapper.Map<List<NotificationSettingDto>>(
                    await _notificationSettingsService.GetBussinessSettingsAsync(userProfile.User.Id));
            }

            return result;
        }

        public async Task<ClaimsPrincipal> UpdateUserProfile(Guid userId, ProfileDto profile)
        {
            using (var trx = _repo.Transaction())
            {
                var up = await trx.Track<UserProfile>()
                    .Include(x => x.User)
                    .ThenInclude(x => x.Business)
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (up == null)
                    throw new ValidationException("User profile is not found.");

                var role = _principal.GetUserRole();

                if (role.Equals("Vendor"))
                {
                    if (String.IsNullOrWhiteSpace(profile.UserProfile.ProfileImage) || String.IsNullOrEmpty(profile.UserProfile.ProfileImage))
                    {
                        throw new ValidationException("Profile image is required");
                    }
                }

                if (up.User.Business != null && profile.BusinessProfile != null)
                {
                    if (up.User.Business.ToDoSummaryTime != profile.BusinessProfile.ToDoSummaryTime)
                    {
                        await _scheduleService.DailyToDoSummary(up.UserId, profile.BusinessProfile.ToDoSummaryTime);
                    }
                    _mapper.Map(profile.BusinessProfile, up.User.Business);
                    if (_notificationSettingsService.IsSettingsChanged(
                        await _notificationSettingsService.GetBussinessSettingsAsync(up.UserId),
                        profile.BusinessProfile.NotificationSettings))
                    {
                        await _notificationSettingsService.UpdateBusinessSettingsAsync(up.UserId,
                            profile.BusinessProfile.NotificationSettings);
                    }
                }

                var userSkill = await trx.Track<UserSkill>()
                    .Include(i => i.Skill)
                    .Where(w => w.UserId == userId)
                    .FirstOrDefaultAsync();

                if (userSkill != null && userSkill.Skill != null)
                {
                    userSkill.Skill.Name = profile.UserProfile.FullName;
                }

                _mapper.Map(profile.UserProfile, up);

                await trx.SaveAndCommitAsync();
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return await _signInManager.CreateUserPrincipalAsync(user);
        }

        public async Task<List<UserDropdownItemDto>> GetUsers()
        {
            return await _repo.Read<UserProfile>()
                .Include(x => x.User)
                .Where(p => !p.User.IsLocked)
                .Select(x => _mapper.Map<UserDropdownItemDto>(x))
                .ToListAsync();
        }

        public async Task<List<UserFilterItemDto>> GetUsersForAllProjects(IEnumerable<int> projectIds)
        {
            if (!projectIds.Any())
                return new List<UserFilterItemDto>();
            var userId = _principal.GetUserId();
            var accessQuery = _repo
                .Read<Project>()
                .Where(f =>
                    projectIds.Contains(f.Id));

            //TODO need to refactor this for now EF Core doesnt support IQueryable Union #246
            var isAssignedAsManager = true;

            if (_principal.IsOwner())
            {
                accessQuery = accessQuery.Where(w => w.OwnerGuid == userId);
            }
            else if (_principal.IsManager())
            {
                isAssignedAsManager = await _repo.Read<UserProject>().Where(w => w.UserId == userId && projectIds.Contains(w.ProjectId)).AnyAsync();
            }
            else
            {
                accessQuery = accessQuery
                    .Join(_repo.Read<ProjectTaskUser>()
                        .Where(w => w.UserId == userId && w.ProjectTaskUserType == ProjectTaskUserType.Assigned)
                        .Select(s => s.ProjectTaskId),
                        inner => inner.Id, outer => outer,
                        (project, userProject) => project);
            }

            if (!await accessQuery.AnyAsync() && isAssignedAsManager)
                throw new ForbiddenException(shouldRedirect: true);

            var assignedUsers = _repo.Read<ProjectTask>()
                .Include(t => t.ProjectTaskUsers)
                .Where(c => projectIds.Contains(c.ProjectId))
                .SelectMany(t => t.ProjectTaskUsers)
                .Include(t => t.User)
                .ThenInclude(t => t.Profile)
                .Include(t => t.User)
                    .ThenInclude(t => t.Roles)
                        .ThenInclude(t => t.Role)
                .ToList();

            var usersDictionary = assignedUsers
                .GroupBy(t => t.UserId)
                .ToDictionary(t => t.Key, t => (FullName: t.First().User.Profile.FullName,
                    Roles: t.First().User.Roles.Select(r => r.Role.Name).ToList()));

            var userFilterItemDtos = usersDictionary.Select(t => new UserFilterItemDto
            {
                UserId = t.Key,
                FullName = t.Value.FullName,
                Roles = t.Value.Roles
            }).ToList();

            return userFilterItemDtos;
        }

        public async Task<List<UserDropdownItemDto>> GetOwnerUsers(Guid ownerGuid)
        {
            return await _repo.Read<UserProfile>()
                .Include(x => x.User)
                .Where(p => (!p.User.IsLocked && p.User.OwnerId == ownerGuid) || p.User.Id == ownerGuid)
                .Select(x => _mapper.Map<UserDropdownItemDto>(x))
                .ToListAsync();
        }

        public async Task ChangeRoleTo(AssigneeUserDto userDto, string roleTo)
        {
            var user = await FindUserByEmailSafely(userDto.Email);
            await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
            await EnsureRoleIsExists(roleTo);
            await _userManager.AddToRoleAsync(user, roleTo);
            await _userManager.UpdateAsync(user);
        }

        public async Task CreateAssigneeUser(string ownerUser, AssigneeUserDto userDto)
        {
            var currentUser = await FindUserByEmailSafely(ownerUser);
            var owner = currentUser.OwnerId.HasValue
                ? await FindUserByIdSafely(currentUser.OwnerId.Value)
                : currentUser;


            var assignee = await _userManager.FindByEmailAsync(userDto.Email);

            if (assignee != null)
                throw new ValidationException("User already exists");

            if (string.IsNullOrEmpty(userDto.Role))
                userDto.Role = UserRoles.Worker;

            var user = new User
            {
                Email = userDto.Email,
                UserName = userDto.Email,
                OwnerId = owner.Id,
                Profile = new UserProfile { FullName = userDto.FullName }
            };

            await EnsureSuccessfulIdentityResult(
                () => _userManager.CreateAsync(user, _accountConfirmationSetting.DefaultPassword));

            await EnsureRoleIsExists(userDto.Role);
            await _userManager.AddToRoleAsync(user, userDto.Role);

            if (owner.Profile.FullName == null)
            {
                var userObj = await _repo.Read<User>()
                .Include(i => i.Profile)
                .FirstOrDefaultAsync(w => w.Id == owner.Id);

                owner.Profile.FullName = userObj.Profile.FullName;
            }

            await SendConfirmationEmail(user, owner, _accountConfirmationSetting.ConfirmUrl);
        }

        public async Task ResendConfirmationEmail(Guid userGuid)
        {
            var user = await FindUserByIdSafely(userGuid);
            if (user.EmailConfirmed)
                throw new ValidationException("Email has been already confirmed.");

            await SendConfirmationEmail(user, _accountConfirmationSetting.ConfirmUrl);
        }

        public async Task<List<CompanyUserDto>> GetOwnerUsersWithRoles(string currentUser)
        {
            var user = await FindUserByEmailSafely(currentUser);
            var owner = user.OwnerId.HasValue
                ? await FindUserByIdSafely(user.OwnerId.Value)
                : user;

            var users = await _repo.Read<User>()
                .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Profile)
                .Where(u => u.OwnerId == owner.Id || u.Id == owner.Id)
                .ToListAsync();

            return users.Select(_mapper.Map<CompanyUserDto>).ToList();
        }

        public async Task SendForgotPasswordMessage(ForgotPasswordDto model)
        {
            var user = await FindUserByEmailSafely(model.Email);

            if (user.IsLocked)
                throw new ValidationException("User is locked.");

            if (!user.EmailConfirmed)
                throw new ValidationException("Email is not confirmed.");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = string.Format(_accountConfirmationSetting.ForgotPasswordUrl, WebUtility.UrlEncode(code), WebUtility.UrlEncode(model.Email));
            await _sendGridService.SendMessage(model.Email, _emailTemplates.EmailNotification, "Reset password",
                    new Dictionary<string, string> { { "{{NotificationText}}",
                            "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>" } });
        }

        public async Task<ClaimsPrincipal> ChangeForgotPasswordMessage(ForgotPasswordSubmitDto model)
        {
            var user = await FindUserByEmailSafely(WebUtility.UrlDecode(model.Email));

            if (user.IsLocked)
                throw new ValidationException("User is locked.");

            if (model.Password != model.ConfirmPassword)
                throw new ValidationException("Password confirmation does not match");

            await EnsureSuccessfulIdentityResult(
                () => _userManager.ResetPasswordAsync(user, WebUtility.UrlDecode(model.Code), model.Password));

            return await _signInManager.CreateUserPrincipalAsync(user);
        }

        public async Task<ClaimsPrincipal> ConfirmUser(ConfirmUserDto model)
        {
            try
            {
                var user = await FindUserByEmailSafely(WebUtility.UrlDecode(model.Email));

                if (user.IsLocked)
                    throw new ValidationException("User is locked.");

                await EnsureSuccessfulIdentityResult(
                    () => _userManager.ConfirmEmailAsync(user, WebUtility.UrlDecode(model.Code)));

                //if (!await _userManager.IsInRoleAsync(user, UserRoles.Owner) && // added logic for new role CompanyWorker WRT Sprint 10B
                //!await _userManager.IsInRoleAsync(user, UserRoles.Vendor)) // added logic for new role CompanyWorker WRT Sprint 10B
                //if (!await _userManager.IsInRoleAsync(user, UserRoles.Owner) &&
                //    (!await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || !await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker))) // added logic for new role CompanyWorker WRT Sprint 10B
                if (!await _userManager.IsInRoleAsync(user, UserRoles.Owner) &&
                    (!await _userManager.IsInRoleAsync(user, UserRoles.Vendor) || !await _userManager.IsInRoleAsync(user, UserRoles.CompanyWorker) || !await _userManager.IsInRoleAsync(user, UserRoles.Company))) // added logic for new role Company WRT Sprint 10B
                {
                    await EnsureSuccessfulIdentityResult(
                    () => _userManager.ChangePasswordAsync(user, _accountConfirmationSetting.DefaultPassword, model.Password));
                }
                else
                {
                    var profile = await _repo.Track<UserProfile>().Where(w => w.UserId == user.Id)
                        .FirstAsync();
                    await ConfinUserOnInfusionsoft(profile);
                    await _repo.SaveChangesAsync();
                }

                return await _signInManager.CreateUserPrincipalAsync(user);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<Guid> SetLockStatusForUser(Guid userGuid, bool locked = true)
        {
            using (var trx = _repo.Transaction())
            {
                var user = await trx.Track<User>().FirstOrDefaultAsync(u => u.Id == userGuid);
                if (user == null) throw new ValidationException("User does not exist.");
                user.IsLocked = locked;
                await trx.SaveAndCommitAsync();
                return user.Id;
            }
        }

        #region Helpers

        private async Task<User> FindUserByIdSafely(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new ValidationException("User does not exist");
            return user;
        }

        private async Task<User> FindUserByEmailSafely(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new ValidationException($"User {email} does not exist");
            return user;
        }

        private async Task EnsureSuccessfulIdentityResult(Func<Task<IdentityResult>> identityAction)
        {
            var result = await identityAction();
            if (!result.Succeeded)
                throw new ValidationException(string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        private async Task SendConfirmationEmail(User user, User ownerUser, string confirmUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = string.Format(confirmUrl,
                WebUtility.UrlEncode(code),
                WebUtility.UrlEncode(user.Email));

            var substitutions = new Dictionary<string, string>
                {
                    { "{{InvitedUserName}}", user.Profile.FullName },
                    { "{{InvitorName}}", ownerUser.Profile.FullName },
                    { "{{SingInUrl}}", callbackUrl}
                };

            await _sendGridService.SendMessage(user.Email, _emailTemplates.SignIn, "Verify your email", substitutions);
        }

        private async Task SendConfirmationEmail(User user, string confirmUrl)
        {
            var profile = await _repo.Read<UserProfile>().FirstAsync(x => x.UserId == user.Id);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = string.Format(confirmUrl,
                WebUtility.UrlEncode(code),
                WebUtility.UrlEncode(user.Email));

            var substitutions = new Dictionary<string, string>
                {
                    { "{{InvitedUserName}}", profile.FullName },
                    { "{{SingInUrl}}", callbackUrl}
                };

            await _sendGridService.SendMessage(user.Email, _emailTemplates.SignUp, "Verify your email", substitutions);
        }

        public async Task<List<UserProfileWithRoleDto>> GetAllUserWithRole()
        {
            var users = await _repo.Track<User>()
                .Include(t => t.Profile)
                    .Include(t => t.Roles)
                        .ThenInclude(t => t.Role).Select(x => new UserProfileWithRoleDto
                        {
                            UserId = x.Id,
                            Email = x.Email,
                            FullName = x.Profile.FullName != null ? x.Profile.FullName : string.Empty,
                            EmailConfirmed = x.EmailConfirmed,
                            Role = x.Roles.Select(r => r.Role.Name).FirstOrDefault() != null ? x.Roles.Select(r => r.Role.Name).FirstOrDefault() : string.Empty,
                            ManagerCount = (_repo.Track<User>().Include(t => t.Profile).Include(t => t.Roles).ThenInclude(t => t.Role).
                            Where(s => s.OwnerId == x.Id && (s.Roles.Select(y => y.Role.Name).FirstOrDefault() == "Manager")).Count()),
                            WorkerCount = (_repo.Track<User>().Include(t => t.Profile).Include(t => t.Roles).ThenInclude(t => t.Role).
                            Where(s => s.OwnerId == x.Id && (s.Roles.Select(y => y.Role.Name).FirstOrDefault() == "Worker")).Count()),
                            TotalCount = (_repo.Track<User>().Include(t => t.Profile).Include(t => t.Roles).ThenInclude(t => t.Role).
                            Where(s => s.OwnerId == x.Id && (s.Roles.Select(y => y.Role.Name).FirstOrDefault() == "Manager")).Count())
                            +
                            (_repo.Track<User>().Include(t => t.Profile).Include(t => t.Roles).ThenInclude(t => t.Role).
                            Where(s => s.OwnerId == x.Id && (s.Roles.Select(y => y.Role.Name).FirstOrDefault() == "Worker")).Count())

                        }
                        ).ToListAsync();

            return users;
        }

        public async Task<User> ChangeEmail(ChangeEmail changeEmail)
        {
            var users = await _repo.Read<User>().Select(x => new
            {
                x.Email
            }).ToListAsync();
            bool isEmailExist = users.Any(x => x.Email.ToLower() == changeEmail.newEmail.ToLower());
            if (isEmailExist)
            {
                throw new ValidationException($"This {changeEmail.newEmail} is already exist");
            }

            var user = await FindUserByEmailSafely(changeEmail.eMail);
            // var user =await _repo.Read<User>().Where(x => x.Email == changeEmail.eMail).FirstOrDefaultAsync();
            if (user == null)
                throw new ValidationException($"User {changeEmail.eMail} does not exist");

            //User aspUser = await _userManager.FindByIdAsync(user.Id.ToString());
            user.Email = changeEmail.newEmail.ToLower();
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ValidationException($"{ result.Errors.Select(x => x.Description).FirstOrDefault()}");
            return user;
        }
        public async Task ResendConfirmationEmailByAdmin(string eMail)
        {
            var userById = await _repo.Read<User>().Where(x => x.Email == eMail).FirstOrDefaultAsync();
            if (userById == null)
                throw new ValidationException($"User {eMail} does not exist");
            //User aspUser = await _userManager.FindByIdAsync(userById.Id.ToString());
            var user = await FindUserByIdSafely(userById.Id);
            if (user.EmailConfirmed)
                throw new ValidationException("Email has been already confirmed.");

            await SendConfirmationEmail(user, _accountConfirmationSetting.ConfirmUrl);
        }
        public async Task<ClaimsPrincipal> UserSignInByAdmin(SignInDto data)
        {
            var user = await FindUserByEmailSafely(data.Email);
            return await _signInManager.CreateUserPrincipalAsync(user);
        }
        #endregion
    }
}
