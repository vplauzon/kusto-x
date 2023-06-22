/**************************************************/
//  Centralize suffix generation

var suffix = uniqueString(resourceGroup().id, 'kusto-x')

output suffix string = suffix
