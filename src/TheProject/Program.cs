using LightningDB;
using TheProject;

var env = new LightningEnvironment("path.db");
env.Open();

using var transaction = new Transaction(env);

var folderTypeId = Guid.NewGuid();
var folderNameFieldId = Guid.NewGuid();
var subFolderFieldId = Guid.NewGuid();
var parentFolderFieldId = Guid.NewGuid();

var folder1 = transaction.CreateObj(folderTypeId);
Console.WriteLine($"folder1: {folder1}");
transaction.SetFldValue(folder1, folderNameFieldId, FldValue.FromInt32(420));

var folder2 = transaction.CreateObj(folderTypeId);
Console.WriteLine($"folder2: {folder2}");

transaction.CreateAso(folder1, parentFolderFieldId, folder2, subFolderFieldId);

Console.WriteLine("Iterating subfolders of folder2");
foreach (var aso in transaction.EnumerateAso(folder2, subFolderFieldId))
{
    Console.WriteLine(aso.ObjId);
}

transaction.DeleteObj(folder2);

transaction.DebugPrintAllValues();


// var folderNameValue = transaction.GetFldValue(folder1, folderNameFieldId).ToInt32();
// Console.WriteLine(folderNameValue);
//
// transaction.SetFldValue(folder1, folderNameFieldId, FldValue.FromInt32(0));
// folderNameValue = transaction.GetFldValue(folder1, folderNameFieldId).ToInt32();
// Console.WriteLine(folderNameValue);

