namespace Rom24
{
    /// <summary>
    /// Byte-at-a-time telnet splitter. Text, option negotiation, and GMCP
    /// subnegotiations come out in order. A sequence split across reads stays
    /// in the parser until it is complete.
    /// </summary>
    public sealed class TelnetParser
    {
        public const byte IAC = 255;
        public const byte SE = 240;
        public const byte NOP = 241;
        public const byte GA = 249;
        public const byte SB = 250;
        public const byte WILL = 251;
        public const byte WONT = 252;
        public const byte DO = 253;
        public const byte DONT = 254;
        public const byte EOR = 239;
        public const byte GMCP = 201;
        public const int MaxSubnegotiation = 64 * 1024;

        public enum EventKind { Text, Negotiate, Gmcp }

        public readonly struct Event
        {
            public EventKind Kind { get; init; }
            public byte[] Data { get; init; }
            public byte Command { get; init; }
            public byte Option { get; init; }
        }

        enum State { Data, Iac, Opt, Sub, SubIac }

        State _state = State.Data;
        byte _cmd;
        byte _opt;
        readonly List<byte> _sub = new();
        readonly List<byte> _text = new();

        public List<Event> Push(byte[] data, int count)
        {
            var events = new List<Event>();
            if (data == null || count <= 0)
                return events;
            if (count > data.Length)
                count = data.Length;
            for (int i = 0; i < count; i++)
                Feed(data[i], events);
            FlushText(events);
            return events;
        }

        public List<Event> Push(byte[] data) => Push(data, data?.Length ?? 0);

        void Feed(byte b, List<Event> events)
        {
            switch (_state)
            {
                case State.Data:
                    if (b == IAC)
                    {
                        FlushText(events);
                        _state = State.Iac;
                    }
                    else
                    {
                        _text.Add(b);
                    }
                    break;

                case State.Iac:
                    if (b == IAC)
                    {
                        _text.Add(IAC);
                        _state = State.Data;
                    }
                    else if (b == WILL || b == WONT || b == DO || b == DONT)
                    {
                        _cmd = b;
                        _state = State.Opt;
                    }
                    else if (b == SB)
                    {
                        _sub.Clear();
                        _opt = 0;
                        _state = State.Sub;
                    }
                    else
                    {
                        /* GA, EOR, NOP, SE, and every other single-byte command. */
                        _state = State.Data;
                    }
                    break;

                case State.Opt:
                    events.Add(new Event { Kind = EventKind.Negotiate, Command = _cmd, Option = b });
                    _state = State.Data;
                    break;

                case State.Sub:
                    if (b == IAC)
                    {
                        _state = State.SubIac;
                        break;
                    }
                    AppendSub(b, events);
                    break;

                case State.SubIac:
                    if (b == SE)
                    {
                        FinishSub(events);
                        _state = State.Data;
                    }
                    else if (b == IAC)
                    {
                        AppendSub(IAC, events);
                        _state = State.Sub;
                    }
                    else
                    {
                        /* Broken subnegotiation. Drop it and resync. */
                        _sub.Clear();
                        _state = State.Data;
                    }
                    break;
            }
        }

        void AppendSub(byte b, List<Event> events)
        {
            if (_sub.Count == 0)
                _opt = b;
            if (_sub.Count >= MaxSubnegotiation)
            {
                _sub.Clear();
                _state = State.Data;
                /* The overflowing byte is discarded. Later bytes are data again
                   so a missing SE cannot eat the rest of the connection. */
                return;
            }
            _sub.Add(b);
        }

        void FinishSub(List<Event> events)
        {
            if (_sub.Count > 0 && _sub[0] == GMCP)
            {
                var payload = new byte[_sub.Count - 1];
                if (payload.Length > 0)
                    _sub.CopyTo(1, payload, 0, payload.Length);
                events.Add(new Event { Kind = EventKind.Gmcp, Data = payload });
            }
            _sub.Clear();
        }

        void FlushText(List<Event> events)
        {
            if (_text.Count == 0)
                return;
            events.Add(new Event { Kind = EventKind.Text, Data = _text.ToArray() });
            _text.Clear();
        }
    }
}
