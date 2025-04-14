using System.Text.Json;
using Imperium.Common.Json;
using Imperium.Common.Points;

namespace Imperium.Tests.Unit;

[TestClass]
public class PointJsonSerializerTests
{
    [TestMethod]
    public void TestMissingPointType()
    {
        var point = new Point("k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        // Remove point type
        json = json.Replace(",\"PointType\":\"Integer\"", "");

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyPointType()
    {
        var point = new Point("k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace(",\"PointType\":\"Integer\"", ",\"PointType\":\"\"");

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestInvalidPointType()
    {
        var point = new Point("k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace(",\"PointType\":\"Integer\"", ",\"PointType\":\"NotAType\"");

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidPointTypeMessage, ex.Message);
    }

    [TestMethod]
    public void TestMissingKey()
    {
        var point = new Point("k", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        // Remove key
        json = json.Replace("\"Key\":\"k\",", "");

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidKeyMessage, ex.Message);
    }

    [TestMethod]
    public void TestEmptyKey()
    {
        var point = new Point("", PointType.Integer);
        var json = JsonSerializer.Serialize(point);

        var ex = Assert.ThrowsExactly<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Point>(json);
        });

        Assert.AreEqual(PointJsonConverter.InvalidKeyMessage, ex.Message);
    }

    [TestMethod]
    public void TestWhiteSpaceKey()
    {
        var point = new Point("k", PointType.DoubleFloat);
        var json = JsonSerializer.Serialize(point);

        json = json.Replace("\"Key\":\"k\",", "\"Key\":\"  da-key  \",");

        var deserialized = JsonSerializer.Deserialize<Point>(json);

        Assert.IsNotNull(deserialized);

        Assert.AreEqual("da-key", deserialized.Key);
        Assert.AreEqual("Key='da-key',Value='',PointType='DoubleFloat'", deserialized.ToString());
    }
}
