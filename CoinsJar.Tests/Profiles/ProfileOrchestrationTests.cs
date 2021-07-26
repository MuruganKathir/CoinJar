//-----------------------------------------------------------------------
// <copyright file="ProfileOrchestrationTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.Profiles
{
    using System;
    using System.Data;
    using System.Linq;

    using CoinsJar.Tests.TestData;
    using CoinsJar.WebApi.Adapters;
    using CoinsJar.WebApi.Profiles;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Rhino.Mocks;

    /// <summary>
    /// Profile Orchestration Tests
    /// </summary>
    [TestClass]
    public class ProfileOrchestrationTests
    {
        /// <summary>
        /// The Profile is not found test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ProfileIsNotFound()
        {
            var correlationId = Guid.NewGuid().ToString();

            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(correlationId))
                        .Throw(new InvalidOperationException())
                        .Repeat.Once();

                    var profileOutcome = orchestration.GetProfile(correlationId);
                    Assert.Fail("Logically, can not reach this code");
                    return profileOutcome;
                },
                outcome => Assert.Fail("Logically, can not reach this code"));
        }

        /// <summary>
        /// The profile is found.
        /// </summary>
        [TestMethod]
        public void ProfileIsFound()
        {
            var correlationId = ObjectMother.ValidProfileFredNotCompleted.CorrelationId;
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(e => e.GetProfile(correlationId)).Repeat.Once().Return(ObjectMother.ValidProfileFredNotCompleted);
                        var profileOutcome = orchestration.GetProfile(correlationId);
                        return profileOutcome;
                    },
                outcome =>
                    {
                        Assert.AreEqual(true, outcome.IsSuccessful, "Result was successful");
                        Assert.IsNull(outcome.Errors, "Unexpected number of errors returned");
                        Assert.IsNotNull(outcome.Data);
                        Assert.AreEqual(correlationId, outcome.Data.CorrelationId);
                    });
        }

        /// <summary>
        /// Confirmation the email is email confirmed failed test.
        /// </summary>
        [TestMethod]
        public void ConfirmEmailIsEmailAddressConfirmedFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                       profileAdapter.Expect(
                            x => x.IsEmailAddressConfirmed(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(new DataOperationOutcome { IsSuccessful = false });

                        var outcome =
                            orchestration.ConfirmEmailAddress(
                                ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                                ObjectMother.EmailToken);

                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "EmailAddressAlreadyConfirmed"));
                    });
        }
        
        /// <summary>
        /// Enlists the new profile error correlation identifier in use.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileErrorCorrelationIdInUse()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        var correlationId = ObjectMother.ValidProfileFredNotCompleted.CorrelationId;
                        profileAdapter.Expect(e => e.IsCorrelationIdInUse(correlationId)).Repeat.Once().Return(true);
                        var profileOutcome = orchestration.EnlistNewProfile(
                            ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                            ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                            ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);
                        return profileOutcome;
                    },
                outcome =>
                    {
                        Assert.AreEqual(false, outcome.IsSuccessful);
                        Assert.AreEqual(1, outcome.Errors.Count());
                    });
        }

        /// <summary>
        /// Confirms the email confirm email token test.
        /// </summary>
        [TestMethod]
        public void ConfirmEmailConfirmEmailTokenTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(
                            x => x.IsEmailAddressConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(DataOperationOutcome.Success);                        
                        profileAdapter.Expect(
                            x =>
                            x.ConfirmEmailToken(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken))
                            .Repeat.Once()
                            .Return(new DataOperationOutcome { IsSuccessful = false });
                        var outcome =
                            profileOrchestration.ConfirmEmailAddress(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken);

                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "EmailTokenDoesNotMatch"));
                    });
        }

        /// <summary>
        /// Confirms the email confirm email token test.
        /// </summary>
        [TestMethod]
        public void ConfirmEmailConfirmEmailTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(
                            x => x.IsEmailAddressConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(DataOperationOutcome.Success);
                        profileAdapter.Expect(
                            x =>
                            x.ConfirmEmailToken(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken)).Repeat.Once().Return(DataOperationOutcome.Success);

                        profileAdapter.Expect(x => x.ConfirmEmail(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(new DataOperationOutcome { IsSuccessful = false });
                        var outcome =
                            profileOrchestration.ConfirmEmailAddress(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken);
                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));                        
                    });            
        }

        /// <summary>
        /// Enlists the new profile error identity number in use.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileErrorIdentityNumberInUse()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        var correlationId = ObjectMother.ValidProfileFredNotCompleted.CorrelationId;
                        profileAdapter.Expect(e => e.IsCorrelationIdInUse(correlationId)).Repeat.Once().Return(false);
                        profileAdapter.Expect(e => e.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                            .Repeat.Once()
                            .Return(true);

                        var profileOutcome = orchestration.EnlistNewProfile(
                            ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                            ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                            ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

                        return profileOutcome;
                    },
                outcome =>
                    {
                        Assert.AreEqual(false, outcome.IsSuccessful);
                        Assert.AreEqual(1, outcome.Errors.Count());
                    });
        }

        /// <summary>
        /// Enlists the new profile error email address in use.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileErrorEmailAddressInUse()
        {
            var correlationId = ObjectMother.ValidProfileFredNotCompleted.CorrelationId;
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(e => e.IsCorrelationIdInUse(correlationId)).Repeat.Once().Return(false);
                        profileAdapter.Expect(e => e.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress)).Repeat.Once().Return(true);

                        var profileOutcome = orchestration.EnlistNewProfile(
                            ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                            ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                            ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

                        return profileOutcome;
                    },
                outcome =>
                    {
                        Assert.AreEqual(false, outcome.IsSuccessful);
                        Assert.AreEqual(1, outcome.Errors.Count());
                    });
        }

        /// <summary>
        /// Enlists the new profile error identity and email address in use.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileErrorIdentityAndEmailAddressInUse()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(e => e.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                            .Repeat.Once()
                            .Return(true);
                        profileAdapter.Expect(e => e.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress))
                            .Repeat.Once()
                            .Return(true);

                        var profileOutcome = orchestration.EnlistNewProfile(
                            ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                            ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                            ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

                        return profileOutcome;
                    },
                outcome =>
                    {
                        Assert.AreEqual(false, outcome.IsSuccessful);
                        Assert.AreEqual(2, outcome.Errors.Count());
                    });
        }

        /// <summary>
        /// Enlists the new profile unexpected error.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileUnexpectedError()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.IsCorrelationIdInUse(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(false);
                    profileAdapter.Expect(e => e.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                        .Repeat.Once()
                        .Return(false);
                    profileAdapter.Expect(e => e.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress))
                        .Repeat.Once()
                        .Return(false);

                    profileAdapter.Expect(e => e.EnlistNewProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                    ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.CreateError("Unexpected error"));

                    var profileOutcome = orchestration.EnlistNewProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                        ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    Assert.AreEqual(1, outcome.Errors.Count());
                });
        }

        /// <summary>
        /// Enlists the new profile nominal.
        /// </summary>
        [TestMethod]
        public void EnlistNewProfileNominal()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.IsCorrelationIdInUse(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(false);
                    profileAdapter.Expect(e => e.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                        .Repeat.Once()
                        .Return(false);
                    profileAdapter.Expect(e => e.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress))
                        .Repeat.Once()
                        .Return(false);

                    profileAdapter.Expect(e => e.EnlistNewProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                    ObjectMother.ValidProfileFredNotCompleted.IdentityNumber))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var profileOutcome = orchestration.EnlistNewProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.ValidProfileFredNotCompleted.EmailAddress,
                        ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(true, outcome.IsSuccessful);
                    Assert.IsNull(outcome.Errors);
                });
        }

        /// <summary>
        /// Rollbacks the new profile nominal.
        /// </summary>
        [TestMethod]
        public void RollbackNewProfileNominal()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredNotCompleted);
                    
                    profileAdapter.Expect(e => e.DeleteProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var profileOutcome =
                        orchestration.RollbackEnlistedProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(true, outcome.IsSuccessful);
                    Assert.IsNull(outcome.Errors);
                });
        }

        /// <summary>
        /// Rollbacks the new profile if it does not exist.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void RollbackNewProfileDoesNotExist()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Throw(new InvalidOperationException())
                        .Repeat.Once();

                    profileAdapter.Expect(e => e.DeleteProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Repeat.Never();

                    var profileOutcome =
                        orchestration.RollbackEnlistedProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

                    Assert.Fail("Logically, can not reach this code");

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.Fail("Logically, can not reach this code");
                });
        }

        /// <summary>
        /// Rollbacks the new profile if it is completed.
        /// </summary>
        [TestMethod]
        public void RollbackNewProfileIsCompleted()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Return(ObjectMother.ValidProfileFredCompleted)
                        .Repeat.Once();

                    profileAdapter.Expect(e => e.DeleteProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Never();

                    var profileOutcome =
                        orchestration.RollbackEnlistedProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    Assert.AreEqual("NotEnlistedInTransaction", outcome.Errors.First().Code);
                });
        }

        /// <summary>
        /// Insert or Update the profile error not enlisted.
        /// </summary>
        [TestMethod]
        public void UpsertProfileErrorNotEnlisted()
        {
            this.EvaluateProfileOrchestrationCase(
                    (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(null);

                        var profileOutcome = orchestration.UpsertProfile(
                            ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                            ObjectMother.ValidUpdateableFred);

                        return profileOutcome;
                    },
                    outcome =>
                    {
                        Assert.AreEqual(false, outcome.IsSuccessful);
                        Assert.AreEqual(1, outcome.Errors.Count());
                    });
        }

        /// <summary>
        /// insert or update the profile error identity number not provided.
        /// </summary>
        [TestMethod]
        public void UpsertProfileErrorIdentityNumberNotProvided()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    var profileOutcome = orchestration.UpsertProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.InvalidUpdateableFredFieldsEmpty);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    Assert.AreEqual(1, outcome.Errors.Count());
                    Assert.AreEqual("EmailAddressNotProvided", outcome.Errors.First().Code);
                });
        }

        /// <summary>
        /// Insert or update the profile error fields don't match.
        /// </summary>
        [TestMethod]
        public void UpsertProfileErrorFieldsDontMatch()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredNotCompleted);

                    var profileOutcome = orchestration.UpsertProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.InvalidUpdateableFredFieldsDontMatch);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    ////Assert.AreEqual(2, outcome.Errors.Count());
                    Assert.AreEqual("EmailAddressDoNotMatch", outcome.Errors.First().Code);
                });
        }

        /// <summary>
        /// Updates the profile error invalid cell number.
        /// </summary>
        [TestMethod]
        public void UpsertProfileErrorInvalidCellNumber()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredNotCompleted);

                    var profileOutcome = orchestration.UpsertProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.InvalidUpdateableFredFieldsIncorrectCell);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "InvalidCellNumber"));
                });
        }

        /// <summary>
        /// Update the profile duplicate email address error.
        /// </summary>
        [TestMethod]
        public void UpsertProfileDuplicateEmailAddressError()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(e => e.IsEmailAddressInUse(ObjectMother.InvalidUpdateableFredFieldsDontMatch.EmailAddress))
                        .Repeat.Once()
                        .Return(true);

                    var profileOutcome = orchestration.UpsertProfile(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.InvalidUpdateableFredFieldsDontMatch);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(false, outcome.IsSuccessful);
                    Assert.AreEqual("DuplicateEmailAddress", outcome.Errors.First().Code);
                });
        }

        /// <summary>
        /// Insert or Updates the profile.
        /// </summary>
        [TestMethod]
        public void UpsertProfile()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                        .Return(ObjectMother.ValidProfileFredNotCompleted)
                        .Repeat.Once();

                    profileAdapter.Expect(e => e.UpsertProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId, ObjectMother.ValidUpdateableFred))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var profileOutcome = orchestration.UpsertProfile(
                        ObjectMother.ValidProfileFredNotCompleted.CorrelationId,
                        ObjectMother.ValidUpdateableFred);

                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.AreEqual(true, outcome.IsSuccessful);
                    Assert.IsNull(outcome.Errors);
                });
        }

        /// <summary>
        /// Gets the partial application invalid profile test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void GetPartialApplicationInvalidProfileTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId))
                    .Throw(new InvalidOperationException());    

                    var profileOutcome = orchestration.GetPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    Assert.Fail("Logically can not reach this code.");
                    return profileOutcome;
                },
                outcome =>
                {
                    Assert.Fail("Logically can not reach this code.");
                });
        }

        /// <summary>
        /// Gets the partial application none exists test.
        /// </summary>
        [TestMethod]
        public void GetPartialApplicationNoneExistsTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Return(ObjectMother.ValidProfileFredCompleted)
                        .Repeat.Once();

                    profileAdapter.Expect(e => e.GetPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Return(null)
                        .Repeat.Once();

                    var profileOutcome = orchestration.GetPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return profileOutcome;
                },
                outcome => Assert.IsFalse(outcome.IsSuccessful));
        }

        /// <summary>
        /// Adds the new partial application test.
        /// </summary>
        [TestMethod]
        public void AddNewPartialApplicationTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.UpdateProfileDetails(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ApplicationUpdateableExisting))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(e => e.IsBankIdValid(ObjectMother.ApplicationUpdateableExisting.BankId))
                        .Repeat.Once()
                        .Return(true);

                    profileAdapter.Expect(e => e.IsEthnicGroupIdValid(ObjectMother.ApplicationUpdateableExisting.EthnicGroupId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsAccountTypeIdValid(ObjectMother.ApplicationUpdateableExisting.AccountTypeId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsMaritalStatusIdValid(ObjectMother.ApplicationUpdateableExisting.MaritalStatusId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsHomeStatusIdValid(ObjectMother.ApplicationUpdateableExisting.HomeStatusId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsReasonForLoanIdValid(ObjectMother.ApplicationUpdateableExisting.LoanReasonId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsNextOfKinRelationshipIdValid(ObjectMother.ApplicationUpdateableExisting.NextOfKinRelationshipId))
                       .Repeat.Once()
                       .Return(true);
                    
                    profileAdapter.Expect(e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.EmployerProvinceId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsSalaryRuleIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryRuleId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsSalaryDateIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDateId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsWeekDaysIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDayId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsPaymentMethodIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryPaymentMethodId))
                       .Repeat.Once()
                       .Return(true);
                    
                    profileAdapter.Expect(e => e.IsNumberOfDependentsIdValid(ObjectMother.ApplicationUpdateableExisting.NumberOfDependentsId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsBankMonthsOpenValid(ObjectMother.ApplicationUpdateableExisting.BankMonthsOpenId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(e => e.IsEmploymentSectorValid(ObjectMother.ApplicationUpdateableExisting.EmploymentSectorId))
                       .Repeat.Once()
                       .Return(true);

                    profileAdapter.Expect(
                        e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.ProvinceId))
                        .Repeat.Once()
                        .Return(true);

                    profileAdapter.Expect(e => e.UpdateCellFromPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ApplicationUpdateableExisting.CellNumber))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(e => e.UpsertPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ApplicationUpdateableExisting))
                        .Return(DataOperationOutcome.Success)
                        .Repeat.Once();

                    var outcome = orchestration.UpsertPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.ApplicationUpdateableExisting);

                    return outcome;
                },
                outcome => Assert.IsTrue(outcome.IsSuccessful));
        }

        /// <summary>
        /// Update the partial application bank identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationBankIdError()
        {
            this.EvaluateProfileOrchestrationCase(
               (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
               {
                   profileAdapter.Expect(e => e.IsBankIdValid(ObjectMother.ApplicationUpdateableAllFields.BankId))
                        .Repeat.Once()
                        .Return(false);
                  
                   var outcome = orchestration.UpsertPartialApplication(
                       ObjectMother.ValidProfileFredCompleted.CorrelationId,
                       ObjectMother.ApplicationUpdateableAllFields);

                   return outcome;
               },
               outcome =>
               {
                   Assert.IsFalse(outcome.IsSuccessful);
                   Assert.IsTrue(outcome.Errors.Any(e => e.Code == "BankDoesNotExist"));
               });
        }

        /// <summary>
        /// Updates the partial application months bank open valid test.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationMonthsBankOpenValidTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
            {
                profileAdapter.Expect(e => e.IsBankMonthsOpenValid(ObjectMother.ApplicationUpdateableAllFields.BankMonthsOpenId))
                .Repeat.Once()
                .Return(false);

                var outcome = orchestration.UpsertPartialApplication(
                    ObjectMother.ValidProfileFredCompleted.CorrelationId,
                    ObjectMother.ApplicationUpdateableAllFields);
                return outcome;
            },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "BankMonthOpenDoesNotExist"));
                });
        }

        /// <summary>
        /// Update the partial application ethnic identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationEthnicIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsEthnicGroupIdValid(ObjectMother.ApplicationUpdateableAllFields.EthnicGroupId))
                       .Repeat.Once()
                       .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "EthnicGroupDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application marital status identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationMaritalStatusIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsMaritalStatusIdValid(ObjectMother.ApplicationUpdateableAllFields.MaritalStatusId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "MaritalStatusDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application home status identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationHomeStatusIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsHomeStatusIdValid(ObjectMother.ApplicationUpdateableAllFields.HomeStatusId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "HomeStatusDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application account type identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationAccountTypeIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsAccountTypeIdValid(ObjectMother.ApplicationUpdateableAllFields.AccountTypeId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "AccountTypeDoesNotExist"));
              });
        }

        /// <summary>
        /// Updates the partial application employment sector identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationEmploymentSectorIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsEmploymentSectorValid(ObjectMother.ApplicationUpdateableAllFields.EmploymentSectorId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "EmploymentSectorDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application method validate city and returns city id.
        /// </summary>
        [TestMethod, Ignore]
        public void UpsertPartialApplicationValidateCity()
        {
           this.EvaluateProfileOrchestrationCase(
               (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
               {
                   profileAdapter.Expect(e => e.ResolveProvinceId(ObjectMother.ApplicationUpdateableExisting.ProvinceId))
                        .Repeat.Once()
                        .Return(ObjectMother.ProvinceId);

                   profileAdapter.Expect(x => x.ResolveCityId(ObjectMother.ApplicationUpdateableExisting.City))
                       .Repeat.Once()
                       .Return(Guid.NewGuid());

                   var outcome = orchestration.UpsertPartialApplication(
                     ObjectMother.ValidProfileFredCompleted.CorrelationId,
                     ObjectMother.ApplicationUpdateableExisting);

                   return outcome;
               },
               outcome =>
               {
                   Assert.IsNotNull(outcome);
               });
        }

        /// <summary>
        /// Update the partial application validate suburb.
        /// </summary>
        [TestMethod, Ignore]
        public void UpsertPartialApplicationValidateSuburb()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.ResolveProvinceId(ObjectMother.ApplicationUpdateableAllFields.ProvinceId))
                        .Repeat.Once()
                        .Return(ObjectMother.ProvinceId);

                    profileAdapter.Expect(x => x.ResolveCityId(ObjectMother.ApplicationUpdateableAllFields.City))
                       .Repeat.Once()
                       .Return(Guid.NewGuid());

                    profileAdapter.Expect(x => x.ResolveSuburbId(ObjectMother.ApplicationUpdateableAllFields.Suburb, ObjectMother.CityId))
                        .Repeat.Once()
                        .Return(ObjectMother.SuburbId);

                    var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsNotNull(outcome);
                });
        }

        /// <summary>
        /// Updates the partial application validate province.
        /// </summary>
        [TestMethod, Ignore]
        public void UpsertPartialApplicationValidateProvince()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
            {
                profileAdapter.Expect(x => x.ResolveProvinceId(ObjectMother.ApplicationUpdateableAllFields.ProvinceId))
                    .Repeat.Once()
                    .Return(ObjectMother.ProvinceId);

                var outcome =
                    orchestration.UpsertPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.ApplicationUpdateableAllFields);
                return outcome;
            },
            outcome =>
            {
                Assert.IsNotNull(outcome);
            });
        }

        /// <summary>
        /// Updates the partial application number of dependents identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationNumberOfDependentsIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsNumberOfDependentsIdValid(ObjectMother.ApplicationUpdateableAllFields.NumberOfDependentsId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "NumberOfDependentsDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application salary frequency error test.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationSalaryRuleError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsSalaryRuleIdValid(ObjectMother.ApplicationUpdateableAllFields.SalaryRuleId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "SalaryFrequencyDoesNotExist"));
              });
        }

        /// <summary>
        /// Updates the partial application invalid cell number error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationInvalidCellNumberError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "InvalidCellNumber"));
              });
        }

        /// <summary>
        /// Updates the partial application invalid employer telephone number error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationInvalidEmployerTelephoneNumberError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "InvalidEmployerTelephoneNumber"));
              });
        }

        /// <summary>
        /// updates the partial application salary payment method error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationSalaryPaymentMethodError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.IsPaymentMethodIdValid(ObjectMother.ApplicationUpdateableAllFields.SalaryRuleId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "PaymentMethodDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application next of kin relationship identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationNextOfKinRelationshipIdError()
        {
            var application = ObjectMother.ApplicationUpdateableAllFields;

            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(
                      e => e.IsNextOfKinRelationshipIdValid(application.NextOfKinRelationshipId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      application);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "RelationshipDoesNotExist"));
              });
        }

        /// <summary>
        /// Update the partial application reason for loan identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationReasonForLoanIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(
                      e => e.IsReasonForLoanIdValid(ObjectMother.ApplicationUpdateableAllFields.LoanReasonId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "IncorrectLoanReason"));
              });
        }

        /// <summary>
        /// Update the partial application province identifier error.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationProvinceIdError()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(
                      e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableAllFields.EmployerProvinceId))
                      .Repeat.Once()
                      .Return(false);

                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableAllFields);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "ProvinceDoesNotExist"));
              });
        }

        /// <summary>
        /// Updates the partial application name and surname test.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationUpdateNameAndSurnameFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
            {
                profileAdapter.Expect(e => e.IsEthnicGroupIdValid(ObjectMother.ApplicationUpdateableExisting.EthnicGroupId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsMaritalStatusIdValid(ObjectMother.ApplicationUpdateableExisting.MaritalStatusId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsHomeStatusIdValid(ObjectMother.ApplicationUpdateableExisting.HomeStatusId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsNextOfKinRelationshipIdValid(ObjectMother.ApplicationUpdateableExisting.NextOfKinRelationshipId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.EmployerProvinceId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsSalaryRuleIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryRuleId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsPaymentMethodIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryPaymentMethodId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsNumberOfDependentsIdValid(ObjectMother.ApplicationUpdateableExisting.NumberOfDependentsId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.ProvinceId))
                    .Repeat.Once()
                    .Return(true);

                profileAdapter.Expect(e => e.IsSalaryDateIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDateId))
                     .Repeat.Once()
                     .Return(true);

                profileAdapter.Expect(e => e.IsWeekDaysIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDayId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsEmploymentSectorValid(ObjectMother.ApplicationUpdateableExisting.EmploymentSectorId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsBankIdValid(ObjectMother.ApplicationUpdateableExisting.BankId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsReasonForLoanIdValid(ObjectMother.ApplicationUpdateableExisting.LoanReasonId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsAccountTypeIdValid(ObjectMother.ApplicationUpdateableExisting.AccountTypeId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.IsBankMonthsOpenValid(ObjectMother.ApplicationUpdateableExisting.BankMonthsOpenId))
                   .Repeat.Once()
                   .Return(true);

                profileAdapter.Expect(e => e.UpdateProfileDetails(
                    ObjectMother.ValidProfileFredCompleted.CorrelationId, 
                    ObjectMother.ApplicationUpdateableExisting))
                .Repeat.Once()
                .Return(new DataOperationOutcome { IsSuccessful = false });

                var outcome = orchestration.UpsertPartialApplication(
                    ObjectMother.ValidProfileFredCompleted.CorrelationId,
                    ObjectMother.ApplicationUpdateableExisting);

                return outcome;
            },
            outcome =>
            {
                Assert.IsFalse(outcome.IsSuccessful);
                Assert.IsTrue(outcome.Errors.Any(e => e.Code == "UnexpectedInternalError"));
            });            
        }

        /// <summary>
        /// Updates the partial application failed to update cell number.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationFailedToUpdateCellNumber()
        {
            this.EvaluateProfileOrchestrationCase(
              (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
              {
                  profileAdapter.Expect(e => e.UpdateProfileDetails(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ApplicationUpdateableExisting))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                  profileAdapter.Expect(e => e.IsBankIdValid(ObjectMother.ApplicationUpdateableExisting.BankId))
                        .Repeat.Once()
                        .Return(true);

                  profileAdapter.Expect(e => e.IsEthnicGroupIdValid(ObjectMother.ApplicationUpdateableExisting.EthnicGroupId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsAccountTypeIdValid(ObjectMother.ApplicationUpdateableExisting.AccountTypeId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsMaritalStatusIdValid(ObjectMother.ApplicationUpdateableExisting.MaritalStatusId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsHomeStatusIdValid(ObjectMother.ApplicationUpdateableExisting.HomeStatusId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsReasonForLoanIdValid(ObjectMother.ApplicationUpdateableExisting.LoanReasonId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsNextOfKinRelationshipIdValid(ObjectMother.ApplicationUpdateableExisting.NextOfKinRelationshipId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.EmployerProvinceId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsSalaryRuleIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryRuleId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsPaymentMethodIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryPaymentMethodId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsNumberOfDependentsIdValid(ObjectMother.ApplicationUpdateableExisting.NumberOfDependentsId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(
                      e => e.IsProvinceIdValid(ObjectMother.ApplicationUpdateableExisting.ProvinceId))
                      .Repeat.Once()
                      .Return(true);

                  profileAdapter.Expect(e => e.IsSalaryDateIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDateId))
                       .Repeat.Once()
                       .Return(true);

                  profileAdapter.Expect(e => e.IsWeekDaysIdValid(ObjectMother.ApplicationUpdateableExisting.SalaryDayId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsBankMonthsOpenValid(ObjectMother.ApplicationUpdateableExisting.BankMonthsOpenId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(e => e.IsEmploymentSectorValid(ObjectMother.ApplicationUpdateableExisting.EmploymentSectorId))
                     .Repeat.Once()
                     .Return(true);

                  profileAdapter.Expect(
                      e => e.UpdateCellFromPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ApplicationUpdateableExisting.CellNumber))
                      .Repeat.Once()
                      .Return(new DataOperationOutcome { IsSuccessful = false });
                  
                  var outcome = orchestration.UpsertPartialApplication(
                      ObjectMother.ValidProfileFredCompleted.CorrelationId,
                      ObjectMother.ApplicationUpdateableExisting);

                  return outcome;
              },
              outcome =>
              {
                  Assert.IsFalse(outcome.IsSuccessful);
                  Assert.IsTrue(outcome.Errors.Any(e => e.Code == "UnexpectedInternalError"));
              });
        }

        /// <summary>
        /// Gets the partial application exists test.
        /// </summary>
        [TestMethod]
        public void GetPartialApplicationExistsTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Return(ObjectMother.ValidProfileFredCompleted)
                        .Repeat.Once();

                    profileAdapter.Expect(e => e.GetPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Return(ObjectMother.PartialApplicationAllFields)
                        .Repeat.Once();

                    var outcome = orchestration.GetPartialApplication(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsNotNull(outcome);
                    Assert.IsTrue(outcome.IsSuccessful);
                    Assert.IsNotNull(outcome.Data);
                });
        }

        /// <summary>
        /// Sends the confirmation SMS one time pin generated test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsOneTimePinGeneratedFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(
                        x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.GenerateOneTimePin())
                        .Repeat.Once()
                        .Return(null);
                    
                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "OneTimePinGenerationFailed"));
                });
        }

        /// <summary>
        ///  Saves one time pin test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsSaveOneTimePinFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(
                        x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.GenerateOneTimePin())
                        .Repeat.Once()
                        .Return(ObjectMother.OneTimePin);
                    
                    profileAdapter.Expect(
                        x => x.SaveOneTimePin(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.OneTimePin))
                                .Repeat.Once()
                                .Return(new DataOperationOutcome() { IsSuccessful = false });

                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Sends the confirmation SMS cell not found.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsCellNotFound()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                { 
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredNotCompleted);

                    var outcome = orchestration.SendConfirmationSms(
                                            ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "CellNumberNotProvided"));
                });
        }

        /// <summary>
        /// Sends the confirmation SMS failed test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);
                    
                    profileAdapter.Expect(
                        x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.GenerateOneTimePin())
                        .Repeat.Once()
                        .Return(ObjectMother.OneTimePin);
                    
                    profileAdapter.Expect(x => x.SaveOneTimePin(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.OneTimePin)).Repeat.Once().Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(x => x.UpdateOtpRequestCount(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                      .Repeat.Once()
                      .Return(DataOperationOutcome.Success);

                    templateAdapter.Expect(x => x.GetSmsTemplate())
                                       .Repeat.Once()
                                       .Return(ObjectMother.ValidSMSTemplate);

                    var message = ObjectMother.ValidSMSTemplate.Value.Replace("{pin}", ObjectMother.OneTimePin);

                    smsAdapter.Expect(
                        x => x.SendSms(ObjectMother.ValidProfileFredCompleted.CellNumber, message))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "ErrorSendingSms"));
                });
        }

        /// <summary>
        /// Sends the confirmation SMS send existing OTP when not expired test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsSendExistingOtpWhenNotExpiredTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredCompleted);

                        profileAdapter.Expect(x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                           .Repeat.Once()
                           .Return(DataOperationOutcome.Success);

                        profileAdapter.Expect(
                            x => x.GetExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.OneTimePin);

                        profileAdapter.Expect(
                            x => x.UpdateOtpRequestCount(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(DataOperationOutcome.Success);

                        templateAdapter.Expect(x => x.GetSmsTemplate())
                            .Repeat.Once()
                            .Return(ObjectMother.ValidSMSTemplate);

                        var message = ObjectMother.ValidSMSTemplate.Value.Replace("{pin}", ObjectMother.OneTimePin);

                        smsAdapter.Expect(
                            x => x.SendSms(ObjectMother.ValidProfileFredCompleted.CellNumber, message))
                            .Repeat.Once()
                            .Return(DataOperationOutcome.Success);

                        var outcome = orchestration.SendConfirmationSms(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                                return outcome;
                    },
                outcome =>
                    {
                        Assert.IsTrue(outcome.IsSuccessful);
                    });
        }

        /// <summary>
        /// Sends the confirmation SMS exceeded OTP request limit test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsExceededOtpRequestLimitTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredExceededOTPRequest.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredExceededOTPRequest);

                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredExceededOTPRequest.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "OTPRequestLimitExceeded"));
                });
        }

        /// <summary>
        /// Sends the confirmation SMS failed to update OTP request test.
        /// </summary>
        [TestMethod]

        public void SendConfirmationSmsFailedToUpdateOtpRequestTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(
                        x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(x => x.GenerateOneTimePin())
                       .Repeat.Once()
                       .Return(ObjectMother.OneTimePin);

                    profileAdapter.Expect(x => x.SaveOneTimePin(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.OneTimePin)).Repeat.Once().Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(x => x.UpdateOtpRequestCount(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                       .Repeat.Once()
                       .Return(new DataOperationOutcome { IsSuccessful = false });

                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Sends the confirmation SMS failed due to invalid template test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationSmsFailedDueToInvalidTemplateTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(
                        x => x.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.GenerateOneTimePin())
                        .Repeat.Once()
                        .Return(ObjectMother.OneTimePin);

                    profileAdapter.Expect(x => x.SaveOneTimePin(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.OneTimePin)).Repeat.Once().Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(x => x.UpdateOtpRequestCount(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                      .Repeat.Once()
                      .Return(DataOperationOutcome.Success);

                    templateAdapter.Expect(x => x.GetSmsTemplate())
                                       .Repeat.Once()
                                       .Return(null);

                    var message = ObjectMother.SmsTemplate.Replace("{PIN}", ObjectMother.OneTimePin);
                    
                    var outcome = orchestration.SendConfirmationSms(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "ErrorRetrievingMessageTemplate"));
                });
        }

        /// <summary>
        /// The test confirms if the cell number has already been confirmed.
        /// </summary>
        [TestMethod]
        public void ConfirmSmsCellAlreadyConfirmedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.IsCellConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var outcome = orchestration.ConfirmCellularNumber(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.OneTimePin);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(e => e.Code == "CellAlreadyConfirmed"));
                });
        }

        /// <summary>
        /// Confirms the SMS pin has expired test.
        /// </summary>
        [TestMethod]
        public void ConfirmSmsPinHasExpiredTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.IsCellConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                       .Repeat.Once()
                       .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(x => x.ConfirmationPin(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId,
                            ObjectMother.OneTimePin))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(x => x.OtpExpired(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var outcome = orchestration.ConfirmCellularNumber(
                       ObjectMother.ValidProfileFredCompleted.CorrelationId,
                       ObjectMother.OneTimePin);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);   
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "PinExpired"));
                });
        }

        /// <summary>
        /// Confirms the SMS pin does not match test.
        /// </summary>
        [TestMethod]
        public void ConfirmSmsPinDoesNotMatchTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.IsCellConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.ConfirmationPin(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.OneTimePin))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    var outcome = orchestration.ConfirmCellularNumber(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.OneTimePin);

                    return outcome;
                },
                outcome =>
                {
                    Assert.AreEqual(outcome.IsSuccessful, false);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "InvalidPin"));
                });
        }

        /// <summary>
        /// Sends the confirmation email email address not found test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailAddressNotFoundTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredWithoutEmail.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredWithoutEmail);

                        var outcome =
                            orchestration.SendConfirmationEmail(
                                ObjectMother.ValidProfileFredWithoutEmail.CorrelationId);
                       
                        return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(a => a.Code == "EmailAddressNotProvided"));
                });
        }

        /// <summary>
        /// Sends the confirmation email generate email token failed test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailGenerateEmailTokenFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredCompleted);

                        profileAdapter.Expect(x => x.GenerateEmailToken())
                            .Repeat.Once()
                            .Return(null);

                        var outcome = orchestration.SendConfirmationEmail(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId);

                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "GenerateEmailTokenFailed"));
                    });
        }

        /// <summary>
        /// Sends the confirmation email save token failed test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailSaveTokenFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredCompleted);

                        profileAdapter.Expect(x => x.GenerateEmailToken()).Repeat.Once().Return(ObjectMother.EmailToken);

                        profileAdapter.Expect(
                            x =>
                            x.SaveEmailToken(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken))
                            .Repeat.Once()
                            .Return(new DataOperationOutcome() { IsSuccessful = false });

                        var outcome =
                            orchestration.SendConfirmationEmail(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId);
                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "FailedToSaveTheEmailToken"));                        
                    });
        }

        /// <summary>
        /// Sends the confirmation email send email token test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailSendEmailFailTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                    {
                        profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                            .Repeat.Once()
                            .Return(ObjectMother.ValidProfileFredCompleted);

                        profileAdapter.Expect(x => x.GenerateEmailToken()).Repeat.Once().Return(ObjectMother.EmailToken);

                        profileAdapter.Expect(
                            x =>
                            x.SaveEmailToken(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                                ObjectMother.EmailToken))
                            .Repeat.Once()
                            .Return(DataOperationOutcome.Success);

                        templateAdapter.Expect(x => x.GetEmailBodyTemplate())
                                    .Repeat.Once()
                                    .Return(ObjectMother.ValidEmailBodyTemplate);

                        templateAdapter.Expect(x => x.GetEmailSubjectTemplate())
                                    .Repeat.Once()
                                    .Return(ObjectMother.ValidEmailSubjectTemplate);

                        emailAdapter.Expect(
                            x =>
                            x.SendEmail(
                                ObjectMother.ValidProfileFredCompleted.EmailAddress,
                                ObjectMother.EmailSubject,
                                ObjectMother.EmailBody))
                            .Repeat.Once()
                            .IgnoreArguments()
                            .Return(new DataOperationOutcome() { IsSuccessful = false });

                        var outcome =
                            orchestration.SendConfirmationEmail(
                                ObjectMother.ValidProfileFredCompleted.CorrelationId);

                        return outcome;
                    },
                outcome =>
                    {
                        Assert.IsFalse(outcome.IsSuccessful);
                        Assert.IsTrue(outcome.Errors.Any(x => x.Code == "SendConfirmationEmailFailed"));
                    });
        }

        /// <summary>
        /// Sends the confirmation email could not get template test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailCouldNotGetBodyTemplateTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(x => x.GenerateEmailToken()).Repeat.Once().Return(ObjectMother.EmailToken);

                    profileAdapter.Expect(
                        x =>
                        x.SaveEmailToken(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId,
                            ObjectMother.EmailToken))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                   templateAdapter.Expect(x => x.GetEmailBodyTemplate())
                                    .Repeat.Once()
                                    .Return(null);

                    var outcome =
                        orchestration.SendConfirmationEmail(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "ErrorRetrievingMessageTemplate"));
                });
        }

        /// <summary>
        /// Sends the confirmation email could not get subject template test.
        /// </summary>
        [TestMethod]
        public void SendConfirmationEmailCouldNotGetSubjectTemplateTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(ObjectMother.ValidProfileFredCompleted);

                    profileAdapter.Expect(x => x.GenerateEmailToken()).Repeat.Once().Return(ObjectMother.EmailToken);

                    profileAdapter.Expect(
                        x =>
                        x.SaveEmailToken(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId,
                            ObjectMother.EmailToken))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    templateAdapter.Expect(x => x.GetEmailBodyTemplate())
                                     .Repeat.Once()
                                     .Return(ObjectMother.ValidEmailBodyTemplate);

                    templateAdapter.Expect(x => x.GetEmailSubjectTemplate())
                                     .Repeat.Once()
                                     .Return(null);

                    var outcome =
                        orchestration.SendConfirmationEmail(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "ErrorRetrievingEmailSubjectTemplate"));
                });
        }

        /// <summary>
        /// Confirms the SMS confirm cell test.
        /// </summary>
        [TestMethod]
        public void ConfirmSmsConfirmCellTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.IsCellConfirmed(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(
                        x => x.ConfirmationPin(
                            ObjectMother.ValidProfileFredCompleted.CorrelationId,
                            ObjectMother.OneTimePin))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    profileAdapter.Expect(x => x.OtpExpired(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(new DataOperationOutcome() { IsSuccessful = false });

                    profileAdapter.Expect(x => x.ConfirmCell(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                        .Repeat.Once()
                        .Return(DataOperationOutcome.Success);

                    var outcome = orchestration.ConfirmCellularNumber(
                        ObjectMother.ValidProfileFredCompleted.CorrelationId,
                        ObjectMother.OneTimePin);

                    return outcome;
                }, 
                outcome =>
                {
                    Assert.IsTrue(outcome.IsSuccessful);
                });
        }

        /// <summary>
        /// Gets the previous loan application exists test.
        /// </summary>
        [TestMethod]
        public void GetPrevoiusLoanApplicationExistsTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetPreviousLoanApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.PartialApplicationAllFields);

                    var outcome = profileOrchestration.GetPreviousLoanApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId);
                    return outcome;
                },
            outcome =>
                {
                    Assert.IsNotNull(outcome);
                    Assert.IsTrue(outcome.IsSuccessful);
                });
        }

        /// <summary>
        /// Gets the application state profile not found test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void GetApplicationStateProfileNotFoundTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Throw(new InvalidOperationException());

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome => Assert.Fail("Code not reacheable"));
        }

        /// <summary>
        /// Gets the new application state get partial application state test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateGetNewPartialApplicationStateTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(new DataOperationOutcome { IsSuccessful = false });

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome => 
                {
                    Assert.IsTrue(outcome.IsSuccessful);
                    Assert.AreEqual(outcome.Data.State, "New");
                });
        }

        /// <summary>
        /// Gets the application state failed to get current loan test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateFailedToGetCurrentLoanTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(null);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "NoLoansFound"));
                });
        }

        /// <summary>
        /// Gets the application state get AHV status test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateGetAHVStatusTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyClosedLoan);

                    submitApplicationAdapter.Expect(x => x.IsAHVFailed(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(null);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Gets the application state document exist test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateDocumentExistTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyClosedLoan);

                    submitApplicationAdapter.Expect(x => x.IsAHVFailed(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(new DataOperationOutcome { IsSuccessful = false });

                    submitApplicationAdapter.Expect(x => x.IsDocumentationMissing(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(null);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Gets the application state get loan details test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateGetLoanDetailsTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(null);
                  
                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Gets the application state is quote issued test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateIsQuoteIssuedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyClosedLoan);

                    submitApplicationAdapter.Expect(x => x.IsAHVFailed(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(new DataOperationOutcome { IsSuccessful = false });

                    submitApplicationAdapter.Expect(x => x.IsDocumentationMissing(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsQuoteIssued(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsManualAffordabilityReview(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsTrue(outcome.IsSuccessful);
                });
        }

        /// <summary>
        /// Gets the application state is manual affordability review fail test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateIsManualAffordabilityReviewFailedTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyClosedLoan);

                    submitApplicationAdapter.Expect(x => x.IsAHVFailed(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(new DataOperationOutcome { IsSuccessful = false });

                    submitApplicationAdapter.Expect(x => x.IsDocumentationMissing(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsQuoteIssued(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsManualAffordabilityReview(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(null);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Gets the application state is manual affordability review test.
        /// </summary>
        [TestMethod]
        public void GetApplicationStateIsManualAffordabilityReviewTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(e => e.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ValidProfileFredCompleted);

                    submitApplicationAdapter.Expect(x => x.IsPartialApplicationSubmitted(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.GetLatestLoan(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyApprovedLoan);

                    selfServiceAdapter.Expect(x => x.GetDetailedLoan(ObjectMother.MyApprovedLoan.LoanReferenceNumber))
                    .Repeat.Once()
                    .Return(ObjectMother.MyClosedLoan);

                    submitApplicationAdapter.Expect(x => x.IsAHVFailed(ObjectMother.ValidProfileFredCompleted.IdentityNumber))
                    .Repeat.Once()
                    .Return(new DataOperationOutcome { IsSuccessful = false });

                    submitApplicationAdapter.Expect(x => x.IsDocumentationMissing(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsQuoteIssued(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    submitApplicationAdapter.Expect(x => x.IsManualAffordabilityReview(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    var outcome = profileOrchestration.GetApplicationState(ObjectMother.ValidProfileFredCompleted.CorrelationId);

                    return outcome;
                },
                outcome =>
                {
                    Assert.IsTrue(outcome.IsSuccessful);
                });
        }

        /// <summary>
        /// Update the existing user partial application profile does not exist test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void UpsertExistingUserPartialApplicationProfileDoesNotExistTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Throw(new InvalidOperationException());

                    var outcome = profileOrchestration.UpsertExistingUserPartailApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningInvalidPartialApplication);
                    return outcome;
                },
                outcome => Assert.Fail("Code not reacheable"));
        }

        /// <summary>
        /// Updates the existing user partial application test.
        /// </summary>
        [TestMethod]
        public void UpsertExistingUserPartailApplicationSuccessTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ProfileAllFields);

                    profileAdapter.Expect(x => x.UpsertExistingUserPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.Success);

                    var outcome = profileOrchestration.UpsertExistingUserPartailApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication);
                    return outcome;
                },
                outcome =>
                {
                    Assert.IsTrue(outcome.IsSuccessful);
                });
        }

        /// <summary>
        /// Updates the existing user partial application test.
        /// </summary>
        [TestMethod]
        public void UpsertExistingUserPartailApplicationUnexpectedErrorTest()
        {
            this.EvaluateProfileOrchestrationCase(
                (profileAdapter, profileOrchestration, smsAdapter, emailAdapter, communicationTemplateAdapter, submitApplicationAdapter, selfServiceAdapter) =>
                {
                    profileAdapter.Expect(x => x.GetProfile(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                    .Repeat.Once()
                    .Return(ObjectMother.ProfileAllFields);

                    profileAdapter.Expect(x => x.UpsertExistingUserPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication))
                    .Repeat.Once()
                    .Return(DataOperationOutcome.CreateError("fails"));

                    var outcome = profileOrchestration.UpsertExistingUserPartailApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication);
                    return outcome;
                },
                outcome =>
                {
                    Assert.IsFalse(outcome.IsSuccessful);
                    Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
                });
        }

        /// <summary>
        /// Evaluates the profile orchestration case.
        /// </summary>
        /// <typeparam name="T">The return type of the test case</typeparam>
        /// <param name="performTestCase">The logic to perform test case.</param>
        /// <param name="performAsserts">The logic to perform asserts.</param>
        private void EvaluateProfileOrchestrationCase<T>(Func<IProfileAdapter, IProfileOrchestrations, ISmsAdapter, IEmailAdapter, ICommunicationTemplateAdapter, ISubmitApplicationAdapter, ISelfServiceAdapter, T> performTestCase, Action<T> performAsserts)
        {
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var profileAdapter = MockRepository.GenerateMock<IProfileAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();

            var orchestration = new ProfileOrchestration(performanceCountersAdapter, profileAdapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            performanceCountersAdapter.Expect(e => e.LogRequest()).Repeat.Once();

            var profileOutcome = performTestCase(profileAdapter, orchestration, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            Console.WriteLine(JsonConvert.SerializeObject(profileOutcome, JsonWorker.JsonSettings));

            performAsserts(profileOutcome);
            profileAdapter.VerifyAllExpectations();
            performanceCountersAdapter.VerifyAllExpectations();
            smsAdapter.VerifyAllExpectations();
            emailAdapter.VerifyAllExpectations();
            submitApplicationAdapter.VerifyAllExpectations();
            selfServiceAdapter.VerifyAllExpectations();
        }
    }
}
