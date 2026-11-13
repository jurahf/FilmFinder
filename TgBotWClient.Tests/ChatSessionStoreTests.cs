using EsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TgBotWClient.Bot;
using Xunit;

namespace TgBotWClient.Tests;

public sealed class ChatSessionStoreTests
{
    private static EsDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<EsDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        return new EsDbContext(options);
    }

    private static TgChatSessionStore CreateStore(EsDbContext db)
    {
        return new TgChatSessionStore(db, NullLogger<TgChatSessionStore>.Instance);
    }

    [Fact]
    public async Task Set_then_Get_returns_binding()
    {
        var name = Guid.NewGuid().ToString("N");

        await using (var db = CreateDb(name))
        {
            await CreateStore(db).SetSessionIdAsync(100, "session-1");
        }

        await using (var db = CreateDb(name))
        {
            var sessionId = await CreateStore(db).GetSessionIdAsync(100);
            Assert.Equal("session-1", sessionId);
        }
    }

    [Fact]
    public async Task Set_overwrites_existing_binding_without_duplicates()
    {
        var name = Guid.NewGuid().ToString("N");

        await using (var db = CreateDb(name))
        {
            var store = CreateStore(db);
            await store.SetSessionIdAsync(100, "session-1");
            await store.SetSessionIdAsync(100, "session-2");
        }

        await using (var db = CreateDb(name))
        {
            var sessions = await db.FrontendChatSessions.ToListAsync();
            var entity = Assert.Single(sessions);
            Assert.Equal("session-2", entity.SessionId);
            Assert.Equal("telegram", entity.Frontend);
            Assert.Equal("100", entity.ChatId);
        }
    }

    [Fact]
    public async Task Clear_removes_binding()
    {
        var name = Guid.NewGuid().ToString("N");

        await using (var db = CreateDb(name))
        {
            var store = CreateStore(db);
            await store.SetSessionIdAsync(100, "session-1");
            await store.ClearAsync(100);
        }

        await using (var db = CreateDb(name))
        {
            Assert.Null(await CreateStore(db).GetSessionIdAsync(100));
            Assert.Empty(await db.FrontendChatSessions.ToListAsync());
        }
    }

    [Fact]
    public async Task Binding_of_another_chat_is_not_touched()
    {
        var name = Guid.NewGuid().ToString("N");

        await using (var db = CreateDb(name))
        {
            var store = CreateStore(db);
            await store.SetSessionIdAsync(100, "session-1");
            await store.SetSessionIdAsync(200, "session-2");
            await store.ClearAsync(100);
        }

        await using (var db = CreateDb(name))
        {
            var store = CreateStore(db);
            Assert.Null(await store.GetSessionIdAsync(100));
            Assert.Equal("session-2", await store.GetSessionIdAsync(200));
        }
    }
}
