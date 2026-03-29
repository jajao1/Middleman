namespace Middleman
{
    /// <summary>
    /// Represents the handling outcome for an exception in a request with response.
    /// </summary>
    /// <typeparam name="TResponse">Response type.</typeparam>
    public readonly struct ExceptionHandlerState<TResponse>
    {
        private ExceptionHandlerState(bool handled, TResponse response)
        {
            Handled = handled;
            Response = response;
        }

        public bool Handled { get; }

        public TResponse Response { get; }

        public static ExceptionHandlerState<TResponse> HandledWith(TResponse response)
            => new(true, response);

        public static ExceptionHandlerState<TResponse> NotHandled()
            => new(false, default!);
    }

    /// <summary>
    /// Represents the handling outcome for an exception in a request without response.
    /// </summary>
    public readonly struct ExceptionHandlerState
    {
        private ExceptionHandlerState(bool handled)
        {
            Handled = handled;
        }

        public bool Handled { get; }

        public static ExceptionHandlerState HandledResult()
            => new(true);

        public static ExceptionHandlerState NotHandled()
            => new(false);
    }
}
