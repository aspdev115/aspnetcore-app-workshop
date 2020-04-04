using BackEnd.Data;
using BackEnd.Protos;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Controllers
{
	public class GrpcSessionController : Sessions.SessionsBase
	{

		private readonly ApplicationDbContext _DbContext;

		public GrpcSessionController(ApplicationDbContext dbContext)
		{
			_DbContext = dbContext;
		}

		public override Task<SessionResponse> GetSessions(SessionRequest request, ServerCallContext context)
		{

			var outResponse = new SessionResponse();
			outResponse.Sessions.AddRange(_DbContext.Sessions.Select<Data.Session, BackEnd.Protos.Session>(s => new BackEnd.Protos.Session
			{
				Id = s.Id,
				Abstract = s.Abstract,
				EndTime = s.EndTime.ToString(),
				StartTime = s.StartTime.ToString(),
				Title = s.Title
			}));

			return Task.FromResult(outResponse);

		}
	}

}

