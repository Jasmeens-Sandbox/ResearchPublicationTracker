using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResearchPublicationTracker.Clients;
using ResearchPublicationTracker.Clients.Interfaces;
using ResearchPublicationTracker.Data;
using ResearchPublicationTracker.Options;
using ResearchPublicatoinsTracker.Options;
using System.Text.Json.Serialization;

namespace ResearchPublicationTracker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var config = builder.Configuration;
			builder.Services.AddMvc();
			builder.Services.AddDbContext<PublicationDbContext>(x => x.UseSqlite("Data Source=./publications.db"));
			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			});

			builder.Services.Configure<PubMedClientOptions>(builder.Configuration.GetSection("PubMedClient"));
			builder.Services.Configure<ScopusClientOptions>(builder.Configuration.GetSection("ScopusClient"));
			builder.Services.AddHttpClient<PubMedClient>();
			builder.Services.AddHttpClient<ScopusClient>();
			builder.Services.AddTransient<IPubmedPublicationProvider, PubMedClient>();
			builder.Services.AddTransient<IScopusPublicationProvider, ScopusClient>();

			var app = builder.Build();
			app.UseStaticFiles();
			app.MapDefaultControllerRoute();
			app.Run();
		}
	}
}
