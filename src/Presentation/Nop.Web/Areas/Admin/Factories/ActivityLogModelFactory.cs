﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Models.Logging;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the activity log model factory implementation
    /// </summary>
    public partial class ActivityLogModelFactory : IActivityLogModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ActivityLogModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper)
        {
            this._baseAdminModelFactory = baseAdminModelFactory;
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare activity log type models
        /// </summary>
        /// <returns>List of activity log type models</returns>
        public virtual IList<ActivityLogTypeModel> PrepareActivityLogTypeModels()
        {
            //prepare available activity log types
            var availableActivityTypes = _customerActivityService.GetAllActivityTypes();
            var model = availableActivityTypes.Select(activityType => activityType.ToModel()).ToList();

            return model;
        }

        /// <summary>
        /// Prepare activity log search model
        /// </summary>
        /// <param name="model">Activity log search model</param>
        /// <returns>Activity log search model</returns>
        public virtual ActivityLogSearchModel PrepareActivityLogSearchModel(ActivityLogSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available activity log types
            _baseAdminModelFactory.PrepareActivityLogTypes(model.ActivityLogType);

            return model;
        }

        /// <summary>
        /// Prepare paged activity log list model
        /// </summary>
        /// <param name="searchModel">Activity log search model</param>
        /// <returns>Activity log list model</returns>
        public virtual ActivityLogListModel PrepareActivityLogListModel(ActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            
            //get parameters to filter log
            var startDateValue = searchModel.CreatedOnFrom == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = searchModel.CreatedOnTo == null ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //get log
            var activityLog = _customerActivityService.GetAllActivities(createdOnFrom: startDateValue,
                createdOnTo: endDateValue,
                activityLogTypeId: searchModel.ActivityLogTypeId,
                ipAddress: searchModel.IpAddress,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new ActivityLogListModel
            {
                Data = activityLog.Select(logItem =>
                {
                    //fill in model values from the entity
                    var logItemModel = logItem.ToModel();

                    //convert dates to the user time
                    logItemModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(logItem.CreatedOnUtc, DateTimeKind.Utc);

                    return logItemModel;

                }),
                Total = activityLog.TotalCount
            };

            return model;
        }

        #endregion
    }
}