using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Humanizer;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder<TEntity>
{
    private string AddParameter<TValue>(TValue value)
    {
        var paramName = $"@param{_paramCounter += 1}";
        _parameters[paramName] = value;
        return paramName;
    }

    private static string GetMappedOperator(SqlOperator sqlOperator)
    {
        return sqlOperator switch
        {
            SqlOperator.Equal => "=",
            SqlOperator.GreaterThan => ">",
            SqlOperator.LessThan => "<",
            SqlOperator.GreaterThanOrEqual => ">=",
            SqlOperator.LessThanOrEqual => "<=",
            SqlOperator.NotEqual => "<>",
            SqlOperator.Like => "LIKE",
            _ => throw new NotSupportedException($"Operator {sqlOperator} is not supported.")
        };
    }

    private string GetTableName(Type type)
    {
        var tableName = type.Name;

        if (_pluralizeTableName)
        {
            tableName = tableName.Pluralize();
        }

        return ConvertName(tableName, _namingConvention);
    }

    private static IEnumerable<string> ExtractPropertyNames(Expression expression)
    {
        switch (expression)
        {
            case LambdaExpression lambdaExpression:
                switch (lambdaExpression.Body)
                {
                    case MemberExpression member:
                        return new[] { member.Member.Name };
                    case UnaryExpression { Operand: MemberExpression unaryMember }:
                        return new[] { unaryMember.Member.Name };
                    case NewExpression newExpression:
                        return newExpression.Members?.Select(m => m.Name) ??
                               throw new Exception("No member found");
                }

                break;
        }

        throw new InvalidOperationException("Invalid expression format");
    }

    private static string ExtractPropertyName(Expression expression)
    {
        switch (expression)
        {
            case LambdaExpression lambdaExpression:
                switch (lambdaExpression.Body)
                {
                    case MemberExpression member:
                        return member.Member.Name;
                    case UnaryExpression { Operand: MemberExpression unaryMember }:
                        return unaryMember.Member.Name;
                    case NewExpression:
                        throw new Exception("Anonymous expression is not allowed");
                }

                break;
        }

        throw new InvalidOperationException("Invalid expression format");
    }

    private string DelimitString(string text)
    {
        return _databaseVendor switch
        {
            DatabaseVendor.PostgreSql => $"\"{text}\"",
            DatabaseVendor.SqLite => $"\"{text}\"",
            DatabaseVendor.MySql => $"`{text}`",
            DatabaseVendor.SqlServer => $"[{text}]",
            _ => text
        };
    }

    private static StringBuilder NormalizeWhiteSpace(StringBuilder input)
    {
        if (input.Length == 0)
        {
            return input;
        }

        var writeIndex = 0;
        var skipped = false;

        for (var readIndex = 0; readIndex < input.Length; readIndex++)
        {
            var c = input[readIndex];
            if (char.IsWhiteSpace(c))
            {
                if (skipped) continue;
                input[writeIndex++] = ' ';
                skipped = true;
            }
            else
            {
                skipped = false;
                input[writeIndex++] = c;
            }
        }

        input.Length = writeIndex;
        return input;
    }

    private static string ConvertName(string propertyName, SqlNamingConvention convention)
    {
        return convention switch
        {
            SqlNamingConvention.PascalCase => propertyName.Pascalize(),
            SqlNamingConvention.LowerSnakeCase => propertyName.Underscore().ToLowerInvariant(),
            SqlNamingConvention.UpperSnakeCase => propertyName.Underscore().ToUpperInvariant(),
            _ => throw new NotSupportedException($"Naming convention {convention} is not supported.")
        };
    }
}