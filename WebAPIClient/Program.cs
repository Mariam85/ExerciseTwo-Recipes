using System;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using Spectre.Console;
using System.Text.Json;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Net.Http.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    static async Task Main(string[] args)
    {    
        // Looping till the user chooses to exit.
        while (true)
        {
            // The following lines will be changed so that the url is not hard coded. 
            client.BaseAddress = new Uri("https://localhost:7165/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // The title using FigletText.
            AnsiConsole.Write(
                        new FigletText("Recipes app")
                            .Centered()
                            .Color(Color.Aqua));

            // A prompt to pick the functionality.
            List<string> functionality = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
            .PageSize(10)
            .Title("[purple]Please pick the functionality you want to do[/]")
            .InstructionsText("[grey]([blue][/] use up and down arrows to toggle, press space then enter [green][/]to accept)[/]")
            .AddChoices(
                new[]{
                        "Add recipe","Edit recipe","List recipe","Exit","Remove category","Rename category"
                }));
            var userChoice = functionality.Count == 1 ? functionality[0] : null;

            // Adding a new recipe.
            if (userChoice == "Add recipe")
            {
                List<string> recipeIngredients = AnsiConsole.Ask<string>("1)Ingredients: [grey]seperate them by adding a - [/] ").Split("-").ToList();
                string recipeTitle = AnsiConsole.Ask<string>("2)Title: ");
                List<string> recipeInstructions = AnsiConsole.Ask<string>("3)Instructions: [grey]seperate them by adding a - [/] ").Split("-").ToList();
                List<string> recipeCategories = AnsiConsole.Ask<string>("4)Categories: [grey]seperate them by adding a - [/] ").Split("-").ToList();
                var recipe = new Recipe(recipeIngredients, recipeTitle, recipeInstructions, recipeCategories);

                HttpResponseMessage response = await client.PostAsJsonAsync("recipes/add-recipe", recipe);
                if (!CheckSuccess(response))
                {
                    break;
                }
                AnsiConsole.Clear();
            }
            // Editing a recipe.
            else if (userChoice == "Edit recipe")
            {
                List<Recipe> recipesList = await client.GetFromJsonAsync<List<Recipe>>("recipes");

                var table = new Table().Border(TableBorder.Ascii2);
                table.Expand();
                table.AddColumn("[dodgerblue2]Title[/]");
                table.AddColumn(new TableColumn("[dodgerblue2]Ingredients[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Instructions[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Categories[/]").LeftAligned());

                for (int i = 0; i < recipesList.Count; i++)
                {
                    table.AddRow(
                         String.Join("\n", recipesList[i].Title),
                         String.Join("\n", recipesList[i].Ingredients.Select(x => $"- {x}")),
                         String.Join("\n", recipesList[i].Instructions.Select((x, n) => $"- {x}")),
                         String.Join("\n", recipesList[i].Categories.Select((x) => $"- {x}")));
                    table.AddEmptyRow();
                }
                AnsiConsole.Write(table);

                int index = AnsiConsole.Ask<int>("Enter the number of the recipe to edit:");
                Recipe recipeToEdit = recipesList[index - 1];
                var attributeToEdit = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                    .PageSize(10)
                    .Title("[purple]Please pick what you want to edit:[/]")
                    .InstructionsText("[grey]( use up and down arrows to toggle, press space then press enter to accept)[/]")
                    .AddChoices(
                        new[]
                        {
                                "Categories", "Instructions", "Ingredients", "Title"
                        }));

                var choiceEdit = attributeToEdit.Count == 1 ? attributeToEdit[0] : null;
                bool isSuccess = false;

                // Todo use the edit endpoint here.
                if (isSuccess)
                {
                    AnsiConsole.WriteLine("Successfully edited the recipe!");
                }
                else
                {
                    AnsiConsole.WriteLine("Failed to edit the recipe");
                }
                bool mainMenu = AnsiConsole.Confirm("Do you want to return to main menu?");
                if (!mainMenu)
                {
                    break;
                }
                AnsiConsole.Clear();
            }
            // Listing a recipe.
            else if (userChoice == "List recipe")
            {
                string listTitle = AnsiConsole.Ask<string>("Enter the title of the recipe to list:");
                List<Recipe> foundRecipe = await client.GetFromJsonAsync<List<Recipe>>($"recipes/list-recipe/{listTitle}");
                // Recipe attributes displayed in a table.
                var table = new Table().Border(TableBorder.Ascii2);
                table.Expand();
                table.AddColumn("[dodgerblue2]Title[/]");
                table.AddColumn(new TableColumn("[dodgerblue2]Ingredients[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Instructions[/]").LeftAligned());
                table.AddColumn(new TableColumn("[dodgerblue2]Categories[/]").LeftAligned());
                for (int i = 0; i < foundRecipe.Count; i++)
                {
                    table.AddRow(
                         String.Join("\n", foundRecipe[i].Title),
                         String.Join("\n", foundRecipe[i].Ingredients.Select(x => $"- {x}")),
                         String.Join("\n", foundRecipe[i].Instructions.Select((x, n) => $"- {x}")),
                         String.Join("\n", foundRecipe[i].Categories.Select((x) => $"- {x}")));
                    table.AddEmptyRow();
                }
                AnsiConsole.Write(table);
                AnsiConsole.Write("\n");
                bool mainMenu = AnsiConsole.Confirm("Do you want to return to main menu?");
                if (!mainMenu)
                {
                    break;
                }
                AnsiConsole.Clear();
            }
            // Removing a category.
            else if (userChoice == "Remove category")
            {
                string category = AnsiConsole.Ask<string>("Enter the name of the category to remove:");
                HttpResponseMessage response = await client.DeleteAsync($"recipes/remove-category/{category}");
                if (!CheckSuccess(response))
                {
                    break;
                }
                AnsiConsole.Clear();
            }
            // Renaming a category.
            else if (userChoice == "Rename category")
            {
                string category = AnsiConsole.Ask<string>("Enter the name of the category to edit:");
                string newCategory = AnsiConsole.Ask<string>("Enter the category's new name:");
                HttpResponseMessage response = await client.PutAsync($"recipes/rename-category?oldName={category}&newName={newCategory}", null);
                if (!CheckSuccess(response))
                {
                    break;
                }
                AnsiConsole.Clear();
            }
        }
    }
    static bool CheckSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            AnsiConsole.WriteLine("Successfully renamed category!");
        }
        else
        {
            AnsiConsole.WriteLine("Failed to rename category");
        }
        bool mainMenu = AnsiConsole.Confirm("Do you want to return to main menu?");
        return mainMenu;
    }
}



