using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace SearchEngine.WebFramework.Swagger;

public static class SwaggerConfigurationExtension
{
    public static void AddSwagger(this IServiceCollection services)
    {
        //More info : https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters

        #region AddSwaggerExamples
        //Add services to use Example Filters in swagger
        //If you want to use the Request and Response example filters (and have called options.ExampleFilters() above), then you MUST also call
        //This method to register all ExamplesProvider classes form the assembly
        //services.AddSwaggerExamplesFromAssemblyOf<PersonRequestExample>();

        //We call this method for by reflection with the Startup type of entry assmebly (MyApi assembly)
        var mainAssembly = Assembly.GetEntryAssembly(); // => MyApi project assembly
        var mainType = mainAssembly.GetExportedTypes()[0];

        var methodName = nameof(Swashbuckle.AspNetCore.Filters.ServiceCollectionExtensions.AddSwaggerExamplesFromAssemblyOf);
        //MethodInfo method = typeof(Swashbuckle.AspNetCore.Filters.ServiceCollectionExtensions).GetMethod(methodName);
        MethodInfo method = typeof(Swashbuckle.AspNetCore.Filters.ServiceCollectionExtensions).GetRuntimeMethods().FirstOrDefault(x => x.Name == methodName && x.IsGenericMethod);
        MethodInfo generic = method.MakeGenericMethod(mainType);
        generic.Invoke(null, new[] { services });
        #endregion

        //Add services and configuration to use swagger
        services.AddSwaggerGen(options =>
        {
            var xmlDocPath = Path.Combine(AppContext.BaseDirectory, "Sahand.Payaneh.Api.xml");
            ////show controller XML comments like summary
            //   options.IncludeXmlComments(xmlDocPath, true);

            options.EnableAnnotations();

            #region DescribeAllEnumsAsStrings
            //This method was Deprecated. 
            //options.DescribeAllEnumsAsStrings();

            //You can specify an enum to convert to/from string, uisng :
            //[JsonConverter(typeof(StringEnumConverter))]
            //public virtual MyEnums MyEnum { get; set; }

            //Or can apply the StringEnumConverter to all enums globaly, using :
            //SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            //OR
            //JsonConvert.DefaultSettings = () =>
            //{
            //    var settings = new JsonSerializerSettings();
            //    settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            //    return settings;
            //};
            #endregion

            //options.DescribeAllParametersInCamelCase();
            //options.DescribeStringEnumsInCamelCase()
            //options.UseReferencedDefinitionsForEnums()
            //options.IgnoreObsoleteActions();
            //options.IgnoreObsoleteProperties();

            options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Client API" });
            options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "Web Service" });

            #region Filters
            //Enable to use [SwaggerRequestExample] & [SwaggerResponseExample]
            options.ExampleFilters();

            //It doesn't work anymore in recent versions because of replacing Swashbuckle.AspNetCore.Examples with Swashbuckle.AspNetCore.Filters
            //Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
            //options.OperationFilter<AddFileParamTypesOperationFilter>();

            #region Add Jwt Authentication
            //Add Lockout icon on top of swagger ui page to authenticate
            #region Old way
            // options.AddSecurityDefinition("Bearer", new ApiKeyScheme
            // {
            //     Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            //     Name = "Authorization",
            //     In = "header"
            // });

            //options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
            //{
            //    {"Bearer", new string[] { }}
            //});
            #endregion

            //options.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OAuth2" }
            //        },
            //        Array.Empty<string>() //new[] { "readAccess", "writeAccess" }
            //    }
            //});


            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                Type = SecuritySchemeType.OAuth2
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Name = "ApiKey",
                Description = "Api Key for using web servisec"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                    },
                    new string[] { }
                }
            });

            #endregion

            //If use FluentValidation then must be use this package to show validation in swagger (MicroElements.Swashbuckle.FluentValidation)
            //options.AddFluentValidationRules();
            #endregion
        });
    }

    public static void UseSwaggerAndUi(this IApplicationBuilder app)
    {
        //More info : https://github.com/domaindrivendev/Swashbuckle.AspNetCore

        //Swagger middleware for generate "Open API Documentation" in swagger.json
        app.UseSwagger(options =>
        {
            //options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        //Swagger middleware for generate UI from swagger.json
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Client API");

            #region Customizing
            //// Display
            //options.DefaultModelExpandDepth(2);
            //options.DefaultModelRendering(ModelRendering.Model);
            //options.DefaultModelsExpandDepth(-1);
            //options.DisplayOperationId();
            //options.DisplayRequestDuration();
            options.DocExpansion(DocExpansion.None);
            //options.EnableDeepLinking();
            //options.EnableFilter();
            //options.MaxDisplayedTags(5);
            //options.ShowExtensions();

            //// Network
            //options.EnableValidator();
            //options.SupportedSubmitMethods(SubmitMethod.Get);

            //// Other
            //options.DocumentTitle = "CustomUIConfig";
            //options.InjectStylesheet("/ext/custom-stylesheet.css");
            //options.InjectJavascript("/ext/custom-javascript.js");
            //options.RoutePrefix = "api-docs";
            #endregion
        });

        //ReDoc UI middleware. ReDoc UI is an alternative to swagger-ui
        app.UseReDoc(options =>
        {
            options.SpecUrl("/swagger/v1/swagger.json");

            #region Customizing
            //By default, the ReDoc UI will be exposed at "/api-docs"
            //options.RoutePrefix = "docs";
            //options.DocumentTitle = "My API Docs";

            options.EnableUntrustedSpec();
            options.ScrollYOffset(10);
            options.HideHostname();
            options.HideDownloadButton();
            options.ExpandResponses("200,201");
            options.RequiredPropsFirst();
            options.NoAutoAuth();
            options.PathInMiddlePanel();
            options.HideLoading();
            options.NativeScrollbars();
            options.DisableSearch();
            options.OnlyRequiredInSamples();
            options.SortPropsAlphabetically();
            #endregion
        });
    }
}