using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class QueryTableNameParser
    {
        public struct TableNameInfo
        {
            public string DatabaseName;
            public string SchemaName;
            public string TableName;

            public override string ToString()
            {
                return SchemaName.IsNullOrBlank() ? "[" + TableName + "]" : (DatabaseName.IsNullOrBlank() ? "[" + SchemaName + "].[" + TableName + "]" : "[" + DatabaseName + "].[" + SchemaName + "].[" + TableName + "]");
            }
        }

        public static IEnumerable<TableNameInfo> GetTableNamesFromQuery(string query, bool includeStoredProcedureNames = true, bool includeTempTableNames = false)
        {
            var tableNameInfos = new List<TableNameInfo>();

            // Remove multi-line comments.
            // Must be done before single-line comments in case a single-line comment appears within a multi-line comment.
            // Otherwise the entire line will be removed when the ending of it may no longer be part of the comment.
            int indexOfCommentStart;
            int indexOfCommentEnd = -1;
            do
            {
                indexOfCommentStart = query.IndexOf("/*", StringComparison.OrdinalIgnoreCase);
                if (indexOfCommentStart != -1)
                {
                    indexOfCommentEnd = query.IndexOf("*/", StringComparison.OrdinalIgnoreCase);
                    if (indexOfCommentEnd != -1)
                    {
                        query = query.Substring(0, indexOfCommentStart) + query.Substring(indexOfCommentEnd + 2);
                    }
                }
            } while (indexOfCommentStart != -1 && indexOfCommentEnd != -1);

            // Remove single-line comments.
            query = query.Split('\n').Select(x =>
                {
                    int indexOfCommentType1 = x.IndexOf("//", StringComparison.OrdinalIgnoreCase);
                    int indexOfCommentType2 = x.IndexOf("--", StringComparison.OrdinalIgnoreCase);
                    if (indexOfCommentType1 != -1 || indexOfCommentType2 != -1)
                    {
                        int indexOfComment;
                        if (indexOfCommentType1 != -1 && indexOfCommentType2 != -1)
                        {
                            indexOfComment = Math.Min(indexOfCommentType1, indexOfCommentType2);
                        }
                        else if (indexOfCommentType1 != -1)
                        {
                            indexOfComment = indexOfCommentType1;
                        }
                        else
                        {
                            indexOfComment = indexOfCommentType2;
                        }
                        x = x.Substring(0, indexOfComment);
                    }
                    return x;
                }).Join("");

            // Replace all whitespace, including multiple spaces and new lines, with a single space.
            query = Regex.Replace(query, @"\s+", " ");

            var tempTables = new List<string>();
            if (!includeTempTableNames)
            {
                string tempTableQuery = query;
                int indexOfCreateTable;
                do
                {
                    indexOfCreateTable = tempTableQuery.IndexOf("create table", StringComparison.OrdinalIgnoreCase);
                    if (indexOfCreateTable != -1)
                    {
                        tempTableQuery = tempTableQuery.Substring(indexOfCreateTable + 13);
                        int indexOfSpace = tempTableQuery.IndexOf(' ');
                        int indexOfLeftParenthese = tempTableQuery.IndexOf('(');
                        tempTables.Add(tempTableQuery.Substring(0, indexOfLeftParenthese != -1 && indexOfLeftParenthese < indexOfSpace ? indexOfLeftParenthese : indexOfSpace).ToUpper());
                    }
                } while (indexOfCreateTable != -1);
            }

            if (includeStoredProcedureNames)
            {
                if (!AddStoredProcedureNames("execute procedure ", query, tableNameInfos)) // Advantage syntax.
                {
                    // SQL Server syntax.
                    AddStoredProcedureNames("execute ", query, tableNameInfos);
                    AddStoredProcedureNames("exec ", query, tableNameInfos);
                }
            }

            CoreLogic(query, tableNameInfos);

            return tableNameInfos.DistinctBy(x => x.TableName.ToUpper()).Where(x => !tempTables.Contains(x.TableName.ToUpper()));
        }

        private static bool AddStoredProcedureNames(string executeKeyword, string query, List<TableNameInfo> tableNameInfos)
        {
            bool anyNamesAdded = false;
            int indexOfExecuteKeyword;
            do
            {
                indexOfExecuteKeyword = query.IndexOf(executeKeyword, StringComparison.OrdinalIgnoreCase);
                if (indexOfExecuteKeyword != -1)
                {
                    query = query.Substring(indexOfExecuteKeyword + executeKeyword.Length);
                    int indexOfSpace = query.IndexOf(' ');
                    int indexOfLeftParenthese = query.IndexOf('(');
                    tableNameInfos.Add(GetTableNameInfo(query.Substring(0, indexOfLeftParenthese != -1 && indexOfLeftParenthese < indexOfSpace ? indexOfLeftParenthese : indexOfSpace)));
                    anyNamesAdded = true;
                }
            } while (indexOfExecuteKeyword != -1);
            return anyNamesAdded;
        }

        private static void CoreLogic(string query, List<TableNameInfo> tableNameInfos)
        {
            foreach (string unionQuery in Regex.Split(query, " union", RegexOptions.IgnoreCase).SelectMany(x => Regex.Split(x, "\\)union", RegexOptions.IgnoreCase)))
            {
                List<string> afterFroms = Regex.Split(unionQuery, " from ", RegexOptions.IgnoreCase).Skip(1).ToList();
                for (int i = 0; i < afterFroms.Count; i++)
                {
                    // Start getting table names after from.
                    string afterFrom = afterFroms[i];
                    // If this is a subquery...
                    if (afterFrom[0] == '(')
                    {
                        CoreLogic(afterFrom, tableNameInfos);
                    }
                    else
                    {
                        GetQueryAfterJoin(afterFrom, out string queryAfterJoin, out int indexOfJoin);

                        // Get table name after from.
                        // This will not be a subquery at this point.
                        int indexOfWhere = afterFrom.IndexOf(" where ", StringComparison.OrdinalIgnoreCase);
                        AddTableNamesAfterFrom(indexOfWhere == -1 ? (indexOfJoin == -1 ? afterFrom : afterFrom.Substring(0, indexOfJoin)) : afterFrom.Substring(0, indexOfWhere), tableNameInfos);

                        // Get table names after joins.
                        while (queryAfterJoin != null)
                        {
                            // If this is a subquery...
                            if (queryAfterJoin[0] == '(')
                            {
                                while (!afterFrom.Contains(')') && i < afterFroms.Count - 1)
                                {
                                    i++;
                                    afterFrom += " from " + afterFroms[i];
                                }
                                CoreLogic(afterFrom, tableNameInfos);
                                queryAfterJoin = null;
                            }
                            else
                            {
                                int indexOfSpace = queryAfterJoin.IndexOf(' ');
                                if (indexOfSpace != -1)
                                {
                                    tableNameInfos.Add(GetTableNameInfo(queryAfterJoin.Substring(0, indexOfSpace)));
                                    GetQueryAfterJoin(queryAfterJoin, out queryAfterJoin, out indexOfJoin);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void GetQueryAfterJoin(string query, out string queryAfterJoin, out int indexOfJoin)
        {
            queryAfterJoin = null;
            indexOfJoin = query.IndexOf(" join ", StringComparison.OrdinalIgnoreCase);
            if (indexOfJoin == -1)
            {
                indexOfJoin = query.IndexOf(" where ", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                queryAfterJoin = query.Substring(indexOfJoin + 6);
            }
        }

        private static void AddTableNamesAfterFrom(string input, List<TableNameInfo> tableNameInfos)
        {
            var tableNameInfo = new TableNameInfo();
            TableNameInfo previousTableNameInfo;
            do
            {
                previousTableNameInfo = tableNameInfo;
                tableNameInfo = GetTableNameInfo(GetTableNameAfterFrom(ref input));
                if (!tableNameInfo.Equals(previousTableNameInfo))
                {
                    tableNameInfos.Add(tableNameInfo);
                }
            } while (!tableNameInfo.Equals(previousTableNameInfo));
        }

        private static string GetTableNameAfterFrom(ref string input)
        {
            int indexOfSpace = input.IndexOf(' ');

            int indexOfComma = input.IndexOf(',');
            if (indexOfComma != -1)
            {
                string tableName = input.Substring(0, indexOfSpace == -1 ? indexOfComma : indexOfSpace);
                if (tableName[tableName.Length - 1] == ',')
                {
                    input = input.Substring(indexOfComma + 1).Trim();
                    tableName = tableName.Substring(0, tableName.Length - 1);
                }
                return tableName;
            }

            return indexOfSpace == -1 ? input : input.Substring(0, indexOfSpace);
        }

        private static TableNameInfo GetTableNameInfo(string tableName)
        {
            var tableNameInfo = new TableNameInfo();
            int indexOfDot = tableName.LastIndexOf('.');
            if (indexOfDot == -1)
            {
                tableNameInfo.DatabaseName = "";
                tableNameInfo.SchemaName = "";
            }
            else
            {
                string priorToTableName = tableName.Substring(0, indexOfDot);
                int indexOfDotBeforeSchema = priorToTableName.LastIndexOf('.');
                if (indexOfDotBeforeSchema == -1)
                {
                    tableNameInfo.DatabaseName = "";
                    tableNameInfo.SchemaName = priorToTableName.TrimStart('[').TrimEnd(']');
                }
                else
                {
                    tableNameInfo.DatabaseName = priorToTableName.Substring(0, indexOfDotBeforeSchema).TrimStart('[').TrimEnd(']');
                    tableNameInfo.SchemaName = priorToTableName.Substring(indexOfDotBeforeSchema + 1).TrimStart('[').TrimEnd(']');
                }
                tableName = tableName.Substring(indexOfDot + 1);
            }
            tableNameInfo.TableName = tableName.TrimStart('[').TrimEnd(']').TrimEnd(')');
            return tableNameInfo;
        }
    }
}
