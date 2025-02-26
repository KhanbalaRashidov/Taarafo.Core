﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the Good-Hearted Engineers
// FREE TO USE TO CONNECT THE WORLD
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Taarafo.Core.Models.GroupPosts;
using Taarafo.Core.Models.GroupPosts.Exceptions;
using Xunit;

namespace Taarafo.Core.Tests.Unit.Services.Foundations.GroupPosts
{
    public partial class GroupPostServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfGroupPostIsNullAndLogItAsync()
        {
            // given
            GroupPost nullGroupPost = null;

            var nullGroupPostException =
                new NullGroupPostException();

            var expectedGroupPostValidationException =
                new GroupPostValidationException(nullGroupPostException);

            // when
            ValueTask<GroupPost> addGrouPostTask =
                this.groupPostService.AddGroupPostAsync(nullGroupPost);

            GroupPostValidationException actualGroupPostValidationException =
                await Assert.ThrowsAsync<GroupPostValidationException>(
                    addGrouPostTask.AsTask);

            // then
            actualGroupPostValidationException.Should().BeEquivalentTo(
                expectedGroupPostValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedGroupPostValidationException))),
                      Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionOnAddIfGroupPostIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidGuid = Guid.Empty;

            var invalidGroupPost = new GroupPost
            {
                GroupId = invalidGuid,
                PostId = invalidGuid
            };

            var invalidGrouPostException =
                new InvalidGroupPostException();

            invalidGrouPostException.AddData(
                key: nameof(GroupPost.GroupId),
                values: "Id is required");

            invalidGrouPostException.AddData(
                key: nameof(GroupPost.PostId),
                values: "Id is required");

            var expectedGroupPostValidationException =
                new GroupPostValidationException(invalidGrouPostException);

            // when
            ValueTask<GroupPost> addGroupPostTask =
                this.groupPostService.AddGroupPostAsync(invalidGroupPost);

            GroupPostValidationException actualGroupPostValidationException =
                await Assert.ThrowsAsync<GroupPostValidationException>(
                    addGroupPostTask.AsTask);

            // then
            actualGroupPostValidationException.Should().BeEquivalentTo(
                expectedGroupPostValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedGroupPostValidationException))),
                        Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }
    }
}
