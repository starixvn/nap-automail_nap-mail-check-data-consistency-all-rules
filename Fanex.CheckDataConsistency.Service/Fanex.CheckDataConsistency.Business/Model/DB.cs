using System.Collections.Generic;

namespace Fanex.CheckDataConsistency.Business.Model;

public static class DB
{
    public const string DBCONSISTENCY = "DBTP";
    public const string DBGLOB = "DBGLOB";
    public const string DBTP = "DBTP";
    public const string DBCUST = "DBCUST";
    public const string DB052 = "DB052";
    public const string DB053 = "DB053";
    public const string DB054 = "DB054";
    public const string DB055 = "DB055";
    public const string DB061 = "DB061";
    public const string DB062 = "DB062";
    public const string DB05 = "DB05";
    public const string DB05BK1 = "DB05BK1";
    public const string DBGRP01L = "DBGRP01L";
    public const string DBGRP02L = "DBGRP02L";
    public const string DBGRP03L = "DBGRP03L";
    public const string DBTRANS = "DBTRANS";
    public const string DBDW = "DBDW";
    public const string DBDW2 = "DBDW2";
    public const string DB04ADMIN1 = "DB04ADMIN1";
    public const string DB04ADMIN2 = "DB04ADMIN2";
    public const string DBPRO = "DBPRO";

    public static List<string> DB05X06X { get; } = ["DB052", "DB053", "DB061", "DB062"];

    public static List<string> DB_05X_06X_CTG { get; } = ["DB052", "DB053", "DB061", "DB062", "DBCTG"];

    public static List<string> MainDB { get; } = ["DBCUST"];

    public static List<string> RepDB { get; } = ["DBTP", "DB061", "DB062"];

    public static List<string> DB05X { get; } = ["DB05", "DB05BK1"];
}