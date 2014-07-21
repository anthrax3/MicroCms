﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Lucene.Net.Store;
using Lucene.Net.Store.Azure;
using MicroCms.WebApi;
using Microsoft.WindowsAzure.Storage;

namespace MicroCms
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly, typeof (CmsDocumentsController).Assembly);
            _Container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_Container));
            
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureCms();
        }

        private static IContainer _Container;
        private const bool UseAzure = false;

        private void ConfigureCms()
        {
            var rootFolder = Server.MapPath("~/");
            var cmsDirectory = new DirectoryInfo(Path.Combine(rootFolder, @"App_Data\Cms"));

            if (cmsDirectory.Exists)
            {
                cmsDirectory.Delete(true);
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (UseAzure)
            {
                //Configure for Azure
                Cms.Configure(c =>
                {
                    var cacheDirectory = new RAMDirectory();
                    var azureDirectory = new AzureDirectory(CloudStorageAccount.Parse("UseDevelopmentStorage=true"), "cms-index", cacheDirectory);
                    c.RegisterBasicRenderServices()
                        .EnableMarkdownRenderService()
                        .EnableSourceCodeRenderService()
                        .UseLuceneSearch(azureDirectory);
                    c.UseAzureStorage("UseDevelopmentStorage=true");
                });
            }
            else
            {
                //Configure for local filesystem
                Cms.Configure(c =>
                {
                    c.RegisterBasicRenderServices()
                        .EnableMarkdownRenderService()
                        .EnableSourceCodeRenderService()
                        .UseLuceneSearch(new SimpleFSDirectory(new DirectoryInfo(Path.Combine(cmsDirectory.FullName, "Index"))));
                    c.UseFileSystemStorage(cmsDirectory);
                });
            }

            if (!Cms.GetArea().Documents.GetAll().Any())
            {
                var singleItemTemplate = new CmsTemplate("SingleTemplate", "<div class=\"row\">{0}</div>");
                var template = new CmsTemplate("PageTemplate", "<div class=\"row\">{0}{1}</div><div class=\"row\">{2}{3}{4}{5}</div><div class=\"row\">{6}</div>");
                Cms.GetArea().Templates.Save(template);
                Cms.GetArea().Templates.Save(singleItemTemplate);
                var document = new CmsDocument(template, "Example Rows",
                    new CmsItem(CreateMarkdown("#MD4", 4)),
                    new CmsItem(CreateMarkdown("#MD8", 8)),
                    new CmsItem(CreateMarkdown("#MD3", 3)),
                    new CmsItem(CreateMarkdown("#MD3", 3)),
                    new CmsItem(CreateMarkdown("#MD3", 3)),
                    new CmsItem(CreateMarkdown("#MD3", 3)),
                    new CmsItem(CreateMarkdown("#MD12", 12)));
                document.Tags.Add("documents");
                Cms.GetArea().Documents.Save(document);
                Cms.GetArea().Documents.Save(new CmsDocument(singleItemTemplate, "Source Code Example", new CmsItem(CreateMarkdown(@"#CODE
    {{CSharp}}
    public class Thing
    {
        public string Name { get; set; }
    }
", 12))));

                var readmeText = File.ReadAllText(Path.Combine(rootFolder, @"..\..\README.md"));

                var homeDocument = new CmsDocument(singleItemTemplate, "Readme", new CmsItem(CreateMarkdown(readmeText, 12)));
                homeDocument.Tags.Add("home");
                Cms.GetArea().Documents.Save(homeDocument);
            }
        }

        private CmsPart CreateMarkdown(string value, int columns)
        {
            return new CmsPart(CmsTypes.Markdown, value, new
            {
                @class = "col-md-" + columns
            });
        }
    }
}
