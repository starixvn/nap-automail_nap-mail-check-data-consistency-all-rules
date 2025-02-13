using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public static class DB
{
    public const string DBCONSISTENCY = "DBTP";
    public const string DBGLOB = "DBGLOB";
    public const string DBTP = "DBTP";

    public static List<string> DB05X06X = ["DB052", "DB053", "DB061", "DB062"];
}