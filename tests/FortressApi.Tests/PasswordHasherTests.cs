using FortressApi.Services;
using Xunit;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_Works()
    {
        var h = new PasswordHasher();
        var (hash, salt) = h.Hash("SuperSecurePassword!!");
        Assert.True(h.Verify("SuperSecurePassword!!", hash, salt));
        Assert.False(h.Verify("wrong", hash, salt));
    }
}
