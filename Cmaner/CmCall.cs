﻿using System.Text;
using Cmaner.Holder;
using Cmaner.Menu;

namespace Cmaner;

/// <summary>
/// A class of methods representing commands for CM
/// </summary>
public static class CmCall
{
    /// <summary>
    /// Run a command and block console until it's done
    /// </summary>
    /// <param name="cmd">Command to run</param>
    public static async Task RunCmd(Command cmd)
    {
        var runner = new Runner(cmd);
        try
        {
            Console.WriteLine($"Running {cmd.CommandText}");
            CmConfig.CanBeInterrupted = false;
            await runner.RunAsync();
            CmConfig.CanBeInterrupted = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Run a command by displaying a menu of commands to the user and then executing the selected command
    /// </summary>
    public static async Task RunCommand()
    {
        if (CmStorage.Instance.Categories.Count == 0)
        {
            Console.WriteLine("No categories found, run [help] to see how to add one");
            return;
        }

        if (!CmStorage.Instance.Categories.SelectMany(x => x.Commands).Any())
        {
            Console.WriteLine("No commands found, run [help] to see how to add one");
            return;
        }

        var result = MenuRunner.RunMenu(new MenuCommand());
        if (result != null)
            await RunCmd(result);
    }

    #region Category Management

    /// <summary>
    /// Add a new category by prompting the user
    /// </summary>
    public static void AddCategory()
    {
        var cat = new Category();
        Console.Write("Enter category name: ");
        while (true)
        {
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty");
                continue;
            }

            cat.Name = name;
            break;
        }

        Console.Write("Enter description [optional]: ");
        var desc = Console.ReadLine();
        if (!string.IsNullOrEmpty(desc) && !string.IsNullOrWhiteSpace(desc))
            cat.Description = desc;

        var menuConf = new MenuConfirm("Are you sure?");
        var result = MenuRunner.RunMenu(menuConf);
        if (result)
        {
            Console.WriteLine($"Added {cat.Name}");
            CmStorage.Instance.Categories.Add(cat);
            CmStorage.Instance.Save();
        }
        else
        {
            Console.WriteLine("Aborted");
        }
    }

    /// <summary>
    /// Remove a category by displaying
    /// </summary>
    public static void RemoveCategory()
    {
        var catMenu = new MenuCategory();
        var cat = MenuRunner.RunMenu(catMenu);
        if (cat == null)
        {
            Console.WriteLine("Aborted");
            return;
        }

        var commandCount = cat.Commands.Count;
        var menuConf = new MenuConfirm($"Are you sure? it has {commandCount} commands");
        var result = MenuRunner.RunMenu(menuConf);
        if (result)
        {
            Console.WriteLine($"Removed {cat.Name}");
            CmStorage.Instance.Categories.Remove(cat);
            CmStorage.Instance.Save();
        }
        else
        {
            Console.WriteLine("Aborted");
        }
    }

    #endregion

    #region Command Management

    /// <summary>
    /// Add a new command to a category by displaying a menu of categories
    /// </summary>
    public static void AddCommand()
    {
        var catMenu = new MenuCategory();
        var cat = MenuRunner.RunMenu(catMenu);
        if (cat == null)
        {
            Console.WriteLine("Aborted");
            return;
        }

        Console.WriteLine($"Selected category {cat.Name}");
        var cmd = new Command();
        Console.Write("Enter command title [optional]: ");
        var title = Console.ReadLine();
        if (!string.IsNullOrEmpty(title) && !string.IsNullOrWhiteSpace(title))
            cmd.Title = title;
        Console.Write("Enter command text: ");
        while (true)
        {
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Command text cannot be empty");
                continue;
            }

            cmd.CommandText = text;
            break;
        }

        Console.Write("Enter working directory [optional]: ");
        var dir = Console.ReadLine();
        if (!string.IsNullOrEmpty(dir) && !string.IsNullOrWhiteSpace(dir))
            cmd.WorkingDirectory = dir;

        Console.Write("Enter description [optional]: ");
        var desc = Console.ReadLine();
        if (!string.IsNullOrEmpty(desc) && !string.IsNullOrWhiteSpace(desc))
            cmd.Description = desc;

        Console.Write("Enter short call [optional]: ");
        var shortCall = Console.ReadLine();
        if (!string.IsNullOrEmpty(shortCall) && !string.IsNullOrWhiteSpace(shortCall))
            cmd.ShortCall = shortCall;

        var admReq = new MenuConfirm("Admin required to run?");
        var result = MenuRunner.RunMenu(admReq);
        cmd.AdminRequired = result;
        Console.WriteLine($"Admin required: {(result ? "Yes" : "No")}");

        var menuConf = new MenuConfirm("Are you sure?");
        result = MenuRunner.RunMenu(menuConf);
        if (result)
        {
            Console.WriteLine($"Added {cmd.CommandText}");
            cat.Commands.Add(cmd);
            CmStorage.Instance.Save();
        }
        else
        {
            Console.WriteLine("Aborted");
        }
    }

    /// <summary>
    /// Remove a command from a category by displaying a menu of categories and then a menu of commands
    /// </summary>
    public static void RemoveCommand()
    {
        var catMenu = new MenuCommand();
        var cat = MenuRunner.RunMenu(catMenu);
        if (cat == null)
        {
            Console.WriteLine("Aborted");
            return;
        }

        var menuConf = new MenuConfirm("Are you sure?");
        var result = MenuRunner.RunMenu(menuConf);
        if (result)
        {
            var category = CmStorage.Instance.Categories.FirstOrDefault(x => x.Commands.Contains(cat));
            if (category == null)
            {
                Console.WriteLine("Could not find category");
                return;
            }

            category.Commands.Remove(cat);
            CmStorage.Instance.Save();
            Console.WriteLine($"Removed {cat.CommandText}");
        }
        else
        {
            Console.WriteLine("Aborted");
        }
    }

    #endregion

    /// <summary>
    /// Help list call
    /// </summary>
    public static void Help()
    {
        var strBuilder = new StringBuilder();
        strBuilder.AppendLine("Cmaner - command manager");
        strBuilder.AppendLine("Usage: ");
        strBuilder.AppendLine("cmaner - run menu");
        strBuilder.AppendLine("cmaner [add] [category] - add category");
        strBuilder.AppendLine("cmaner [add] [command] - add command");
        strBuilder.AppendLine("cmaner [rm] [category] - remove category");
        strBuilder.AppendLine("cmaner [rm] [command] - remove command");
        strBuilder.AppendLine("cmaner [help] - show this help");
        strBuilder.AppendLine("cmaner [short call] - run command");
        Console.WriteLine(strBuilder.ToString());
    }
}