﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sprache;

namespace CoreScript.Tokens
{
    public static class ParserExtension
    {
        public static string Text(this IEnumerable<char> item)
        {
            return new string(item.ToArray());
        }

        public static string Text(this IEnumerable<string> item)
        {
            var arr = item as string[] ?? item.ToArray();
            if (arr.Length == 1) return arr[0];

            var size = arr.Select(it => it?.Length ?? 0).Sum();
            var sb = new StringBuilder(size);
            foreach (var s in arr)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static IList<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> item)
        {
            return item.SelectMany(it => it).ToList();
        }

        public static T GetOrDefault<T>(this IOption<T> option,Func<T> func)
        {
            if (option.IsDefined && !option.IsEmpty)
                return option.Get();
            return func();
        }
        public static IEnumerable<T> ToArray<T>(this IOption<T> option)
        {
            if  (option.IsDefined && !option.IsEmpty)
                return new T[] {option.Get()};
            return new T[0];
        }
        public static IEnumerable<T> ToArray<T>(this IOption<IEnumerable<T>> option)
        {
            if (option.IsDefined && !option.IsEmpty)
                return option.Get();
            return new T[0];
        }


        public static IEnumerable<string> Concat(this char chr, IEnumerable<string> str)
        {
            return new[] { chr.ToString() }.Concat(str);
        }

        public static IEnumerable<string> Concat(this IEnumerable<char> chr, IEnumerable<string> str)
        {
            return new[] {chr.Text()}.Concat(str);
        }
        public static IEnumerable<string> Concat(this IEnumerable<string> strs, char chr)
        {
            return strs.Concat(new[] { chr.ToString() });
        }
        public static IEnumerable<string> Concat(this IEnumerable<string> strs, IEnumerable<char> chrs)
        {
            return strs.Concat(new[] { chrs.Text() });
        }

        
        /// <summary>
        /// 匹配的结果类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="first"></param>
        /// <returns></returns>
        public static Parser<U> ThenCast<T, U>(this Parser<T> first) where U:T
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));

            return i =>
            {
                var result = first(i);
                if (result.WasSuccessful && result.Value is U val)
                {
                    return Parse.Return(val)(result.Remainder);
                }

                return Result.Failure<U>(result.Remainder, result.Message, result.Expectations);
            };
        }

        /// <summary>
        /// 截断集合
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lastItemFunc">为true时终止阶段</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> Slice<T>(this IEnumerable<T> list,Func<T,bool> lastItemFunc)
        {
            var result = new List<T>();
            foreach (var item in list)
            {
                if (lastItemFunc(item))
                {
                    result.Add(item);
                    break;
                }
                result.Add(item);
            }

            return result;
        }

    }
}