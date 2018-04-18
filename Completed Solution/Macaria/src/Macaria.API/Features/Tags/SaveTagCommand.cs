using MediatR;
using System.Threading.Tasks;
using System.Threading;
using FluentValidation;
using Macaria.Infrastructure.Data;
using Macaria.Core.Entities;

namespace Macaria.API.Features.Tags
{
    public class SaveTagCommand
    {
        public class Validator: AbstractValidator<Request> {
            public Validator()
            {
                RuleFor(request => request.Tag.TagId).NotNull();
            }
        }

        public class Request : IRequest<Response> {
            public TagApiModel Tag { get; set; }
        }

        public class Response
        {			
            public int TagId { get; set; }
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            public IMacariaContext _context { get; set; }
            public Handler(IMacariaContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var tag = await _context.Tags.FindAsync(request.Tag.TagId);

                if (tag == null) _context.Tags.Add(tag = new Tag());

                tag.Name = request.Tag.Name;

                await _context.SaveChangesAsync(cancellationToken);

                return new Response() { TagId = tag.TagId };
            }
        }
    }
}