using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cookbook.IntegrationTests;

public sealed class RecipesApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static object NewRecipe(string title, string fn = "WhiteRice", int minutes = 20, int servings = 2,
        string[]? tags = null) => new
    {
        title, description = "Descrizione", servings, riceCups = 1, cookingTimeMinutes = minutes,
        cookerFunction = fn, usesSteamer = false,
        ingredients = new[] { new { name = "Riso", quantity = "1 tazza" } },
        steps = new[] { "Lava il riso", "Avvia il programma" },
        tags = tags ?? ["veloce"]
    };

    private async Task<JsonElement> CreateAsync(object body)
    {
        var res = await _client.PostAsJsonAsync("/api/recipes", body);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Crud_RoundTrip()
    {
        var created = await CreateAsync(NewRecipe("Riso bianco semplice"));
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal("WhiteRice", created.GetProperty("cookerFunction").GetString());
        Assert.Equal(2, created.GetProperty("steps").GetArrayLength());

        var update = await _client.PutAsJsonAsync($"/api/recipes/{id}", NewRecipe("Riso integrale", "BrownRice", 45, tags: ["nuovo-tag"]));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var got = await _client.GetFromJsonAsync<JsonElement>($"/api/recipes/{id}");
        Assert.Equal("Riso integrale", got.GetProperty("title").GetString());
        Assert.Equal("BrownRice", got.GetProperty("cookerFunction").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/recipes/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/recipes/{id}")).StatusCode);
    }

    [Fact]
    public async Task Create_RejectsMoreThanFourServings()
    {
        var res = await _client.PostAsJsonAsync("/api/recipes", NewRecipe("Troppo", servings: 6));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task List_FiltersBySearchFunctionAndTag()
    {
        await CreateAsync(NewRecipe("Porridge di avena", "Porridge", 30, 2, ["colazione"]));
        await CreateAsync(NewRecipe("Verdure al vapore", "Steam", 15, 2, ["vapore"]));

        async Task<JsonElement> Query(string q) => await _client.GetFromJsonAsync<JsonElement>("/api/recipes" + q);

        Assert.Equal(1, (await Query("?search=avena")).GetArrayLength());
        Assert.Equal(1, (await Query("?cookerFunction=Steam&tag=vapore")).GetArrayLength());
        Assert.Equal(0, (await Query("?cookerFunction=Porridge&maxMinutes=10")).GetArrayLength());
    }

    [Fact]
    public async Task Media_UploadPhotoAndVideoUrl_ThenServeAndDelete()
    {
        var id = (await CreateAsync(NewRecipe("Con foto"))).GetProperty("id").GetGuid();

        var png = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47, 1, 2, 3]);
        png.Headers.ContentType = new("image/png");
        var form = new MultipartFormDataContent { { png, "file", "foto.png" } };
        var up = await _client.PostAsync($"/api/recipes/{id}/media", form);
        Assert.Equal(HttpStatusCode.Created, up.StatusCode);
        var media = await up.Content.ReadFromJsonAsync<JsonElement>();
        var url = media.GetProperty("url").GetString()!;
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync(url)).StatusCode);

        var vid = await _client.PostAsJsonAsync($"/api/recipes/{id}/media/video-url",
            new { url = "https://www.youtube.com/watch?v=abc", caption = "Tutorial" });
        Assert.Equal(HttpStatusCode.Created, vid.StatusCode);

        var summary = (await _client.GetFromJsonAsync<JsonElement>("/api/recipes"))
            .EnumerateArray().First(r => r.GetProperty("id").GetGuid() == id);
        Assert.Equal(url, summary.GetProperty("coverUrl").GetString());
        Assert.True(summary.GetProperty("hasVideo").GetBoolean());

        var mediaId = media.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/recipes/{id}/media/{mediaId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Media_RejectsUnsupportedFile()
    {
        var id = (await CreateAsync(NewRecipe("Con exe"))).GetProperty("id").GetGuid();
        var bin = new ByteArrayContent([1, 2, 3]);
        bin.Headers.ContentType = new("application/x-msdownload");
        var res = await _client.PostAsync($"/api/recipes/{id}/media", new MultipartFormDataContent { { bin, "file", "a.exe" } });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
