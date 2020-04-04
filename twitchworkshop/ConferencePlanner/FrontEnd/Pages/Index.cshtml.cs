using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using B = BackEnd.Protos;
using ConferenceDTO;
using FrontEnd.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		private readonly IConfiguration _Configuration;
		protected readonly IApiClient _apiClient;

		public IndexModel(IApiClient apiClient, ILogger<IndexModel> logger, IConfiguration configuration)
		{
			_apiClient = apiClient;
			_logger = logger;
			_Configuration = configuration;
		}

		[TempData]
		public string Message { get; set; }

		public bool ShowMessage => !string.IsNullOrEmpty(Message);

		public IEnumerable<IGrouping<DateTimeOffset?, SessionResponse>> Sessions { get; set; }

		public IEnumerable<(int Offset, DayOfWeek? DayofWeek)> DayOffsets { get; set; }

		public bool IsAdmin { get; set; }

		public int CurrentDayOffset { get; set; }

		public async Task OnGet(int day = 0)
		{

			IsAdmin = User.IsAdmin();

			CurrentDayOffset = day;

			var sessions = await _apiClient.GetSessionsAsync();

			var startDate = sessions.Min(s => s.StartTime?.Date);

			var offset = 0;
			DayOffsets = sessions.Select(s => s.StartTime?.Date)
													 .Distinct()
													 .OrderBy(d => d)   
													 .Select(day => (offset++, day?.DayOfWeek));

			var filterDate = startDate?.AddDays(day);

			Sessions = sessions.Where(s => s.StartTime?.Date == filterDate)
												 .OrderBy(s => s.TrackId)
												 .GroupBy(s => s.StartTime)
												 .OrderBy(g => g.Key);

		}

		public async Task GetUsingGrpc() {

			using var channel = GrpcChannel.ForAddress(_Configuration["serviceUrl"]);
			
			var client = new B.Sessions.SessionsClient(channel);
			var response = await client.GetSessionsAsync(new B.SessionRequest() { });

			foreach (var s in response.Sessions) {

			}

		}

	}
}
