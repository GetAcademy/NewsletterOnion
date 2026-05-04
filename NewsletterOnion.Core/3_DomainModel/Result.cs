namespace NewsletterOnion.Core._3_DomainModel
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        public Result(bool isSuccess, string message)
        {
            Message = message;
            IsSuccess = isSuccess;
        }

        public static Result Ok(string message) => new(true, message);
        public static Result Fail(string message) => new(false, message);
    }
}
