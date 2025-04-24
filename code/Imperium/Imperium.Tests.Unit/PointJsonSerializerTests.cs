using Imperium.Common.Json;
using Imperium.Common.Points;
using System.Text.Json;

namespace Imperium.Tests.Unit;

[TestClass]
public class PointJsonSerializerTests
{
    [TestMethod]
    public void TestMissingPointType()
    {
        var point = new Point("dk", "k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        // Remove point type
        json = json.Replace(",\"PointType\":\"Integer\"", "", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyPointType()
    {
        var point = new Point("dk", "k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace(",\"PointType\":\"Integer\"", ",\"PointType\":\"\"", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestInvalidPointType()
    {
        var point = new Point("dk", "k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace(",\"PointType\":\"Integer\"", ",\"PointType\":\"NotAType\"", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestMissingKey()
    {
        var point = new Point("dk", "k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        // Remove key
        json = json.Replace("\"Key\":\"k\",", "", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidKeyMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyKey()
    {
        var point = new Point("dk", "abc", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace("\"Key\":\"abc\",", "", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidKeyMessage, ex.Message);
    }

    [TestMethod]
    public void TestWhiteSpaceKey()
    {
        var point = new Point("dk", "k", PointType.DoubleFloat) { DeviceKey = "bob" };
        var json = JsonSerializer.Serialize(point);

        json = json.Replace("\"Key\":\"k\",", "\"key\":\"  da-key  \",", StringComparison.OrdinalIgnoreCase);

        var deserialized = JsonSerializer.Deserialize<Point>(json);

        Assert.IsNotNull(deserialized);

        Assert.AreEqual("da-key", deserialized.Key);
        Assert.AreEqual("Key='da-key',Value='',PointType='DoubleFloat'", deserialized.ToString());
    }

    [TestMethod]
    public void TestMissingId()
    {
        var guid = Guid.NewGuid();
        var point = new Point("dk", "k", PointType.Integer) { Id = guid };
        var json = JsonSerializer.Serialize(point);

        // Remove key
        json = json.Replace($"\"Id\":\"{point.Id}\",", "", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidIdMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyId()
    {
        var guid = Guid.NewGuid();
        var point = new Point("dk", "abc", PointType.Integer) { Id = guid };
        var json = JsonSerializer.Serialize(point);

        json = json.Replace($"{point.Id}", "", StringComparison.OrdinalIgnoreCase);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidIdMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyGuidId()
    {
        var point = new Point("dk", "k", PointType.TimeOnly) { Id = Guid.Empty };
        var json = JsonSerializer.Serialize(point);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidIdMessage, ex.Message);
    }
}
