namespace FortressApi.Contracts;

public sealed record NoteCreateReq(string Title, string Body);
public sealed record NoteUpdateReq(string Title, string Body);
public sealed record NoteRes(Guid Id, string Title, string Body, DateTimeOffset UpdatedAtUtc);
