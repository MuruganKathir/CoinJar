//-----------------------------------------------------------------------
// <copyright file="DocumentationUploadsAdapterTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------
namespace CoinsJar.Tests.DocumentaionUploads
{
    using System;
    using System.Linq;
    using Adapters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Rhino.Mocks;
    using TestData;
    using WebApi.Adapters;
    using WebApi.Adapters.DocumentationUploads;
    using WebApi.Adapters.Profiles;
    using WebApi.Adapters.SubmitApplications;
    using WebApi.DocumentationUploads;
    using WebApi.Profiles;

    /// <summary>
    /// The documentation adapter.
    /// </summary>
    [TestClass]
    public class DocumentationUploadsAdapterTests
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
        /// Documents the upload.
        /// </summary>
        [TestMethod]
        public void DocumentUpload()
        {
            var performanceAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var boodleService = MockRepository.GenerateMock<IBoodleServicesAdapter>();
            var profileAdapter = new ProfileAdapter();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();

            var adapter = new DocumentationUploadsAdapter();            
            var submitApplicationAdapter = new SubmitApplicationsAdapter(boodleService, profileAdapter, boodleServiceOrchestration);
            var orchestration = new DocumentationUploadsOrchestration(performanceAdapter, adapter, submitApplicationAdapter);

            var profileOrchestration = new ProfileOrchestration(performanceAdapter, profileAdapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(profileOrchestration);

            var addApplicationOutcome = submitApplicationAdapter.AddApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.MaleGender, ObjectMother.PartialApplicationAllFields);

            Assert.IsTrue(addApplicationOutcome.IsSuccessful);

            var application = submitApplicationAdapter.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsTrue(application.IsSuccessful);

            var outcome = orchestration.UploadDocument(
                ObjectMother.ValidProfileFredCompleted.CorrelationId, 
                ObjectMother.FileName, 
                ObjectMother.GetFilePath, 
                @"application/octet-stream");

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Gets the documents fail test.
        /// </summary>
        [TestMethod]
        public void GetDocumentsFailTest()
        {
            var performanceAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var adapter = new DocumentationUploadsAdapter();

            var outcome = adapter.GetDocumentsDetails(ObjectMother.ApplicationAllFieldsPreApproved.ApplicationId);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsNull(outcome.Documents);
        }

        /// <summary>
        /// Gets the documents test.
        /// </summary>
        [TestMethod]
        public void GetDocumentsTest()
        {
            var performanceAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var boodleService = MockRepository.GenerateMock<IBoodleServicesAdapter>();
            var profileAdapter = new ProfileAdapter();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();

            var submitApplicationAdapter = new SubmitApplicationsAdapter(boodleService, profileAdapter, boodleServiceOrchestration);
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();

            var adapter = new DocumentationUploadsAdapter();
            var orchestration = new DocumentationUploadsOrchestration(performanceAdapter, adapter, submitApplicationAdapter);
            var profileOrchestration = new ProfileOrchestration(performanceAdapter, profileAdapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(profileOrchestration);

            var addApplicationOutcome = submitApplicationAdapter.AddApplication(
                profile.CorrelationId,
                ObjectMother.MaleGender,
                ObjectMother.PartialApplicationAllFields);

            Assert.IsTrue(addApplicationOutcome.IsSuccessful);

            var application = submitApplicationAdapter.GetApplication(profile.CorrelationId);

            Assert.IsNotNull(application.Data);

            var documentUploadOutcome = orchestration.UploadDocument(
                       profile.CorrelationId,
                       ObjectMother.FileName,
                       ObjectMother.GetFilePath,
                       @"application/octet-stream");

            Assert.IsTrue(documentUploadOutcome.IsSuccessful);

            var receiveDocumentOutcome = adapter.GetDocumentsDetails(application.Data.ApplicationId);

            Console.WriteLine(JsonConvert.SerializeObject(receiveDocumentOutcome, JsonWorker.JsonSettings));

            Assert.IsTrue(receiveDocumentOutcome.IsSuccessful);
            Assert.IsNotNull(receiveDocumentOutcome.Documents);
        }

        /// <summary>
        /// Deletes the documents test.
        /// </summary>
        [TestMethod]
        public void DeleteDocumentsTest()
        {
            var performanceAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var boodleService = MockRepository.GenerateMock<IBoodleServicesAdapter>();
            var profileAdapter = new ProfileAdapter();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();

            var submitApplicationAdapter = new SubmitApplicationsAdapter(boodleService, profileAdapter, boodleServiceOrchestration);
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();

            var adapter = new DocumentationUploadsAdapter();
            var orchestration = new DocumentationUploadsOrchestration(performanceAdapter, adapter, submitApplicationAdapter);
            var profileOrchestration = new ProfileOrchestration(performanceAdapter, profileAdapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);

            var profile = EnlistProfile(profileOrchestration);

            var addApplicationOutcome = submitApplicationAdapter.AddApplication(
                profile.CorrelationId,
                ObjectMother.MaleGender,
                ObjectMother.PartialApplicationAllFields);

            Assert.IsTrue(addApplicationOutcome.IsSuccessful);

            var application = submitApplicationAdapter.GetApplication(profile.CorrelationId);

            Assert.IsNotNull(application.Data);

            var documentUploadOutcome = orchestration.UploadDocument(
                       profile.CorrelationId,
                       ObjectMother.FileName,
                       ObjectMother.GetFilePath,
                       @"application/octet-stream");

            Assert.IsTrue(documentUploadOutcome.IsSuccessful);

            var receiveDocumentOutcome = adapter.GetDocumentsDetails(application.Data.ApplicationId);

            Assert.IsTrue(receiveDocumentOutcome.IsSuccessful);
            Assert.IsNotNull(receiveDocumentOutcome.Documents);

            var document = receiveDocumentOutcome.Documents.Single();

            Console.WriteLine(JsonConvert.SerializeObject(document, JsonWorker.JsonSettings));

            var outcome = adapter.DeleteDocument(document.DocumentId.ToString());

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

            return profile;
        }
    }
}
