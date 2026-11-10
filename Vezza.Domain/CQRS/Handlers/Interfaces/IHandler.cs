using Vezza.Domain.CQRS.Commands.Interfaces;
using Vezza.Domain.CQRS.Results;

namespace Vezza.Domain.CQRS.Handlers.Interfaces;
public interface IHandler<T> where T : ICommand
{
    Task<GenericResult> Handle(T command);
}