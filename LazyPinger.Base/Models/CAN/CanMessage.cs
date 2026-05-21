namespace LazyPinger.Base.Models.CAN;

public class CanMessage
{
    public uint Id { get; set; }

    public byte[] Data { get; set; } = [];

    public byte DataLength { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public bool IsExtendedId { get; set; }

    public string IdHex => IsExtendedId
        ? $"0x{Id:X8}"
        : $"0x{Id:X3}";

    public string DataHex => Data.Length > 0
        ? string.Join(" ", Data.Select(b => b.ToString("X2")))
        : string.Empty;
}
