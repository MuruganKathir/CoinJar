//-----------------------------------------------------------------------
// <copyright file="ProfileAdapterTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------
namespace CoinsJar.Tests.Profiles
{
    using System;
    using Adapters;
    using CoinsJar.Tests.TestData;
    using CoinsJar.WebApi;
    using CoinsJar.WebApi.Adapters;
    using CoinsJar.WebApi.Adapters.Profiles;
    using CoinsJar.WebApi.Areas.V1.Models;
    using CoinsJar.WebApi.Profiles;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Rhino.Mocks;
    using WebApi.Adapters.SubmitApplications;

    /// <summary>
    /// The Profile Adapter Tests.
    /// </summary>
    [TestClass]
    public class ProfileAdapterTests
    {
        /// <summary>
        /// Tests the initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            TestDataAdapter.ResetData();
        }

        /// <summary>
        /// Gets the profile does not exist test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void GetProfileDoesNotExistTest()
        {
            var adapter = new ProfileAdapter();

            adapter.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);
        }

        /// <summary>
        /// Enlists the profile test.
        /// </summary>
        [TestMethod]
        public void EnlistProfileTest()
        {
            var adapter = new ProfileAdapter();

            var profile = ObjectMother.ValidProfileFredNotCompleted;
            var outcome = adapter.EnlistNewProfile(
                profile.CorrelationId,
                profile.EmailAddress,
                profile.IdentityNumber);

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Gets the profile test.
        /// </summary>
        [TestMethod]
        public void GetProfileTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(orchestration);

            var result = adapter.GetProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.EmailAddress, profile.EmailAddress, "Email address not correct");
            Assert.AreEqual(result.IdentityNumber, profile.IdentityNumber, "Id number not correct");
            Assert.IsFalse(result.IsProfileCreationCompleted, "Unexpected profile state");
        }

        /// <summary>
        /// Generates the valid south african identifier number test.
        /// </summary>
        [TestMethod]
        public void GenerateValidSouthAfricanIdNumberTest()
        {
            var dateOfBirth = DateTime.Today.AddYears(-30);
            const bool IsMale = false;
            const bool IsCitizen = false;
            var validator = new IdentityNumberValidator();

            for (var i = 0; i < 1000; i++)
            {
                var idNumber = ObjectMother.GenerateIdNumber(dateOfBirth, IsMale, IsCitizen);
                Console.WriteLine(idNumber);
                var isValidId = validator.IsValid(idNumber);
                Assert.IsTrue(isValidId, "Fails validation");
            }
        }

        /// <summary>
        /// Deletes the profile test.
        /// </summary>
        [TestMethod]
        public void DeleteProfileTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            EnlistProfile(orchestration);

            var result = adapter.DeleteProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);

            var isFound = adapter.IsCorrelationIdInUse(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);
            Assert.IsFalse(isFound);
        }

        /// <summary>
        /// Deletes the profile invalid test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void DeleteProfileInvalidTest()
        {
            var adapter = new ProfileAdapter();

            adapter.DeleteProfile(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);
        }

        /// <summary>
        /// Tests the Inserts or updates to the profile after enlisting.
        /// </summary>
        [TestMethod]
        public void UpsertProfileAfterEnlistingTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(orchestration);

            var outcomeCellConfirmed = adapter.ConfirmCell(profile.CorrelationId);
            Assert.IsNotNull(outcomeCellConfirmed);
            Assert.IsTrue(outcomeCellConfirmed.IsSuccessful);

            var outcome = adapter.UpsertProfile(
                profile.CorrelationId,
                ObjectMother.ValidUpdateableFred);

            Assert.IsNotNull(outcome);
            Assert.IsTrue(outcome.IsSuccessful);

            var validatedProfile = orchestration.GetProfile(profile.CorrelationId);
            Assert.IsTrue(validatedProfile.Data.IsProfileCreationCompleted);
        }

        /// <summary>
        /// Determines whether [is correlation identifier in use no test].
        /// </summary>
        [TestMethod]
        public void IsCorrelationIdInUseNoTest()
        {
            var adapter = new ProfileAdapter();
            var outcome = adapter.IsCorrelationIdInUse(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Determines whether [is correlation identifier in use yes test].
        /// </summary>
        [TestMethod]
        public void IsCorrelationIdInUseYesTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            EnlistProfile(orchestration);

            var outcome = adapter.IsCorrelationIdInUse(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Determines whether [is identity number in use no test].
        /// </summary>
        [TestMethod]
        public void IsIdentityNumberInUseNoTest()
        {
            var adapter = new ProfileAdapter();
            var outcome = adapter.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Determines whether [is identity number in use yes test].
        /// </summary>
        [TestMethod]
        public void IsIdentityNumberInUseYesTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            EnlistProfile(orchestration);

            var outcome = adapter.IsIdentityNumberInUse(ObjectMother.ValidProfileFredNotCompleted.IdentityNumber);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Determines whether [is email address in use no test].
        /// </summary>
        [TestMethod]
        public void IsEmailAddressInUseNoTest()
        {
            var adapter = new ProfileAdapter();
            var outcome = adapter.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress);

            Assert.IsFalse(outcome);
        }

        /// <summary>
        /// Determines whether [is email address in use yes test].
        /// </summary>
        [TestMethod]
        public void IsEmailAddressInUseYesTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            EnlistProfile(orchestration);

            var outcome = adapter.IsEmailAddressInUse(ObjectMother.ValidProfileFredNotCompleted.EmailAddress);

            Assert.IsTrue(outcome);
        }

        /// <summary>
        /// Gets the partial application empty test.
        /// </summary>
        [TestMethod]
        public void GetPartialApplicationEmptyTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var application = adapter.GetPartialApplication(profile.CorrelationId);
            Assert.IsNull(application);
        }

        /// <summary>
        /// Gets the latest loan applications adapter empty test.
        /// </summary>
        [TestMethod]
        public void GetLatestApplicationAdapterEmptyTest()
        {
            var applicationServiceAdapter = MockRepository.GenerateMock<IBoodleServicesAdapter>();
            var profileAdapter = MockRepository.GenerateMock<IProfileAdapter>();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var recentPartialApplication = adapter.GetPreviousLoanApplication(profile.CorrelationId);
            Assert.IsNull(recentPartialApplication);
        }

        /// <summary>
        /// Gets the previous loan applications test.
        /// </summary>
        [TestMethod]

        public void GetLatestApplicationsAdapterTest()
        {
            var applicationServiceAdapter = MockRepository.GenerateMock<IBoodleServicesAdapter>();
            var profileAdapter = MockRepository.GenerateMock<IProfileAdapter>();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var submitApplicationAdapter = new SubmitApplicationsAdapter(applicationServiceAdapter, profileAdapter, boodleServiceOrchestration);

            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var outcome = orchestration.UpsertPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableSomeFields);

            Assert.IsTrue(outcome.IsSuccessful);

            var addApplication = submitApplicationAdapter.AddApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.MaleGender, ObjectMother.PartialApplicationAllFields);

            Assert.IsNotNull(addApplication);
            Assert.IsTrue(addApplication.IsSuccessful);

            var recentPartialApplication = adapter.GetPreviousLoanApplication(profile.CorrelationId);
            Assert.IsNotNull(recentPartialApplication);
        }

        /// <summary>
        /// Tests the inserts to the partial application.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationEmptyTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            var emptyPartialApplication = new PartialApplicationUpdateable();

            var outcome = adapter.UpsertPartialApplication(profile.CorrelationId, emptyPartialApplication);
            Assert.IsNotNull(outcome);
            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Tests the inserts and update to the partial application updatable.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationEmptyPartialUpdatableTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var outcome = adapter.UpsertPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableSomeFields);

            Assert.IsNotNull(outcome);
            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Updates the partial application update cell number test.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationUpdatCellNumberTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var outcome = adapter.UpdateCellFromPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableAllFields.CellNumber);

            Assert.IsNotNull(outcome);
            Assert.IsTrue(outcome.IsSuccessful);

            var updatedProfile = adapter.GetProfile(profile.CorrelationId);

            Assert.AreEqual(updatedProfile.CellNumber, ObjectMother.ApplicationUpdateableAllFields.CellNumber);
            Assert.IsFalse(updatedProfile.IsCellConfirmed.Value);
        }

        /// <summary>
        /// Updates the partial application name and surname test.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationUpdateNameAndSurnameTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var outcome = adapter.UpdateProfileDetails(
                profile.CorrelationId,
                ObjectMother.ApplicationUpdateableAllFields);

            Assert.IsNotNull(outcome);
            Assert.IsTrue(outcome.IsSuccessful);

            var updatedProfile = adapter.GetProfile(profile.CorrelationId);

            Assert.AreEqual(updatedProfile.Name, ObjectMother.ApplicationUpdateableAllFields.Name);
            Assert.AreEqual(updatedProfile.Surname, ObjectMother.ApplicationUpdateableAllFields.Surname);
        }

        /// <summary>
        /// Gets the partial application exists test.
        /// </summary>
        [TestMethod]
        public void GetPartialApplicationExistsTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var newApplicationOutcome = adapter.UpsertPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableAllFields);

            Assert.IsNotNull(newApplicationOutcome);

            var application = adapter.GetPartialApplication(profile.CorrelationId);

            Console.WriteLine(JsonConvert.SerializeObject(application, JsonWorker.Settings));

            Assert.IsNotNull(application);
        }

        /// <summary>
        /// Tests the updates to the partial application.
        /// </summary>
        [TestMethod]
        public void UpsertPartialApplicationExistsTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var newApplicationOutcome = adapter.UpsertPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableAllFields);

            Assert.IsNotNull(newApplicationOutcome);
            Assert.IsTrue(newApplicationOutcome.IsSuccessful);

            var updateOutcome = adapter.UpsertPartialApplication(
                profile.CorrelationId,
                ObjectMother.ApplicationUpdateableExisting);
           
            Assert.IsNotNull(updateOutcome);
            Assert.IsTrue(updateOutcome.IsSuccessful);
        }

        /// <summary>
        /// Saves the one time pin test.
        /// </summary>
        [TestMethod]
        public void SaveOneTimePinTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            
            var oneTimePin = adapter.GenerateOneTimePin();

            var adapterOutcome = adapter.SaveOneTimePin(
                profile.CorrelationId,
                oneTimePin);

            Assert.IsTrue(adapterOutcome.IsSuccessful);
        }

        /// <summary>
        /// Determines whether the cell has been confirmed.
        /// </summary>
        [TestMethod]
        public void IsCellConfirmedTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var outcome = adapter.IsCellConfirmed(profile.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful);
        }

        /// <summary>
        /// Determines whether email token has expired test.
        /// </summary>
        [TestMethod]
        public void IsEmailAddressConfirmed()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            var outcome = adapter.IsEmailAddressConfirmed(profile.CorrelationId);
            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Confirms the email confirm email token test.
        /// </summary>
        [TestMethod]
        public void ConfirmEmailConfirmEmailTokenTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var addEmailToken = adapter.SaveEmailToken(profile.CorrelationId, ObjectMother.EmailToken);
            Assert.IsTrue(addEmailToken.IsSuccessful, "Could not save the Email Token Code");

            var outcome = adapter.ConfirmEmailToken(profile.CorrelationId, ObjectMother.EmailToken);

            Assert.IsTrue(outcome.IsSuccessful, "Email Token Mismatch.");
        }

        /// <summary>
        /// Confirmations the pin test.
        /// </summary>
        [TestMethod]
        public void ConfirmationPinTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var addPinOutome = adapter.SaveOneTimePin(profile.CorrelationId, ObjectMother.OneTimePin);
            Assert.IsTrue(addPinOutome.IsSuccessful, "Could not save the pin");

            var outcome = adapter.ConfirmationPin(profile.CorrelationId, ObjectMother.OneTimePin);
            
            Assert.IsTrue(outcome.IsSuccessful, "Pin Mismatch.");
        }

        /// <summary>
        /// One time pin has expired test.
        /// </summary>
        [TestMethod]
        public void OneTimePinExpiredTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var addPinOutome = adapter.SaveOneTimePin(profile.CorrelationId, ObjectMother.OneTimePin);
            Assert.IsTrue(addPinOutome.IsSuccessful, "Could not save the pin");

            var outcome = adapter.OtpExpired(profile.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful, "OTP has expired.");
        }

        /// <summary>
        /// Confirms the cell test.
        /// </summary>
        [TestMethod]
        public void ConfirmCellTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var outcome = adapter.ConfirmCell(profile.CorrelationId);

            Assert.IsTrue(outcome.IsSuccessful, "Could not confirm cell.");
        }
        
        /// <summary>
        /// Confirms the email confirm email test.
        /// </summary>
        [TestMethod]
        public void ConfirmEmailConfirmEmailTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);

            var outcome = adapter.ConfirmEmail(profile.CorrelationId);

            Assert.IsTrue(outcome.IsSuccessful, "Could not confirm email.");
        }

        /// <summary>
        /// Updates the OTP request count passed.
        /// </summary>
        [TestMethod]
        public void UpdateOtpRequestCountPassed()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var outcome = adapter.UpdateOtpRequestCount(profile.CorrelationId);
            var updatedProfile = orchestration.GetProfile(profile.CorrelationId);

            Assert.IsNotNull(outcome);
            Assert.AreEqual(1, updatedProfile.Data.NumberOfOTPRequests);
        }

        /// <summary>
        /// Resets the OTP request count passed.
        /// </summary>
        [TestMethod]
        public void ResetOtpRequestCountPassed()
        {
            var adapter = new ProfileAdapter();
            var perfomanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(perfomanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var updateOTPOutcome = adapter.UpdateOtpRequestCount(profile.CorrelationId);
            var outcome = adapter.ResetNumberOfOTPRequests(profile.CorrelationId);
            var updatedProfile = orchestration.GetProfile(profile.CorrelationId);

            Assert.IsNotNull(outcome);
            Assert.AreEqual(0, updatedProfile.Data.NumberOfOTPRequests);
        }

        /// <summary>
        /// Generates the email token test.
        /// </summary>
        [TestMethod]
        public void GenerateEmailTokenTest()
        {
            var adapter = new ProfileAdapter();

            var outcome = adapter.GenerateEmailToken();

            Console.WriteLine(outcome);
            Assert.IsNotNull(outcome, "Could not generate email token.");
        }

        /// <summary>
        /// Saves the email token test.
        /// </summary>
        [TestMethod]
        public void SaveEmailTokenTest()
        {
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var adapter = new ProfileAdapter();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(orchestration);

            var emailToken = adapter.GenerateEmailToken();

            var saveEmailTokenOutcome = adapter.SaveEmailToken(profile.CorrelationId, emailToken);

            Assert.IsTrue(saveEmailTokenOutcome.IsSuccessful);
        }

        /// <summary>
        /// Verifies the valid existing OTP.
        /// </summary>
        [TestMethod]
        public void VerifyValidExistingOtpTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(orchestration);

            var result = adapter.VerifyValidExistingOtp(ObjectMother.ValidProfileFredCompleted.CorrelationId);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.IsSuccessful, false);
        }

        /// <summary>
        /// Verifies the in valid existing OTP test.
        /// </summary>
        [TestMethod]
        public void VerifyInValidExistingOtpTest()
        {
            var adapter = new ProfileAdapter();

            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var orchestration = new ProfileOrchestration(performanceCountersAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(orchestration);

            var result = adapter.VerifyValidExistingOtp(ObjectMother.ValidProfileFredNotCompleted.CorrelationId);
            Assert.IsFalse(result.IsSuccessful);
        }

        /// <summary>
        /// Gets the city identifier test.
        /// </summary>
        [TestMethod]
        public void GetCityIdTest()
        {
            var adapter = new ProfileAdapter();

            var outcome = adapter.ResolveCityId("Polokwane");

           Assert.IsNotNull(outcome);
        }

        /// <summary>
        /// Gets the suburb identifier test.
        /// </summary>
        [TestMethod]
        public void GetSuburbIdTest()
        {
            var adapter = new ProfileAdapter();

            var outcome = adapter.ResolveSuburbId("Morningside", ObjectMother.CityId);

            Assert.IsNotNull(outcome);
        }

        /// <summary>
        /// Update the existing profile throw exception test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void UpsertExistingProfileThrowExceptionTest()
        {
            var profileAdapter = new ProfileAdapter();

            var outcome = profileAdapter.UpsertExistingUserPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication);

            Assert.Fail("Code not reacheable");
        }

        /// <summary>
        /// Update the existing user profile test.
        /// </summary>
        [TestMethod]
        public void UpsertExistingUserProfileTest()
        {
            var adapter = new ProfileAdapter();
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();

            var orchestration = new ProfileOrchestration(performanceCounterAdapter, adapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(orchestration);
            UpsertProfile(orchestration, profile.CorrelationId);

            var partialApplicationOutcome = adapter.UpsertPartialApplication(profile.CorrelationId, ObjectMother.ApplicationUpdateableAllFields);

            Assert.IsTrue(partialApplicationOutcome.IsSuccessful);

            var outcome = adapter.UpsertExistingUserPartialApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.ReturningValidPartialApplication);

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Enlists the profile.
        /// </summary>
        /// <param name="orchestration">The orchestration.</param>
        /// <returns>The profile</returns>
        private static Profile EnlistProfile(ProfileOrchestration orchestration)
        {
            var profile = ObjectMother.ValidProfileFredNotCompleted;
            var outcome = orchestration.EnlistNewProfile(
                profile.CorrelationId,
                profile.EmailAddress,
                profile.IdentityNumber);

            Assert.IsTrue(outcome.IsSuccessful, "Could not enlist profile");
            return profile;
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="orchestration">The orchestration.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        private static void UpsertProfile(ProfileOrchestration orchestration, string correlationId)
        {
            var updateableProfile = ObjectMother.ValidUpdateableFred;
            var outcome = orchestration.UpsertProfile(
                correlationId,
                updateableProfile);

            Assert.IsTrue(outcome.IsSuccessful, "Could not update profile");
        }
    }
}
