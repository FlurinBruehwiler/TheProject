using LightningDB;
using TheProject;

var env = new LightningEnvironment("path.db");
env.Open();

using var transaction = new Transaction(env);

var folderTypeId = Guid.NewGuid();
var folderNameFieldId = Guid.NewGuid();

var folder1 = transaction.CreateObj(folderTypeId);
transaction.SetFldValue(folder1, folderNameFieldId, FldValue.FromInt32(420));

var folderNameValue = transaction.GetFldValue(folder1, folderNameFieldId).ToInt32();
Console.WriteLine(folderNameValue);


