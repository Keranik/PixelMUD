using Rom24;
using Xunit;

namespace Rom24.Tests;

public class GmcpTests
{
    [Fact]
    public void Char_family_enables_children_until_a_longer_name()
    {
        var mods = new GmcpModules();
        Assert.True(mods.Allows("Char.Vitals"));
        Assert.True(mods.Allows("Room.Info"));
        Assert.True(mods.Allows("Comm.Channel"));
        Assert.True(mods.Allows("Core.Ping"));
        Assert.False(mods.Allows("Client.Media"));

        mods.Set(new[] { "Char 1", "Char.Skills 1", "Room 1" });
        Assert.True(mods.Allows("Char.Vitals"));
        Assert.True(mods.Allows("Char.Skills.Groups"));
        Assert.True(mods.Allows("Room.Info"));
        Assert.False(mods.Allows("Comm.Channel"));
    }

    [Fact]
    public void Hello_accepts_either_key_casing()
    {
        var d = new DescriptorData { Gmcp = true };
        var payload = System.Text.Encoding.ASCII.GetBytes(
            "Core.Hello {\"client\":\"Mudlet\",\"Version\":\"4.17\"}");
        Gmcp.OnFrame(d, payload);
        Assert.Equal("Mudlet", d.GmcpClient);
        Assert.Equal("4.17", d.GmcpVersion);
    }

    [Fact]
    public void Frame_doubles_iac_inside_the_payload()
    {
        var frame = Gmcp.Frame("Core.Ping", null);
        Assert.Equal(TelnetParser.IAC, frame[0]);
        Assert.Equal(TelnetParser.SB, frame[1]);
        Assert.Equal(TelnetParser.GMCP, frame[2]);
        Assert.Equal(TelnetParser.IAC, frame[^2]);
        Assert.Equal(TelnetParser.SE, frame[^1]);

        Assert.Equal(new byte[] { 65, 255, 255, 66 }, Gmcp.EscapeIac(new byte[] { 65, 255, 66 }));
    }

    [Fact]
    public void Closed_exit_is_omitted()
    {
        var dest = new RoomIndexData { vnum = 5, name = "North" };
        var closed = new RoomIndexData { vnum = 9, name = "East" };
        var here = new RoomIndexData { vnum = 1, name = "Here\n\r" };
        here.exit[0] = new ExitData { to_room = dest };
        here.exit[1] = new ExitData { to_room = closed, exit_info = (int)Merc.EX_CLOSED };
        var ch = new CharData { in_room = here, name = "Tester" };
        var exits = Gmcp.ExitMap(ch);
        Assert.Equal(5, exits["n"]);
        Assert.False(exits.ContainsKey("e"));
    }
}
