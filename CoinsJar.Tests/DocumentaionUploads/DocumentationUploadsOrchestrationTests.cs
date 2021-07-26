//-----------------------------------------------------------------------
// <copyright file="DocumentationUploadsOrchestrationTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.DocumentaionUploads
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhino.Mocks;
    using TestData;
    using WebApi.Adapters;
    using WebApi.Adapters.DocumentationUploads;
    using WebApi.DocumentationUploads;

    /// <summary>
    /// Document upload orchestration tests.
    /// </summary>
    [TestClass]
    public class DocumentationUploadsOrchestrationTests
    {
        /// <summary>
        /// Uploads the document could not read stream test.
        /// </summary>
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void UploadDocumentCouldNotReadStreamTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            submitApplicationAdapter.Expect(e => e.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once()
                .Return(ObjectMother.ResponseApplicationValidPreApproved);

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);
            adapter.Expect(e => e.GetFileStream(ObjectMother.GetFilePath))
                .Throw(new ArgumentException())
                .Repeat.Once();

            var outcome = orchestration.UploadDocument(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.FileName, ObjectMother.GetFilePath, null);
            Assert.Fail("Logically, can not reach this code");
        }

        /// <summary>
        /// Uploads the document could not get application identifier test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentCouldNotGetApplicationIdTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);
            submitApplicationAdapter.Expect(e => e.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once()
                .Return(null);

            var outcome = orchestration.UploadDocument(
                ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId, 
                ObjectMother.FileName, 
                ObjectMother.GetFilePath, 
                null);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.AreEqual("ApplicationNotFound", outcome.Errors.First().Code);
        }

        /// <summary>
        /// Uploads the documentation failed to save file.
        /// </summary>
        [TestMethod]
        public void UploadDocumentationFailedToSaveFileTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            var fileStream = ObjectMother.FileStream;

            submitApplicationAdapter.Expect(e => e.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once()
                .Return(ObjectMother.ResponseApplicationValidPreApproved);

            adapter.Expect(e => e.GetFileStream(ObjectMother.GetFilePath))
                .Repeat.Once()
                .Return(fileStream);

            adapter.Expect(e => e.UploadDocument(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId, null, fileStream, null))
                .Repeat.Once()
                .Return(new DataOperationOutcome { IsSuccessful = false });

            var outcome = orchestration.UploadDocument(ObjectMother.ValidProfileFredCompleted.CorrelationId, null, ObjectMother.GetFilePath, null);

            Assert.IsFalse(outcome.IsSuccessful);
        }

        /// <summary>
        /// Uploads the documentation test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentationTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            var fileStream = ObjectMother.FileStream;

            submitApplicationAdapter.Expect(e => e.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once()
                .Return(ObjectMother.ResponseApplicationValidPreApproved);

            adapter.Expect(e => e.GetFileStream(ObjectMother.GetFilePath))
                .Repeat.Once()
                .Return(fileStream);

            adapter.Expect(e => e.UploadDocument(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId, ObjectMother.FileName, fileStream, @"application/octet-stream"))
                .Repeat.Once()
                .Return(DataOperationOutcome.Success);

            var outcome = orchestration.UploadDocument(
                ObjectMother.ValidProfileFredCompleted.CorrelationId,
                ObjectMother.FileName,
                ObjectMother.GetFilePath,
                @"application/octet-stream");

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Gets the document not available test.
        /// </summary>
        [TestMethod]
        public void GetDocumentNotAvailableTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            performanceCountersAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            submitApplicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            adapter.Expect(x => x.GetDocumentsDetails(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once()
                .Return(ObjectMother.DocumentCollectionInvalid);

            var outcome = orchestration.GetDocumentsDetails(ObjectMother.ValidProfileFredCompleted.CorrelationId);
            performanceCountersAdapter.VerifyAllExpectations();

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "NoDocumentsFound"));
        }

        /// <summary>
        /// Gets the document available test.
        /// </summary>
        [TestMethod]
        public void GetDocumentAvailableTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            performanceCountersAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            submitApplicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            adapter.Expect(x => x.GetDocumentsDetails(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once()
                .Return(ObjectMother.DocumentCollectionValid);

            var outcome = orchestration.GetDocumentsDetails(ObjectMother.ValidProfileFredCompleted.CorrelationId);
            performanceCountersAdapter.VerifyAllExpectations();

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Gets the documents get application test.
        /// </summary>
        [TestMethod]
        public void GetDocumentsGetApplicationTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            performanceCountersAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            submitApplicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(null);

            var outcome = orchestration.GetDocumentsDetails(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "ApplicationNotFound"));
            Assert.IsFalse(outcome.IsSuccessful);
        }

        /// <summary>
        /// Deletes the document fail test.
        /// </summary>
        [TestMethod]
        public void DeleteDocumentFailTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            performanceCountersAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            adapter.Expect(x => x.DeleteDocument("123"))
                .Repeat.Once()
                .Return(DataOperationOutcome.CreateError("fails"));

            var outcome = orchestration.DeleteDocument("123");
            performanceCountersAdapter.VerifyAllExpectations();

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "NotEnlistedInTransaction"));
        }

        /// <summary>
        /// Deletes the document test.
        /// </summary>
        [TestMethod]
        public void DeleteDocumentTest()
        {
            var adapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var performanceCountersAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var submitApplicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var orchestration = new DocumentationUploadsOrchestration(performanceCountersAdapter, adapter, submitApplicationAdapter);

            performanceCountersAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            adapter.Expect(x => x.DeleteDocument("123"))
                .Repeat.Once()
                .Return(DataOperationOutcome.Success);

            var outcome = orchestration.DeleteDocument("123");
            performanceCountersAdapter.VerifyAllExpectations();

            Assert.IsTrue(outcome.IsSuccessful);
        }

        /// <summary>
        /// Uploads the documents to AZ get application fail test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentsToAZGetApplicationFailTest()
        {
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var documentsAdapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var applicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var documentsOrchestration = new DocumentationUploadsOrchestration(performanceCounterAdapter, documentsAdapter, applicationAdapter);

            performanceCounterAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            applicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(null);

            var outcome = documentsOrchestration.UploadDocumentToAZ(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "ApplicationNotFound"));
        }

        /// <summary>
        /// Uploads the documents to AZ get documents test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentsToAZGetDocumentsFailTest()
        {
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var documentsAdapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var applicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var documentsOrchestration = new DocumentationUploadsOrchestration(performanceCounterAdapter, documentsAdapter, applicationAdapter);

            performanceCounterAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            applicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            documentsAdapter.Expect(x => x.GetDocumentsDetails(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once().Return(null);

            var outcome = documentsOrchestration.UploadDocumentToAZ(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "NoDocumentsFound"));
        }

        /// <summary>
        /// Uploads the documents to AZ get image express.
        /// </summary>
        [TestMethod]
        public void UploadDocumentsToAZGetImageExpress()
        {
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var documentsAdapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var applicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var documentsOrchestration = new DocumentationUploadsOrchestration(performanceCounterAdapter, documentsAdapter, applicationAdapter);

            performanceCounterAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            applicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            documentsAdapter.Expect(x => x.GetDocumentsByApplicationId(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once().Return(ObjectMother.Documents);

            documentsAdapter.Expect(x => x.GetImageExpressDocuments(ObjectMother.Documents))
                .Repeat.Once().Return(null);

            var outcome = documentsOrchestration.UploadDocumentToAZ(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "CouldNotConvertImagesToImageExpress"));
        }

        /// <summary>
        /// Uploads the documents to AZ upload documents fail test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentsToAZUploadDocumentsFailTest()
        {
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var documentsAdapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var applicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var documentsOrchestration = new DocumentationUploadsOrchestration(performanceCounterAdapter, documentsAdapter, applicationAdapter);

            performanceCounterAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            applicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            documentsAdapter.Expect(x => x.GetDocumentsByApplicationId(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once().Return(ObjectMother.Documents);

            documentsAdapter.Expect(x => x.GetImageExpressDocuments(ObjectMother.Documents))
                .Repeat.Once().Return(ObjectMother.ImageExpressDocuments);

            documentsAdapter.Expect(x => x.UploadImagesToImageExpress(ObjectMother.ImageExpressDocuments))
                .Repeat.Once().Return(DataOperationOutcome.CreateError("fails"));           

            var outcome = documentsOrchestration.UploadDocumentToAZ(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsFalse(outcome.IsSuccessful);
            Assert.IsTrue(outcome.Errors.Any(x => x.Code == "UnexpectedInternalError"));
        }

        /// <summary>
        /// Uploads the documents to AZ upload documents success test.
        /// </summary>
        [TestMethod]
        public void UploadDocumentsToAZUploadDocumentsSuccessTest()
        {
            var performanceCounterAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var documentsAdapter = MockRepository.GenerateMock<IDocumentationUploadsAdapter>();
            var applicationAdapter = MockRepository.GenerateMock<ISubmitApplicationAdapter>();

            var documentsOrchestration = new DocumentationUploadsOrchestration(performanceCounterAdapter, documentsAdapter, applicationAdapter);

            performanceCounterAdapter.Expect(x => x.LogRequest()).Repeat.Once();

            applicationAdapter.Expect(x => x.GetApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId))
                .Repeat.Once().Return(ObjectMother.ResponseApplicationValidPreApproved);

            documentsAdapter.Expect(x => x.GetDocumentsByApplicationId(ObjectMother.ResponseApplicationValidPreApproved.Data.ApplicationId))
                .Repeat.Once().Return(ObjectMother.Documents);

            documentsAdapter.Expect(x => x.GetImageExpressDocuments(ObjectMother.Documents))
                .Repeat.Once().Return(ObjectMother.ImageExpressDocuments);

            documentsAdapter.Expect(x => x.UploadImagesToImageExpress(ObjectMother.ImageExpressDocuments))
                .Repeat.Once().Return(DataOperationOutcome.Success);

            var outcome = documentsOrchestration.UploadDocumentToAZ(ObjectMother.ValidProfileFredCompleted.CorrelationId);

            Assert.IsTrue(outcome.IsSuccessful);
        }
    }
}
