//-----------------------------------------------------------------------
// <copyright file="FileUploadTests.cs" company="Boodle">
//     Boodle.
// </copyright>
//-----------------------------------------------------------------------

namespace CoinsJar.Tests.DocumentaionUploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Hosting;
    using System.Web.Http.Routing;

    using Adapters;
    using CoinsJar.Tests.TestData;
    using CoinsJar.WebApi.Areas.V1.Controllers;
    using CoinsJar.WebApi.Areas.V1.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Rhino.Mocks;
    using WebApi.Adapters;

    using WebApi.Adapters.DocumentationUploads;
    using WebApi.Adapters.Profiles;

    using WebApi.Adapters.SubmitApplications;
    using WebApi.Profiles;

    /// <summary>
    /// The file upload tests.
    /// </summary>
    [TestClass]
    public class FileUploadTests
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
        /// Uploading files test.
        /// </summary>
        [TestMethod]
        public void FileUploadingTest()
        {
            var boodleService = new BoodleServicesAdapter();
            var profileAdapter = new ProfileAdapter();
            var performanceAdapter = MockRepository.GenerateMock<IPerformanceCountersAdapter>();
            var emailAdapter = MockRepository.GenerateMock<IEmailAdapter>();
            var templateAdapter = MockRepository.GenerateMock<ICommunicationTemplateAdapter>();
            var smsAdapter = MockRepository.GenerateMock<ISmsAdapter>();
            var boodleServiceOrchestration = MockRepository.GenerateMock<IBoodleServicesOrchestration>();
            var selfServiceAdapter = MockRepository.GenerateMock<ISelfServiceAdapter>();
            var submitApplicationAdapter = new SubmitApplicationsAdapter(boodleService, profileAdapter, boodleServiceOrchestration);

            var profileOrchestration = new ProfileOrchestration(performanceAdapter, profileAdapter, smsAdapter, emailAdapter, templateAdapter, submitApplicationAdapter, selfServiceAdapter);
            var profile = EnlistProfile(profileOrchestration);
            var addApplicationOutcome = submitApplicationAdapter.AddApplication(ObjectMother.ValidProfileFredCompleted.CorrelationId, ObjectMother.MaleGender, ObjectMother.PartialApplicationAllFields);

            Assert.IsTrue(addApplicationOutcome.IsSuccessful);

            var controller = new ImagesController();
            SetupControllerWithFileForTests(controller);

            var result = controller.SubmitFiles(profile.CorrelationId);
            result.Wait();

            var jsonFileResults = result.Result as DocumentUploadResponse;

            Console.WriteLine(JsonConvert.SerializeObject(jsonFileResults, JsonWorker.Settings));

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonFileResults.FileUploadResults.Any(x => x.Name == ObjectMother.FileName));
        }

        /// <summary>
        /// Setups the controller for tests.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private static void SetupControllerWithFileForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, @"http://localhost:12267/api/v1/images");
            var content = new MultipartFormDataContent();
            var files = Directory.GetFiles(ObjectMother.CreateFiles);

            foreach (var file in files)
            {
                var fileStream = new FileStream(file, FileMode.Open);
                var fileName = Path.GetFileName(file);
                content.Add(new StreamContent(fileStream), "file", fileName);
            }

            request.Content = content;

            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{areas}/{controller}/{id}", new { id = RouteParameter.Optional });
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "images" } });

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.Request = request;
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
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
