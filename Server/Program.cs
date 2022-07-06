using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Adding a recipe.
app.MapPost("recipes/add-recipe", async (Recipe recipe) =>
{
    List<Recipe> recipes = await ReadFile();
    if (recipes.Any())
    {
        recipes.Add(recipe);
        UpdateFile(recipes);
        return Results.Created("Successfully added a recipe", recipe);
    }
    return Results.BadRequest();
});

// Editing a recipe.
app.MapPut("recipes/edit-recipe/{id}", async (Guid id, string attributeName, string editedParameter) =>
{
    List<Recipe> recipes = await ReadFile();
    List<string> newValue = editedParameter.Split(",").ToList();

    if (attributeName == "Title")
    {
        recipes.Find(r => r.Id == id).Title = editedParameter;
    }
    else if (attributeName == "Instructions")
    {
        recipes.Find(r => r.Id == id).Instructions = newValue;
    }
    else if (attributeName == "Ingredients")
    {
        recipes.Find(r => r.Id == id).Ingredients = newValue;
    }
    else if (attributeName == "Categories")
    {
        recipes.Find(r => r.Id == id).Categories = newValue;
    }
    else
    {
        return Results.BadRequest();
    }
    UpdateFile(recipes);
    return Results.Ok(recipes.Find(r => r.Id == id));
});

// Listing a recipe.
app.MapGet("recipes/list-recipe/{title}", async (string title) =>
{
    List<Recipe> recipes = await ReadFile();
    List<Recipe> foundRecipes = recipes.FindAll(r => r.Title == title);
    if (!foundRecipes.Any())
        return Results.NotFound();
    else
        return Results.Ok(foundRecipes);
});

// Renaming a category.
app.MapPut("recipes/rename-category", async (string oldName, string newName) =>
{
    List<Recipe> recipes = await ReadFile();
    List<Recipe> beforeRename = recipes.FindAll(r => r.Categories.Contains(oldName));
    if (beforeRename.Any())
    {
        foreach (Recipe r in beforeRename)
        {
            r.Categories.Remove(oldName);
            r.Categories.Add(newName);
        }
        UpdateFile(recipes);
        return Results.Ok("Successfully updated");
    }
    return Results.BadRequest("This category does not exist.");
});

// Removing a category.
app.MapDelete("recipes/remove-category/{category}", async (string category) =>
{
    List<Recipe> recipes = await ReadFile();
    int removed = recipes.RemoveAll(r => r.Categories.Contains(category));
    if (removed > 0)
    {
        UpdateFile(recipes);
        return Results.Ok("Successfully deleted");
    }
    return Results.BadRequest("This category does not exist.");
});

app.Run();

// Reading the json file content.
static async Task<List<Recipe>> ReadFile()
{
    string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    string sFile = System.IO.Path.Combine(sCurrentDirectory, @"..\..\..\" + "Text.json");
    string sFilePath = Path.GetFullPath(sFile);
    string jsonString = await File.ReadAllTextAsync(sFilePath);
    List<Recipe>? menu = System.Text.Json.JsonSerializer.Deserialize<List<Recipe>>(jsonString);
    return menu;
}

// Updating the json file content.
static async void UpdateFile(List<Recipe> newRecipes)
{
    string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    string sFile = System.IO.Path.Combine(sCurrentDirectory, @"..\..\..\" + "Text.json");
    string sFilePath = Path.GetFullPath(sFile);
    var options = new JsonSerializerOptions { WriteIndented = true };
    File.WriteAllText(sFilePath, System.Text.Json.JsonSerializer.Serialize(newRecipes));
}



