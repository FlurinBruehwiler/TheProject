using LightningDB;
using TheProject;
using TheProject.Generated;

var env = new LightningEnvironment("path.db");
env.Open();

using var transaction = new Transaction(env);

var folder = new Folder(transaction);
folder.Name = "Hallo Johnny";

var parentFolder = new Folder(transaction);
folder.Parent = parentFolder; //todo what if the assoc is non nullable?

foreach (var subfolder in parentFolder.Subfolders)
{
    Console.WriteLine(subfolder.Name);
}

transaction.Commit();



