/*
 * Original code : JsonFX http://www.jsonfx.net/license/
 * This Class is under below license.
 * ----------------------------------------------------
 * Distributed under the terms of an MIT-style license:
 * 
 * The MIT License
 * Copyright (c) 2006-2009 Stephen M. McKamey
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JsonFx.Json;
using UnityEngine.Internal;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util.Json
{
    /// <summary>
    /// JsonFx.Json.JsonWriterのサブクラス.
    /// 変更箇所
    /// ・整形後に余計な改行を含ませない
    /// ・null値のデータを無視
    /// </summary>
    public class CustomJsonWriter: JsonFx.Json.JsonWriter
    {
        public bool ignoreNull;
        public CustomJsonWriter(Stream output, JsonWriterSettings settings) :base (output, settings) {
        }
        private int depth = 0;

        protected override void Write(object value, bool isProperty)
        {
            var settings = Settings;
            if (isProperty && settings.PrettyPrint) {
                TextWriter.Write(' ');
            }
            if (value == null) {
                TextWriter.Write("null");
                return;
            }
            if (value is IJsonSerializable) {
                try {
                    if (isProperty) {
                        this.depth++;
                        this.WriteLine();
                    }
                    ((IJsonSerializable)value).WriteJson(this);
                }
                finally {
                    if (isProperty) {
                        this.depth--;
                    }
                }
                return;
            }
            if (value is Enum) {
                this.Write((Enum)value);
                return;
            }
            Type type = value.GetType();
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    TextWriter.Write("null");
                    return;
                case TypeCode.Boolean:
                    this.Write((bool)value);
                    return;
                case TypeCode.Char:
                    this.Write((char)value);
                    return;
                case TypeCode.SByte:
                    this.Write((sbyte)value);
                    return;
                case TypeCode.Byte:
                    this.Write((byte)value);
                    return;
                case TypeCode.Int16:
                    this.Write((short)value);
                    return;
                case TypeCode.UInt16:
                    this.Write((ushort)value);
                    return;
                case TypeCode.Int32:
                    this.Write((int)value);
                    return;
                case TypeCode.UInt32:
                    this.Write((uint)value);
                    return;
                case TypeCode.Int64:
                    this.Write((long)value);
                    return;
                case TypeCode.UInt64:
                    this.Write((ulong)value);
                    return;
                case TypeCode.Single:
                    this.Write((float)value);
                    return;
                case TypeCode.Double:
                    this.Write((double)value);
                    return;
                case TypeCode.Decimal:
                    this.Write((decimal)value);
                    return;
                case TypeCode.DateTime:
                    this.Write((DateTime)value);
                    return;
                case TypeCode.String:
                    this.Write((string)value);
                    return;
            }
            if (value is Guid) {
                this.Write((Guid)value);
                return;
            }
            var uri = value as Uri;
            if (uri != null) {
                this.Write(uri);
                return;
            }
            if (value is TimeSpan) {
                this.Write((TimeSpan)value);
                return;
            }
            var version = value as Version;
            if (version != null) {
                this.Write(version);
                return;
            }
            var dictionary = value as IDictionary;
            if (dictionary != null) {
                try {
                    if (isProperty) {
                        //this.depth++;
                        this.WriteLine();
                    }
                    this.WriteObject(dictionary);
                }
                finally {
                    if (isProperty) {
                        //this.depth--;
                    }
                }
                return;
            }
            if (type.GetInterface("System.Collections.Generic.IDictionary`2") != null) {
                try {
                    if (isProperty) {
                        this.depth++;
                        this.WriteLine();
                    }
                    this.WriteDictionary((IEnumerable)value);
                }
                finally {
                    if (isProperty) {
                        this.depth--;
                    }
                }
                return;
            }
            var enumerable = value as IEnumerable;
            if (enumerable != null) {
                this.WriteArray(enumerable);
                return;
            }
            try {
                if (isProperty) {
                    //this.depth++;
                    this.WriteLine();
                }
                this.WriteObject(value, type);
            }
            finally {
                if (isProperty) {
                    //this.depth--;
                }
            }
        }
        protected override void WriteArray(IEnumerable value)
        {
            bool flag = false;
            TextWriter.Write('[');
            depth++;
            try {
                WriteLine();
                foreach (object current in value) {
                    if (flag) {
                        this.WriteArrayItemDelim();
                        this.WriteLine();
                    } else {
                        flag = true;
                    }
                    this.WriteArrayItem(current);
                }
            } finally {
                depth--;
            }
            if (flag) {
                this.WriteLine();
            }
            TextWriter.Write(']');
        }
        protected void WriteTab() {
            TextWriter.Write(Settings.Tab);
        }
        protected override void WriteDictionary(IEnumerable value)
        {
            var dicEnum = value.GetEnumerator() as IDictionaryEnumerator;
            if (dicEnum == null) {
                throw new JsonSerializationException(string.Format("Types which implement Generic IDictionary<TKey, TValue> must have an IEnumerator which implements IDictionaryEnumerator. ({0})", value.GetType()));
            }
            bool writeDelim = false;
            TextWriter.Write('{');
            WriteTab();
            depth++;
            try {
                // WriteLine();
                while (dicEnum.MoveNext()) {
                     this.WriteObjectProperty(Convert.ToString(dicEnum.Entry.Key), dicEnum.Entry.Value, 
                                                            () => {
                                                                if (writeDelim) {
                                                                    WriteObjectPropertyDelim();
                                                                    WriteLine();
                                                                } else {
                                                                    writeDelim = true;
                                                                }
                                                            });
                }
            } finally {
                depth--;
            }
            if (writeDelim) this.WriteLine();
            TextWriter.Write('}');
        }

        protected bool WriteObjectProperty(string key, object value, Action act)
        {
            // 値が存在する場合にのみ出力
            if (!ignoreNull || value != null) {
                if (act != null) act();

                //this.WriteLine();
                this.WriteObjectPropertyName(key);
                TextWriter.Write(':');
                this.WriteObjectPropertyValue(value);
                return true;
            }
            return false;
        }
        protected override void WriteObject(object value, Type type)
        {
            bool writeDelim = false;
            TextWriter.Write('{');
            WriteTab();
            depth++;
            try {
                if (!string.IsNullOrEmpty(Settings.TypeHintName)) {
                    this.WriteObjectProperty(Settings.TypeHintName, type.FullName + ", " + type.Assembly.GetName().Name,
                                                                        () =>{
                                                                            if (writeDelim) {
                                                                                this.WriteObjectPropertyDelim();
                                                                            } else {
                                                                                writeDelim = true;
                                                                            }});
                }
                bool isAnonymous = type.IsGenericType && type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal);
                foreach (var propertyInfo in type.GetProperties()) {
                    if (propertyInfo.CanRead) {
                        if (propertyInfo.CanWrite || isAnonymous) {
                            if (!this.IsIgnored(type, propertyInfo, value)) {
                                object val = propertyInfo.GetValue(value, null);
                                if (!this.IsDefaultValue(propertyInfo, val)) {
                                    string key = JsonNameAttribute.GetJsonName(propertyInfo);
                                    if (string.IsNullOrEmpty(key)) {
                                        key = propertyInfo.Name;
                                    }
                                    this.WriteObjectProperty(key, val,
                                                             () =>{
                                                                 if (writeDelim) {
                                                                     this.WriteObjectPropertyDelim();
                                                                     //this.WriteLine();
                                                                 } else {
                                                                     writeDelim = true;
                                                                 }});
                                }
                            }
                        }
                    }
                }
                foreach (var fieldInfo in type.GetFields()) {
                    if (fieldInfo.IsPublic && !fieldInfo.IsStatic) {
                        if (!this.IsIgnored(type, fieldInfo, value)) {
                            object val = fieldInfo.GetValue(value);
                            if (!this.IsDefaultValue(fieldInfo, val)) {
                                string key = JsonNameAttribute.GetJsonName(fieldInfo);
                                if (string.IsNullOrEmpty(key)) {
                                    key = fieldInfo.Name;
                                }
                                this.WriteObjectProperty(key, val,
                                                         () =>{
                                                             if (writeDelim) {
                                                                 this.WriteObjectPropertyDelim();
                                                                 this.WriteLine();
                                                             } else {
                                                                 writeDelim = true;
                                                             }});
                            }
                        }
                    }
                }
            } finally {
                depth--;
            }
            if (writeDelim) {
                this.WriteLine();
            }
            TextWriter.Write('}');
        }

        protected override void WriteLine()
        {
            if (!Settings.PrettyPrint) return;

            TextWriter.WriteLine();
            for (int i = 0; i < depth; i++) {
                WriteTab();
            }
        }

        private bool IsIgnored(Type objType, MemberInfo member, object obj)
        {
            if (JsonIgnoreAttribute.IsJsonIgnore(member)) return true;

            var jsonSpecifiedProperty = JsonSpecifiedPropertyAttribute.GetJsonSpecifiedProperty(member);
            if (!string.IsNullOrEmpty(jsonSpecifiedProperty)) {
                PropertyInfo property = objType.GetProperty(jsonSpecifiedProperty);
                if (property != null) {
                    object value = property.GetValue(obj, null);
                    if (value is bool && !Convert.ToBoolean(value)) {
                        return true;
                    }
                }
            }
            if (Settings.UseXmlSerializationAttributes) {
                if (JsonIgnoreAttribute.IsXmlIgnore(member)) {
                    return true;
                }
                PropertyInfo property2 = objType.GetProperty(member.Name + "Specified");
                if (property2 != null) {
                    object value2 = property2.GetValue(obj, null);
                    if (value2 is bool && !Convert.ToBoolean(value2)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsDefaultValue(MemberInfo member, object value)
        {
            var defaultValAttr = Attribute.GetCustomAttribute(member, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
            if (defaultValAttr == null) {
                return false;
            }
            if (defaultValAttr.Value == null) {
                return value == null;
            }
            return defaultValAttr.Value.Equals(value);
        }
    }
}
