// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Taarafo.Core.Brokers.Loggings;
using Taarafo.Core.Brokers.Storages;
using Taarafo.Core.Models.Groups;
using Taarafo.Core.Models.Groups.Exceptions;
using Xeptions;

namespace Taarafo.Core.Services.Foundations.Groups
{
    public partial class GroupService : IGroupService
    {
        private delegate ValueTask<Group> ReturningGroupFunction();
        private delegate IQueryable<Group> ReturningGroupsFunction();

        private async ValueTask<Group> TryCatch(ReturningGroupFunction returningGroupFunction)
        {
            try
            {
                return await returningGroupFunction();
            }
            catch (InvalidGroupException invalidGroupException)
            {
                throw CreateAndLogValidationException(invalidGroupException);
            }
            catch (NotFoundGroupException notFoundGroupException)
            {
                throw CreateAndLogValidationException(notFoundGroupException);
            }
            catch (SqlException sqlException)
            {
                var failedGroupStorageException =
                    new FailedGroupStorageException(sqlException);

                throw CreateAndLogCriticalDependencyException(failedGroupStorageException);
            }
        }

        private IQueryable<Group> TryCatch(ReturningGroupsFunction returningGroupsFunction)
        {
            try
            {
                return returningGroupsFunction();
            }
            catch (SqlException sqlException)
            {
                var failedGroupStorageException =
                    new FailedGroupStorageException(sqlException);

                throw CreateAndLogCriticalDependencyException(failedGroupStorageException);
            }
            catch (Exception exception)
            {
                var failedGroupServiceException =
                    new FailedGroupServiceException(exception);

                throw CreateAndLogServiceException(failedGroupServiceException);
            }
        }

        private GroupDependencyException CreateAndLogCriticalDependencyException(Xeption exception)
        {
            var groupDependencyException= new GroupDependencyException(exception);
            this.loggingBroker.LogCritical(groupDependencyException);

            return groupDependencyException;
        }

        private GroupServiceException CreateAndLogServiceException(Xeption exception)
        {
            var groupServiceException = new GroupServiceException(exception);
            this.loggingBroker.LogError(groupServiceException);

            return groupServiceException;
        }
        
        private GroupValidationException CreateAndLogValidationException(
            Xeption exception)
        {
            var groupValidationException =
                new GroupValidationException(exception);

            this.loggingBroker.LogError(groupValidationException);

            return groupValidationException;
        }
    }
}