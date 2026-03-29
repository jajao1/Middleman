
using FluentValidation;

namespace Middleman.FluentValidation
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {

        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) 
            { 
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var _validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = _validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0) 
            {
                throw new ValidationException(failures);
            }

            return await next();

        }
    }

    public class ValidationBehaviorNoResult<TRequest> : IPipelineBehavior<TRequest>
       where TRequest : IRequest
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviorNoResult(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task Handle(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
        {
            await Validate(request, cancellationToken);
            await next();
        }

        private async Task Validate(TRequest request, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return;

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0) throw new ValidationException(failures);
        }
    }
}
