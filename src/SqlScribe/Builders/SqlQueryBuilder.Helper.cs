using System.Linq.Expressions;
using System.Text;
using Humanizer;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
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

    private static string ExtractPropertyName<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new InvalidOperationException("Invalid expression format.")
        };
    }

    private static string ExtractPropertyName(LambdaExpression expression)
    {
        return expression.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new InvalidOperationException("Invalid expression format.")
        };
    }

    private string DelimitString(string text)
    {
        return _databaseVendor switch
        {
            DatabaseVendor.PostgreSql => $"\"{text}\"",
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

        // Trim the excess characters
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