using Faluf.BannerGen.Core.Enums;
using Faluf.BannerGen.Core.Helpers;
using Faluf.BannerGen.Core.Interfaces;
using Faluf.BannerGen.Infrastructure.Contexts;
using Faluf.BannerGen.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.SemanticKernel;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using System.Data;

namespace Faluf.BannerGen.Blazor.Helpers;

public static class ServiceCollectionHelper
{
	public static void AddCoreServices(this WebApplicationBuilder builder)
	{
		// Semantic Kernel
		builder.Services.AddTransient(sp =>
		{
			IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

			foreach (AIModel model in AIModelHelper.SupportedModels)
			{
				if (model.GetProvider() is AIProvider.OpenAI)
				{
					kernelBuilder.Services.AddOpenAIChatCompletion(modelId: model.ToModelIdentifier(), apiKey: builder.Configuration["OpenAI:APIKey"]!, orgId: builder.Configuration["OpenAI:OrgId"]!, serviceId: model.ToModelIdentifier());
				}
				else if (model.GetProvider() is AIProvider.Google)
				{
					kernelBuilder.Services.AddGoogleAIGeminiChatCompletion(modelId: model.ToModelIdentifier(), apiKey: builder.Configuration["Google:APIKey"]!, serviceId: model.ToModelIdentifier());
				}
			}

			return kernelBuilder.Build();
		});

		// FluentValidation
		builder.Services.AddValidatorsFromAssembly(typeof(BannerRequestValidator).Assembly, includeInternalTypes: true);

		// Serilog
		builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
		{
			loggerConfiguration.Enrich.WithProperty("Project", "BannerGen");

			loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
			loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning);

			loggerConfiguration.WriteTo.Console(
				outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", 
				theme: AnsiConsoleTheme.Code,
				restrictedToMinimumLevel: LogEventLevel.Information);

			MSSqlServerSinkOptions sinkOptions = new()
			{
				TableName = "Serilogs",
				AutoCreateSqlTable = true,
			};

			ColumnOptions columnOptions = new()
			{
				TimeStamp = 
				{ 
					ConvertToUtc = true, 
					DataType = SqlDbType.DateTime2 
				},
			};

			loggerConfiguration.WriteTo.Logger(lc => lc
				.Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Warning || (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContext) && sourceContext.ToString() == "\"ExplicitLog\""))
				.WriteTo.MSSqlServer(connectionString: builder.Configuration.GetConnectionString("BannerGenConnection"), sinkOptions: sinkOptions, columnOptions: columnOptions)
			);

			loggerConfiguration.Filter.ByExcluding(logEvent => logEvent.Exception is TaskCanceledException or OperationCanceledException);
		});

		// Localization
		builder.Services.AddLocalization();

		// IHttpClientFactory
		builder.Services.AddHttpClient("API", httpClient => httpClient.BaseAddress = new Uri($"{builder.Configuration["BaseUrl"]!}/api/"));
	}

	public static void AddDatabases(this WebApplicationBuilder builder)
	{
		builder.Services.AddDbContextFactory<AppDataContext>(options =>
		{
			options.UseSqlServer(builder.Configuration.GetConnectionString("BannerGenConnection"), sqlOptions =>
			{
				sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
				sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
			});

			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		});
	}

	public static void AddInfrastructure(this WebApplicationBuilder builder)
	{
		builder.Services.AddScoped<IBannerService, BannerService>();
		builder.Services.AddScoped<IStringLocalizer, EFCoreStringLocalizer>();
	}
}