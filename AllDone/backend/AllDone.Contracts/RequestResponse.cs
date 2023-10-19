namespace AllDone.Contracts;

public static class FacadeServiceContracts
{
    public record AddUserRequest(
        string Email,
        string FullName
    );
    
    public record AddUserResponse(
        bool Success,
        string? ErrorCode
    );
}
