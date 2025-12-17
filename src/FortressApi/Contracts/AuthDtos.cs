namespace FortressApi.Contracts;

public sealed record RegisterReq(string Email, string Password);
public sealed record LoginReq(string Email, string Password);
public sealed record AuthRes(string Token);
