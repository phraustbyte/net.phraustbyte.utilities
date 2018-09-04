using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace SQLStoredProcedureGenerator
{
    public class Generator
    {
        public static string ObjectToCreate(Type obj)
        {
            List<string> PropertyList = new List<string>();
            List<string> VariableList = new List<string>();
            var propertyList = obj.GetProperties();
            var varList = new List<string>();
            var TableNameAttribute = obj.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var TableName = TableNameAttribute.TableName ?? obj.Name;
            string Identifier = "";
            foreach (var prop in propertyList)
            {
                if (prop.CustomAttributes.Any(x => x.AttributeType == typeof(UniqueIdentiferAttribute)))
                {
                    Identifier = $"{prop.Name}";
                }
                else
                {
                    if (!(prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute))))
                    {
                        VariableList.Add($"@{prop.Name} {Enum.GetName(typeof(SqlDbType), SqlHelper.GetDbType(prop.PropertyType))}");
                        varList.Add($"@{prop.Name}");
                        PropertyList.Add(prop.Name);
                    }
                }
            }
            if (String.IsNullOrEmpty(Identifier))
                throw new NullReferenceException("Identifier");
            return $@"CREATE PROC usp{TableName}_Insert
                        ({String.Join(",", VariableList)}) AS
                    INSERT INTO {TableName} 
                    ({String.Join(",\n\t",PropertyList)}) 
                    VALUES (
                        {String.Join(",\n\t", varList)}
                    );";
        }
        public static string ObjectToUpdate(Type obj)
        {
            List<string> PropertyList = new List<string>();
            List<string> VariableList = new List<string>();
            var propertyList = obj.GetProperties();
            var TableNameAttribute = obj.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var TableName = TableNameAttribute.TableName ?? obj.Name;
            string Identifier = "";
            foreach (var prop in propertyList)
            {
                if (prop.CustomAttributes.Any(x =>  x.AttributeType == typeof(UniqueIdentiferAttribute)))
                {
                    Identifier = $"{prop.Name} = {prop.GetValue(obj, null).ToString()}";
                }
                else
                {
                    if (!(prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute))))
                    {
                        VariableList.Add($"@{prop.Name} {Enum.GetName(typeof(SqlDbType), prop.PropertyType)}");
                        PropertyList.Add(prop.Name);
                    }
                }
            }
            if (String.IsNullOrEmpty(Identifier))
                throw new NullReferenceException("Identifier");
            return $@"CREATE PROC usp{TableName}_Update
                        ({String.Join(",",VariableList)}) AS
                    Update {TableName} SET {String.Join(",", PropertyList)} WHERE {Identifier} ;";
        }
        public static string ObjectToDelete(Type obj)
        {
            string ActiveIdentifier = "";
            List<string> PropertyList = new List<string>();
            List<string> VariableList = new List<string>();
            var propertyList = obj.GetProperties();
            var TableNameAttribute = obj.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var TableName = TableNameAttribute.TableName ?? obj.Name;
            string Identifier = "";
            foreach (var prop in propertyList)
            {
                if (prop.CustomAttributes.Any(x => x.AttributeType == typeof(UniqueIdentiferAttribute)))
                {
                    Identifier = $"{prop.Name} = {prop.GetValue(obj, null).ToString()}";
                }
                else
                {
                    if (!(prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute))))
                    {
                        VariableList.Add($"@{prop.Name} {Enum.GetName(typeof(SqlDbType), prop.PropertyType)}");
                        PropertyList.Add(prop.Name);
                        if (prop.PropertyType == typeof(bool) &&
                        prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute) &&
                                                x.AttributeType == typeof(ActiveIdentifierAttribute)))
                            ActiveIdentifier = $"{prop.Name} = 0";
                    }
                }
            }
            if (String.IsNullOrEmpty(Identifier))
                throw new NullReferenceException("Identifier");
            return $@"CREATE PROC usp{TableName}_Update
                        ({String.Join(",", VariableList)}) AS
                    Update {TableName} SET {ActiveIdentifier} = 0 WHERE {Identifier} ;";
        }
        public static string ObjectToRead(Type obj)
        {
            List<string> PropertyList = new List<string>();
            List<string> ValueList = new List<string>();
            var propertyList = obj.GetProperties();
            var TableNameAttribute = obj.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var TableName = TableNameAttribute.TableName ?? obj.Name;
            string Identifier = "";
            SqlDbType IdentifierType = SqlDbType.NVarChar;
            PropertyInfo Id;
            foreach (var prop in propertyList)
            {
                if (prop.CustomAttributes.Any(x => x.AttributeType == typeof(UniqueIdentiferAttribute)))
                {
                    Id = prop;
                    Identifier = $"{prop.Name}";
                    IdentifierType = SqlHelper.GetDbType(prop.PropertyType);
                }
                if (!(prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute) )))
                {
                    PropertyList.Add(prop.Name);
                }
            }
            
            if (String.IsNullOrEmpty(Identifier))
                throw new NullReferenceException("Identifier");
           
            return $@"CREATE PROC usp{TableName}_Read 
                        (@{Identifier} {Enum.GetName(typeof(SqlDbType), IdentifierType)}) 
                    AS
                    SELECT {String.Join(",", PropertyList)} FROM {TableName} WHERE @{Identifier} = {Identifier};";
        }
        public static string ObjectToReadAll(Type obj)
        {
            List<string> PropertyList = new List<string>();
            List<string> ValueList = new List<string>();
            var propertyList = obj.GetProperties();
            var TableNameAttribute = obj.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var TableName = TableNameAttribute.TableName ?? obj.Name;
            string Identifier = "";
            foreach (var prop in propertyList)
            {
                if (!(prop.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreAttribute))))
                {
                    PropertyList.Add(prop.Name);
                }
            }

            if (String.IsNullOrEmpty(Identifier))
                throw new NullReferenceException("Identifier");

            return $@" CREATE PROC usp{TableName}_ReadAll AS
                        SELECT {String.Join(",", PropertyList)} FROM {TableName};";
        }
    }
    public class IgnoreAttribute : Attribute
    {
        public bool Ignore { get; }
        public IgnoreAttribute() => this.Ignore = true;
    }
    public class ActiveIdentifierAttribute : Attribute
    {
        public bool ActiveIdentifier { get; }
        public ActiveIdentifierAttribute() => this.ActiveIdentifier = true;
    }
    public class UniqueIdentiferAttribute : Attribute
    {
        public bool UniqueIdentifier { get; }
        public UniqueIdentiferAttribute() => this.UniqueIdentifier = true;
    }
    public class TableAttribute : Attribute
    {
        public string TableName { get; }
        public TableAttribute(string Name) => this.TableName = Name;
    }
    /// <summary>
    /// Represents a series of SQL helper classes
    /// </summary>
    public static class SqlHelper
    {
        private static Dictionary<Type, SqlDbType> typeMap;

        // Create and populate the dictionary in the static constructor
        static SqlHelper()
        {
            typeMap = new Dictionary<Type, SqlDbType>
            {
                [typeof(string)] = SqlDbType.NVarChar,
                [typeof(char[])] = SqlDbType.NVarChar,
                [typeof(byte)] = SqlDbType.TinyInt,
                [typeof(short)] = SqlDbType.SmallInt,
                [typeof(int)] = SqlDbType.Int,
                [typeof(long)] = SqlDbType.BigInt,
                [typeof(byte[])] = SqlDbType.Image,
                [typeof(bool)] = SqlDbType.Bit,
                [typeof(DateTime)] = SqlDbType.DateTime2,
                [typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset,
                [typeof(decimal)] = SqlDbType.Money,
                [typeof(float)] = SqlDbType.Real,
                [typeof(double)] = SqlDbType.Float,
                [typeof(TimeSpan)] = SqlDbType.Time
            };
        }

        /// <summary>
        /// Gets equivelant SQLDataType from specified Type
        /// </summary>
        /// <param name="giveType"></param>
        /// <returns>SqlDataType</returns>
        public static SqlDbType GetDbType(this Type giveType)
        {
            // Allow nullable types to be handled
            giveType = Nullable.GetUnderlyingType(giveType) ?? giveType;

            if (typeMap.ContainsKey(giveType))
            {
                return typeMap[giveType];
            }

            throw new ArgumentException($"{giveType.FullName} is not a supported .NET class");
        }

        /// <summary>
        /// Gets SqlDBType from Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>SQLDataType</returns>
        public static SqlDbType GetDbType<T>()
        {
            return GetDbType(typeof(T));
        }
    }
}

