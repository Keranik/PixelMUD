using Rom24;
using Xunit;

namespace Rom24.Tests;

public class TelnetParserTests
{
    static byte[] Bytes(params byte[] b) => b;

    static string Text(List<TelnetParser.Event> ev)
    {
        var s = "";
        foreach (var e in ev)
            if (e.Kind == TelnetParser.EventKind.Text && e.Data != null)
                s += System.Text.Encoding.Latin1.GetString(e.Data);
        return s;
    }

    [Fact]
    public void Plain_text_passes_through()
    {
        var p = new TelnetParser();
        var ev = p.Push(System.Text.Encoding.ASCII.GetBytes("hi\r\n"));
        Assert.Equal("hi\r\n", Text(ev));
        Assert.DoesNotContain(ev, e => e.Kind == TelnetParser.EventKind.Gmcp);
    }

    [Fact]
    public void Ga_is_two_bytes_and_does_not_eat_the_next_character()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes(TelnetParser.IAC, TelnetParser.GA, (byte)'A'));
        Assert.Equal("A", Text(ev));
        Assert.DoesNotContain(ev, e => e.Kind == TelnetParser.EventKind.Negotiate);
    }

    [Fact]
    public void Doubled_iac_in_text_is_one_ff()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes((byte)'x', TelnetParser.IAC, TelnetParser.IAC, (byte)'y'));
        Assert.Equal("x\u00ffy", Text(ev));
    }

    [Fact]
    public void Will_gmcp_is_negotiation_not_text()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes(TelnetParser.IAC, TelnetParser.WILL, TelnetParser.GMCP));
        Assert.Equal("", Text(ev));
        var n = Assert.Single(ev);
        Assert.Equal(TelnetParser.EventKind.Negotiate, n.Kind);
        Assert.Equal(TelnetParser.WILL, n.Command);
        Assert.Equal(TelnetParser.GMCP, n.Option);
    }

    [Fact]
    public void Do_and_dont_are_distinct()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes(
            TelnetParser.IAC, TelnetParser.DO, TelnetParser.GMCP,
            TelnetParser.IAC, TelnetParser.DONT, TelnetParser.GMCP));
        Assert.Equal(2, ev.Count);
        Assert.Equal(TelnetParser.DO, ev[0].Command);
        Assert.Equal(TelnetParser.DONT, ev[1].Command);
    }

    [Fact]
    public void Gmcp_frame_split_across_reads_reassembles()
    {
        var json = "{\"hp\":1}";
        var payload = System.Text.Encoding.ASCII.GetBytes("Char.Vitals " + json);
        var frame = new byte[3 + payload.Length + 2];
        frame[0] = TelnetParser.IAC;
        frame[1] = TelnetParser.SB;
        frame[2] = TelnetParser.GMCP;
        Buffer.BlockCopy(payload, 0, frame, 3, payload.Length);
        frame[^2] = TelnetParser.IAC;
        frame[^1] = TelnetParser.SE;

        var p = new TelnetParser();
        var first = p.Push(frame, 2);
        Assert.Empty(first);
        var mid = p.Push(frame.AsSpan(2, 6).ToArray());
        Assert.Empty(mid);
        var rest = p.Push(frame.AsSpan(8).ToArray());
        var g = Assert.Single(rest, e => e.Kind == TelnetParser.EventKind.Gmcp);
        Assert.Equal("Char.Vitals " + json, System.Text.Encoding.ASCII.GetString(g.Data));
    }

    [Fact]
    public void Doubled_iac_inside_gmcp_is_one_ff()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes(
            TelnetParser.IAC, TelnetParser.SB, TelnetParser.GMCP,
            (byte)'A', TelnetParser.IAC, TelnetParser.IAC, (byte)'B',
            TelnetParser.IAC, TelnetParser.SE));
        var g = Assert.Single(ev, e => e.Kind == TelnetParser.EventKind.Gmcp);
        Assert.Equal(new byte[] { (byte)'A', 255, (byte)'B' }, g.Data);
    }

    [Fact]
    public void Unfinished_subnegotiation_does_not_leak_into_text()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes(TelnetParser.IAC, TelnetParser.SB, TelnetParser.GMCP, (byte)'Z'));
        Assert.Equal("", Text(ev));
        var more = p.Push(Bytes(TelnetParser.IAC, TelnetParser.SE, (byte)'Q'));
        Assert.Equal("Q", Text(more));
        Assert.Contains(more, e => e.Kind == TelnetParser.EventKind.Gmcp);
    }

    [Fact]
    public void Oversized_subnegotiation_is_dropped_and_later_text_works()
    {
        var p = new TelnetParser();
        var head = Bytes(TelnetParser.IAC, TelnetParser.SB, TelnetParser.GMCP);
        p.Push(head);
        var bulk = new byte[TelnetParser.MaxSubnegotiation + 10];
        Array.Fill(bulk, (byte)'A');
        var mid = p.Push(bulk);
        Assert.DoesNotContain(mid, e => e.Kind == TelnetParser.EventKind.Gmcp);
        var tail = p.Push(System.Text.Encoding.ASCII.GetBytes("hi"));
        Assert.Contains("hi", Text(mid) + Text(tail));
    }

    [Fact]
    public void Partial_iac_waits_for_the_next_read()
    {
        var p = new TelnetParser();
        var ev = p.Push(Bytes((byte)'a', TelnetParser.IAC));
        Assert.Equal("a", Text(ev));
        var ev2 = p.Push(Bytes(TelnetParser.WILL, TelnetParser.GMCP, (byte)'b'));
        Assert.Contains(ev2, e => e.Kind == TelnetParser.EventKind.Negotiate && e.Command == TelnetParser.WILL);
        Assert.Contains("b", Text(ev2));
    }
}
