using GameServer.Services;
using NUnit.Framework;
using Shared.Messages;
using Shared.Models;

[TestFixture]
public class PlayerServiceTests
{
    [Test]
    public void TryLogin_AllowsFirstLogin_RejectsDuplicate()
    {
        var service = new PlayerService();
        Assert.That(service.TryLogin("player1"), Is.True); // Updated to use Assert.That with Is.True
        Assert.That(service.TryLogin("player1"), Is.False); // Updated to use Assert.That with Is.False
        service.Logout("player1");
        Assert.That(service.TryLogin("player1"), Is.True); // Updated to use Assert.That with Is.True
    }

    [Test]
    public void GetOrCreatePlayer_CreatesAndRetrievesPlayer()
    {
        var service = new PlayerService();
        var player = service.GetOrCreatePlayer("p1", "d1");
        Assert.That(player, Is.Not.Null); // Updated to use Assert.That with Is.Not.Null
        Assert.That(player.PlayerId, Is.EqualTo("P_p1")); // Updated to use Assert.That with Is.EqualTo
        Assert.That(player.DeviceId, Is.EqualTo("d1")); // Updated to use Assert.That with Is.EqualTo

        var samePlayer = service.GetOrCreatePlayer("p1", "d1");
        Assert.That(player, Is.EqualTo(samePlayer)); // Updated to use Assert.That with Is.EqualTo
    }

    [Test]
    public void UpdateResource_UpdatesAndReturnsNewBalance()
    {
        var service = new PlayerService();
        service.GetOrCreatePlayer("p2", "d2");
        int newBalance;
        Assert.That(service.UpdateResource("P_p2", ResourceType.Coins, 10, out newBalance), Is.True); // Updated to use Assert.That with Is.True
        Assert.That(newBalance, Is.EqualTo(10)); // Updated to use Assert.That with Is.EqualTo
        Assert.That(service.UpdateResource("P_p2", ResourceType.Coins, 5, out newBalance), Is.True); // Updated to use Assert.That with Is.True
        Assert.That(newBalance, Is.EqualTo(15)); // Updated to use Assert.That with Is.EqualTo
    }

    [Test]
    public void TransferResource_TransfersBetweenPlayers()
    {
        var service = new PlayerService();
        service.GetOrCreatePlayer("a", "da");
        service.GetOrCreatePlayer("b", "db");
        int _;
        service.UpdateResource("P_a", ResourceType.Rolls, 20, out _);

        Assert.That(service.TransferResource("P_a", "P_b", ResourceType.Rolls, 5), Is.True); // Updated to use Assert.That with Is.True
        PlayerState? a, b;
        Assert.That(service.TryGetPlayer("a", out a), Is.True); // Updated to use Assert.That with Is.True
        Assert.That(service.TryGetPlayer("b", out b), Is.True); // Updated to use Assert.That with Is.True
        Assert.That(a.Resources[ResourceType.Rolls], Is.EqualTo(15)); // Updated to use Assert.That with Is.EqualTo
        Assert.That(b.Resources[ResourceType.Rolls], Is.EqualTo(5)); // Updated to use Assert.That with Is.EqualTo
    }
}