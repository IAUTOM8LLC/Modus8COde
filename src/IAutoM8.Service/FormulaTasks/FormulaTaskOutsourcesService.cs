using AutoMapper;
using IAutoM8.Domain.Models.User;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global;
using IAutoM8.Global.Enums;
using IAutoM8.Repository;
using IAutoM8.Service.CommonDto;
using IAutoM8.Service.CommonService.Interfaces;
using IAutoM8.Service.FormulaTasks.Dto;
using IAutoM8.Service.FormulaTasks.Interfaces;
using IAutoM8.Service.Notification.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Service.FormulaTasks
{
    public class FormulaTaskOutsourcesService : IFormulaTaskOutsourcesService
    {
        private readonly IRepo _repo;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IDateTimeService _dateTimeService;

        public FormulaTaskOutsourcesService(IRepo repo,
            INotificationService notificationService,
            IMapper mapper,
            IDateTimeService dateTimeService)
        {
            _repo = repo;
            _notificationService = notificationService;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
        }

        public async Task CreateRequest(OutsourceRequestDto requestDto)
        {
            using (var trx = _repo.Transaction())
            {
                var now = _dateTimeService.NowUtc;
                var curOutsources = await trx.Track<FormulaTaskVendor>().Where(w => w.FormulaTaskId == requestDto.TaskId)
                    .ToListAsync();
                var availableOutsourcesList = await GetOutsources(requestDto.TaskId, 0, 200); // Change the last variable = COUNT of total outsources.
                foreach (var vendor in curOutsources)
                {
                    //Below Line of Code is Added to Verify Role On basis of Selected or deselected outsourcers on 06-05-2021
                    var roleName = UserRoles.RoleName(trx.Read<UserRole>().Where(a => a.UserId == vendor.VendorGuid).Select(a => a.RoleId).FirstOrDefault());
                    // var roleName = availableOutsourcesList.Where(a => a.Id == vendor.VendorGuid).FirstOrDefault().Role;//Original Code-: Commeneted on -06-05-2021
                    if (roleName == "Vendor") // For Vendor
                    {
                        if (vendor.Status == FormulaRequestStatus.None &&
                              requestDto.Outsources.Any(a => !a.IsSelected && a.Id == vendor.VendorGuid))
                        {
                            vendor.Status = FormulaRequestStatus.DeclinedByOwner;
                            vendor.LastModified = now;
                        }
                        else if (vendor.Status == FormulaRequestStatus.DeclinedByOwner &&
                             requestDto.Outsources.Any(a => a.IsSelected && a.Id == vendor.VendorGuid))
                        {
                            vendor.Status = FormulaRequestStatus.None;
                            vendor.LastModified = now;
                        }
                    }

                    if (roleName == "CompanyWorker") // For Company Worker
                    {
                        // For CompanyWorker
                        if ((vendor.Status == FormulaRequestStatus.None
                            || vendor.Status == FormulaRequestStatus.WaitingForCompanyApproval) &&
                            requestDto.Outsources.Any(a => !a.IsSelected && a.Id == vendor.VendorGuid)) // For CompanyWorker
                        {
                            vendor.Status = FormulaRequestStatus.DeclinedByOwner;
                            vendor.LastModified = now;
                        }
                        else if (vendor.Status == FormulaRequestStatus.DeclinedByOwner &&
                             requestDto.Outsources.Any(a => a.IsSelected && a.Id == vendor.VendorGuid))
                        {
                            vendor.Status = FormulaRequestStatus.WaitingForCompanyApproval;
                            vendor.LastModified = now;
                        }

                        //var userProfile = trx.Track<UserProfile>().Where(w => w.UserId == vendor.VendorGuid).FirstOrDefault();

                        ////For Company
                        //if ((vendor.Status == FormulaRequestStatus.None
                        //    || vendor.Status == FormulaRequestStatus.WaitingForCompanyApproval
                        //    || vendor.Status == FormulaRequestStatus.DeclinedByOwner) &&
                        //    requestDto.Outsources.Any(a => !a.IsSelected && a.Id == userProfile.CompanyWorkerOwnerID)) // For Company
                        //{
                        //    vendor.Status = FormulaRequestStatus.DeclinedByOwner;
                        //    vendor.LastModified = now;
                        //}

                        //else if (vendor.Status == FormulaRequestStatus.DeclinedByOwner &&
                        //         requestDto.Outsources.Any(a => a.IsSelected && a.Id == userProfile.CompanyWorkerOwnerID))
                        //{
                        //    vendor.Status = FormulaRequestStatus.None;
                        //    vendor.LastModified = now;
                        //}
                    }

                    if (roleName == "Company") // For Company 
                    {
                        var userProfile = trx.Track<UserProfile>().Where(w => w.UserId == vendor.VendorGuid).FirstOrDefault();

                        //For Company
                        if (vendor.Status == FormulaRequestStatus.None
                            //|| vendor.Status == FormulaRequestStatus.DeclinedByOwner)
                            //&&requestDto.Outsources.Any(a => !a.IsSelected && a.Id == userProfile.UserId)
                            ) // For Company
                        {
                            vendor.Status = FormulaRequestStatus.DeclinedByOwner;
                            vendor.LastModified = now;
                        }

                        else if (vendor.Status == FormulaRequestStatus.DeclinedByOwner
                            //&& requestDto.Outsources.Any(a => a.IsSelected && a.Id == userProfile.UserId)
                            )
                        {
                            vendor.Status = FormulaRequestStatus.None;
                            vendor.LastModified = now;
                        }
                    }
                }
                foreach (var newVendor in requestDto.Outsources.Where(w => w.IsSelected && curOutsources.All(a => a.VendorGuid != w.Id)))
                {
                    if (newVendor.Role == "Vendor")
                    {
                        var vendorEntity = new FormulaTaskVendor
                        {
                            Created = now,
                            FormulaTaskId = requestDto.TaskId,
                            Status = FormulaRequestStatus.None,
                            LastModified = now,
                            VendorGuid = newVendor.Id
                        };
                        await trx.AddAsync(vendorEntity);
                        await trx.SaveChangesAsync();
                        await _notificationService.SendFormulaTaskOutsourcesAsync(trx, vendorEntity.Id);
                    }

                    if (newVendor.Role == "CompanyWorker")
                    {
                        var vendorEntity = new FormulaTaskVendor
                        {
                            Created = now,
                            FormulaTaskId = requestDto.TaskId,
                            Status = FormulaRequestStatus.WaitingForCompanyApproval,
                            LastModified = now,
                            VendorGuid = newVendor.Id
                        };

                        var userProfile = trx.Track<UserProfile>().Where(w => w.UserId == newVendor.Id).FirstOrDefault();

                        //var ownerIdToGuid = new Guid(newVendor.OwnerId);

                        var ownerEntity = new FormulaTaskVendor
                        {
                            Created = now,
                            FormulaTaskId = requestDto.TaskId,
                            Status = FormulaRequestStatus.None,
                            LastModified = now,
                            VendorGuid = Guid.Parse(Convert.ToString(userProfile.CompanyWorkerOwnerID)),
                            ChildCompanyWorkerID = newVendor.Id
                        };

                        await trx.AddAsync(ownerEntity);
                        await trx.AddAsync(vendorEntity);
                        await trx.SaveChangesAsync();
                        await _notificationService.SendFormulaTaskOutsourcesAsync(trx, ownerEntity.Id);
                        await _notificationService.SendFormulaTaskOutsourcesAsync(trx, vendorEntity.Id);

                    }
                }
                await trx.SaveAndCommitAsync();
            }
        }

        public async Task<IList<FormulaTaskOutsourceDto>> GetOutsources(int id, short skip, byte count)
        {
            var avgRespondingType = StatisticType.Responding;
            var avgWorkingType = StatisticType.Working;
            var avgRatingType = StatisticType.Rating;
            var avgMessagingType = StatisticType.Messaging;

            //            var sqlForSelectingOutSources = @"
            //SELECT TOP(@count) Id,FullName,Date,Status,Price,AvgResponding,AvgWorking,AvgRating,AvgMessaging,Role,OwnerId
            //FROM
            //(
            //	SELECT Id,FullName,Date,Status,Price,AvgResponding,AvgWorking,AvgRating,AvgMessaging, ROW_NUMBER ( )  OVER ( order by Status desc, FullName asc) as RowNumber, Role, OwnerId
            //	FROM
            //	(
            //		SELECT anu.Id,up.FullName,ftv.LastModified as Date,ftv.Status, ftv.Price, anr.Name as Role, anu.OwnerId
            //		FROM FormulaTaskVendor ftv
            //		INNER JOIN AspNetUsers anu on ftv.VendorGuid=anu.Id
            //        INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id -- changes made WRT Sprint 10B (NEW ROLES)
            //        INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId -- changes made WRT Sprint 10B (NEW ROLES)
            //		INNER JOIN UserProfile up on up.UserId=anu.Id
            //		WHERE ftv.FormulaTaskId=@id
            //		UNION
            //		SELECT anu.Id,up.FullName,null as Date, null as Status, null as Price, anr.Name as Role, anu.OwnerId
            //		FROM AspNetUsers anu
            //		INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
            //		INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name IN('Vendor','CompanyWorker') -- and anr.Name=@role -- changes made WRT Sprint 10B (NEW ROLES)
            //		INNER JOIN UserProfile up on up.UserId=anu.Id
            //		LEFT JOIN FormulaTaskVendor ftv on ftv.VendorGuid=anu.Id and ftv.FormulaTaskId=@id
            //		WHERE ftv.FormulaTaskId is null
            //	) outsources
            //	left join (
            //		select VendorGuid, AVG(cast (Value as float)) as AvgResponding 
            //		from FormulaTaskStatistic
            //		where [Value] is not null and [Type]=@avgRespondingType
            //		group by VendorGuid) VResponding on VResponding.VendorGuid = outsources.Id
            //	left join (
            //		select VendorGuid, AVG(cast (Value as float)) as AvgWorking 
            //		from FormulaTaskStatistic
            //		where [Value] is not null and [Type]=@avgWorkingType and FormulaTaskId=@id
            //		group by VendorGuid) VWorking on VWorking.VendorGuid = outsources.Id
            //	left join (
            //		select VendorGuid, AVG(cast (Value as float)) as AvgRating 
            //		from FormulaTaskStatistic
            //		where [Value] is not null and [Type]=@avgRatingType and FormulaTaskId=@id
            //		group by VendorGuid) VRating on VRating.VendorGuid = outsources.Id
            //	left join (
            //		select VendorGuid, AVG(cast (Value as float)) as AvgMessaging 
            //		from FormulaTaskStatistic
            //		where [Value] is not null and [Type]=@avgMessagingType
            //		group by VendorGuid) VMessaging on VMessaging.VendorGuid = outsources.Id
            //) sortedOutsources
            //WHERE sortedOutsources.RowNumber>@skip
            //ORDER BY sortedOutsources.RowNumber";

            //Above are the code for original Commenetd on 15 April 2021 for "sprint10c-sprint10b-sprint14admin-staging-v10" 
             var sqlForSelectingOutSources = @"SELECT TOP(@count) Id,FullName,Date,Status,Price,AvgResponding,AvgWorking,AvgRating,AvgMessaging,Role,OwnerId
FROM
(
	SELECT Id,FullName,Date,Status,Price,AvgResponding,AvgWorking,AvgRating,AvgMessaging, ROW_NUMBER ( )  OVER ( order by Status desc, FullName asc) as RowNumber, Role, OwnerId
	FROM
	(
		SELECT anu.Id,up.FullName,ftv.LastModified as Date,ftv.Status, ftv.Price, anr.Name as Role, anu.OwnerId
		FROM FormulaTaskVendor ftv
		INNER JOIN AspNetUsers anu on ftv.VendorGuid=anu.Id
        INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id -- changes made WRT Sprint 10B (NEW ROLES)
        INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId  and anr.Name IN('Vendor')-- changes made WRT Sprint 10B (NEW ROLES)
		INNER JOIN UserProfile up on up.UserId=anu.Id
		WHERE ftv.FormulaTaskId=@id

		UNION
		
		SELECT anu.Id,up.FullName,ftv.LastModified as Date,ftv.Status, 
		--ftv.Price, 
		FTV2.Price, 
		anr.Name as Role, anu.OwnerId
		FROM FormulaTaskVendor ftv
		INNER JOIN AspNetUsers anu on ftv.VendorGuid=anu.Id
        INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id -- changes made WRT Sprint 10B (NEW ROLES)
        INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId  and anr.Name IN('CompanyWorker')-- changes made WRT Sprint 10B (NEW ROLES)
		INNER JOIN UserProfile up on up.UserId=anu.Id

		INNER JOIN FormulaTaskVendor FTV2 ON FTV.VendorGuid = FTV2.ChildCompanyWorkerID AND FTV.FormulaTaskId = FTV2.FormulaTaskId

		WHERE ftv.FormulaTaskId=@id
		
		UNION
		
		SELECT anu.Id,up.FullName,null as Date, null as Status, null as Price, anr.Name as Role, anu.OwnerId
		FROM AspNetUsers anu
		INNER JOIN AspNetUserRoles anur on anur.UserId=anu.Id
		INNER JOIN AspNetRoles anr on anr.Id=anur.RoleId and anr.Name IN('Vendor','CompanyWorker') -- and anr.Name=@role -- changes made WRT Sprint 10B (NEW ROLES)
		INNER JOIN UserProfile up on up.UserId=anu.Id
		LEFT JOIN FormulaTaskVendor ftv on ftv.VendorGuid=anu.Id and ftv.FormulaTaskId=@id
		WHERE ftv.FormulaTaskId is null
	) outsources
	left join (
		select VendorGuid, AVG(cast (Value as float)) as AvgResponding 
		from FormulaTaskStatistic
		where [Value] is not null and [Type]=@avgRespondingType
		group by VendorGuid) VResponding on VResponding.VendorGuid = outsources.Id
	left join (
		select VendorGuid, AVG(cast (Value as float)) as AvgWorking 
		from FormulaTaskStatistic
		where [Value] is not null and [Type]=@avgWorkingType and FormulaTaskId=@id
		group by VendorGuid) VWorking on VWorking.VendorGuid = outsources.Id
	left join (
		select VendorGuid, AVG(cast (Value as float)) as AvgRating 
		from FormulaTaskStatistic
		where [Value] is not null and [Type]=@avgRatingType and FormulaTaskId=@id
		group by VendorGuid) VRating on VRating.VendorGuid = outsources.Id
	left join (
		select VendorGuid, AVG(cast (Value as float)) as AvgMessaging 
		from FormulaTaskStatistic
		where [Value] is not null and [Type]=@avgMessagingType
		group by VendorGuid) VMessaging on VMessaging.VendorGuid = outsources.Id
) sortedOutsources
WHERE sortedOutsources.RowNumber>@skip
ORDER BY sortedOutsources.RowNumber";
            return await _repo.ExecuteSql<FormulaTaskOutsourceDto>(_mapper, sqlForSelectingOutSources,
                new List<SqlParameter> {
                    new SqlParameter{ ParameterName = "@avgRespondingType", SqlDbType = SqlDbType.TinyInt, Value = avgRespondingType },
                    new SqlParameter{ ParameterName = "@avgWorkingType", SqlDbType = SqlDbType.TinyInt, Value = avgWorkingType },
                    new SqlParameter{ ParameterName = "@avgRatingType", SqlDbType = SqlDbType.TinyInt, Value = avgRatingType },
                    new SqlParameter{ ParameterName = "@avgMessagingType", SqlDbType = SqlDbType.TinyInt, Value = avgMessagingType },
                    new SqlParameter{ ParameterName = "@count", SqlDbType = SqlDbType.TinyInt, Value = count },
                    new SqlParameter{ ParameterName = "@skip", SqlDbType = SqlDbType.SmallInt, Value = skip },
                    new SqlParameter{ ParameterName = "@id", SqlDbType = SqlDbType.Int, Value = id },
                    //new SqlParameter{ ParameterName = "@role", SqlDbType = SqlDbType.VarChar, Value = UserRoles.Vendor } -- changes made WRT Sprint 10B (NEW ROLES)
                }).ToListAsync();
        }
    }
}
