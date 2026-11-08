using System.IO.Compression;
using System.Numerics;
using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

public class ModelFileTests
{
    public static IEnumerable<object[]> Keys => DefenseRows.All.Select(kind => new object[] { kind.Key });

    private static readonly string[] States = { "State01", "State02", "State03", "StateBroken" };

    private static readonly IReadOnlyDictionary<string, Vector3> Movers = States.Concat(States.Select(name => name + "_Head")).ToDictionary(name => name, _ => Vector3.UnitY);

    private static byte[] Asset(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "assets", name));

    private static DefenseKind Kind(string key) => DefenseRows.All.Single(kind => kind.Key == key);

    private static Model Load(string key) => Glb.Read(Asset(Kind(key).ModelFile), Movers);

    // The game shows one state group at a time as the defense takes damage, so the model needs a look for each.
    [Theory]
    [MemberData(nameof(Keys))]
    public void TheModelHasALookForEveryStateTheGameShows(string key)
    {
        var parts = Load(key).Parts.Where(part => part.Meshes.Count > 0).Select(part => part.Name).ToList();

        Assert.All(States, state => Assert.Contains(state, parts));
    }

    // Each state is a separate look, but all of them must be about the same size, or the defense jumps as it breaks.
    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryStateIsAboutTheSizeOfTheWholeDefense(string key)
    {
        var model = Load(key);
        var intact = Bounds(model.Parts.Single(part => part.Name == "State01"));
        var width = intact.Max.X - intact.Min.X;

        Assert.InRange(width, 0.3f, 3f);
        Assert.InRange(intact.Max.Y - intact.Min.Y, 0.05f, 3f);

        foreach (var state in States.Skip(1))
        {
            var (min, max) = Bounds(model.Parts.Single(part => part.Name == state));

            Assert.InRange(max.X - min.X, width * 0.5f, width * 1.5f);
        }
    }

    // The turret's head turns toward the zombie it shoots, so each standing state needs it as its own part riding on
    // that state, turning about a pivot on top of the stand, and the broken one has none.
    [Fact]
    public void TheTurretHeadRidesOnEachStandingState()
    {
        var model = Load("Turret");

        foreach (var state in States.Take(3))
        {
            var head = model.Parts.Single(part => part.Name == state + "_Head");
            var stand = model.Parts.Single(part => part.Name == state);

            Assert.Equal(state, head.Parent);
            Assert.NotEmpty(head.Meshes);
            Assert.InRange(head.Pivot.Y - stand.Pivot.Y, 0.5f, 1f);
        }

        Assert.DoesNotContain(model.Parts, part => part.Name == "StateBroken_Head");
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryMeshIsWholeAndPointsAtARealMaterial(string key)
    {
        var model = Load(key);

        Assert.InRange(model.TriangleCount, 100, 60000);

        foreach (var mesh in model.Parts.SelectMany(part => part.Meshes))
        {
            Assert.InRange(mesh.Material, 0, model.Materials.Count - 1);
            Assert.Equal(mesh.Positions.Length, mesh.Normals.Length);
            Assert.Equal(mesh.Positions.Length, mesh.Uvs.Length);
            Assert.Equal(0, mesh.Triangles.Length % 3);
            Assert.All(mesh.Triangles, index => Assert.InRange(index, 0, mesh.Positions.Length - 1));
        }
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryTextureInTheModelDecodes(string key)
    {
        var model = Load(key);

        foreach (var material in model.Materials.Where(material => material.Image != null))
        {
            var picture = Png.Decode(model.Images[material.Image!.Value]);

            Assert.Equal(picture.Width * picture.Height * 4, picture.Rgba.Length);
            Assert.True(picture.Width >= 16 && picture.Height >= 16);
        }
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void TheIconIsTheSameSizeAsTheGamesOwn(string key)
    {
        var icon = Png.Decode(Asset(Kind(key).IconFile));

        Assert.Equal((512, 512), (icon.Width, icon.Height));
    }

    private static (Vector3 Min, Vector3 Max) Bounds(ModelPart part)
    {
        var points = part.Meshes.SelectMany(mesh => mesh.Positions).ToList();

        return (points.Aggregate(Vector3.Min), points.Aggregate(Vector3.Max));
    }

    // Unity's axes mirror glTF's left to right, so a point on glTF's +X lands on Unity's -X.
    [Fact]
    public void ModelSpaceIsMirroredIntoUnitysHandedness()
    {
        Assert.Equal(new Vector3(-1, 2, 3), Glb.ToUnity(new Vector3(1, 2, 3)));
    }

    [Fact]
    public void ANonGlbFileIsRefused()
    {
        Assert.Throws<FormatException>(() => Glb.Read(new byte[40], Movers));
    }
    // A 3x2 picture whose rows use filters 1 (left), 2 (up), so the decoder must undo each against the right neighbour.
    [Fact]
    public void FilteredRowsDecodeToTheOriginalPixels()
    {
        var expected = new byte[]
        {
            10, 20, 30, 255, 15, 25, 35, 255, 20, 30, 40, 128,
            11, 21, 31, 255, 16, 26, 36, 254, 25, 35, 45, 120,
        };
        var raw = new List<byte> { 1 };

        for (var x = 0; x < 12; x++) raw.Add((byte)(expected[x] - (x >= 4 ? expected[x - 4] : 0)));

        raw.Add(2);

        for (var x = 0; x < 12; x++) raw.Add((byte)(expected[12 + x] - expected[x]));

        var picture = Png.Decode(MakePng(3, 2, raw.ToArray()));

        Assert.Equal(expected, picture.Rgba);
    }

    [Fact]
    public void AnInterlacedPngIsRefused()
    {
        var png = MakePng(1, 1, new byte[] { 0, 1, 2, 3, 4 }, interlace: 1);

        Assert.Throws<FormatException>(() => Png.Decode(png));
    }

    private static byte[] MakePng(int width, int height, byte[] filteredRows, byte interlace = 0)
    {
        using var file = new MemoryStream();

        file.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        var header = new byte[13];

        BigEndian(header, 0, width);
        BigEndian(header, 4, height);
        header[8] = 8;
        header[9] = 6;
        header[12] = interlace;
        Chunk(file, "IHDR", header);

        using (var data = new MemoryStream())
        {
            using (var zlib = new ZLibStream(data, CompressionLevel.Optimal, leaveOpen: true)) zlib.Write(filteredRows);

            Chunk(file, "IDAT", data.ToArray());
        }

        Chunk(file, "IEND", Array.Empty<byte>());

        return file.ToArray();
    }

    // The decoder does not check chunk CRCs, so these are left zero.
    private static void Chunk(Stream file, string type, byte[] data)
    {
        var length = new byte[4];

        BigEndian(length, 0, data.Length);
        file.Write(length);
        file.Write(System.Text.Encoding.ASCII.GetBytes(type));
        file.Write(data);
        file.Write(new byte[4]);
    }

    private static void BigEndian(byte[] into, int at, int value)
    {
        into[at] = (byte)(value >> 24);
        into[at + 1] = (byte)(value >> 16);
        into[at + 2] = (byte)(value >> 8);
        into[at + 3] = (byte)value;
    }
}
