using System.Text;
using SqlScribe.Enums;

namespace SqlScribe.Builders;

public partial class SqlQueryBuilder
{
    private readonly DatabaseVendor _databaseVendor;
    private readonly SqlNamingConvention _namingConvention;
    private readonly bool _pluralizeTableName;
    private bool _runDelete;
    private readonly Dictionary<string, object?> _parameters = new();
    private readonly Queue<string> _orderByQueue = new();
    private readonly Queue<string> _whereQueue = new();
    private readonly Queue<string> _havingQueue = new();
    private readonly Queue<string> _joinQueue = new();
    private readonly Queue<string> _selectQueue = new();
    private readonly Queue<string> _groupByQueue = new();
    private readonly Queue<string> _aggregateQueue = new();
    private int _paramCounter;
    private int? _page;
    private int? _limit;

    public SqlQueryBuilder(DatabaseVendor databaseVendor, SqlNamingConvention namingConvention, bool pluralizeTableName)
    {
        _namingConvention = namingConvention;
        _pluralizeTableName = pluralizeTableName;
        _databaseVendor = databaseVendor;
    }

    public SqlQueryBuilder Clone()
    {
        var clone = new SqlQueryBuilder(_databaseVendor, _namingConvention, _pluralizeTableName)
        {
            _runDelete = _runDelete,
            _paramCounter = _paramCounter,
            _page = _page,
            _limit = _limit
        };
        foreach (var kvp in _parameters)
        {
            clone._parameters[kvp.Key] = kvp.Value;
        }

        foreach (var item in _orderByQueue)
        {
            clone._orderByQueue.Enqueue(item);
        }

        foreach (var item in _whereQueue)
        {
            clone._whereQueue.Enqueue(item);
        }

        foreach (var item in _joinQueue)
        {
            clone._joinQueue.Enqueue(item);
        }

        foreach (var item in _selectQueue)
        {
            clone._selectQueue.Enqueue(item);
        }

        foreach (var item in _groupByQueue)
        {
            clone._groupByQueue.Enqueue(item);
        }

        foreach (var item in _aggregateQueue)
        {
            clone._aggregateQueue.Enqueue(item);
        }

        return clone;
    }

    public (string Sql, Dictionary<string, object?> Parameters) Build<TEntity>(bool excludeSemicolon = false)
    {
        var sql = new StringBuilder();
        var tableName = GetTableName(typeof(TEntity));

        if (_runDelete is not false)
        {
            sql.Append($"DELETE FROM {tableName} ");

            if (_whereQueue.Count != 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" ", _whereQueue));
            }
        }

        if (_runDelete is false)
        {
            sql.Append("SELECT ");

            if (_selectQueue.Count == 0)
            {
                throw new Exception("No select statement is added");
            }

            var upperBound = _selectQueue.Count + _aggregateQueue.Count;
            var counter = 0;

            foreach (var item in _selectQueue)
            {
                counter += 1;
                sql.Append(counter < upperBound ? $"{item}, " : item);
            }


            sql.Append($" FROM {tableName} ");

            foreach (var item in _aggregateQueue)
            {
                counter += 1;
                sql.Append(counter < upperBound ? $"{item}, " : item);
            }

            if (_joinQueue.Count > 0)
            {
                foreach (var item in _joinQueue)
                {
                    sql.Append(item);
                }
            }

            if (_whereQueue.Count != 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" ", _whereQueue));
            }

            upperBound = _groupByQueue.Count;
            counter = 0;
            if (upperBound > 0)
            {
                sql.Append(" GROUP BY ");
                foreach (var item in _groupByQueue)
                {
                    counter += 1;
                    sql.Append(counter < upperBound ? $"{item}, " : item);
                }
            }

            if (_groupByQueue.Count > 0 && _havingQueue.Count > 0)
            {
                sql.Append(" HAVING ");
                foreach (var item in _havingQueue)
                {
                    sql.Append($" {item} ");
                }
            }

            if (_orderByQueue.Count != 0)
            {
                sql.Append(" ORDER BY ").Append(string.Join(", ", _orderByQueue));
            }

            if (_page.HasValue && _limit.HasValue)
            {
                var limit = AddParameter(_limit);
                var page = AddParameter((_page - 1) * _limit);
                sql.Append(" LIMIT ").Append(limit).Append(" OFFSET ").Append(page);
            }

            if (_page.HasValue is false && _limit.HasValue)
            {
                var limit = AddParameter(_limit);
                sql.Append(" LIMIT ").Append(limit);
            }
        }
        
        if (excludeSemicolon is false)
        {
            if (char.IsWhiteSpace(sql[^1]))
            {
                sql[^1] = ';';
            }
            else
            {
                sql.Append(';');
            }
        }

        return (NormalizeWhiteSpace(sql).ToString().Trim(), _parameters);
    }
}