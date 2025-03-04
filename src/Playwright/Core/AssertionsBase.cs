/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.Helpers;
using Microsoft.Playwright.Transport.Protocol;

namespace Microsoft.Playwright.Core
{
    internal class AssertionsBase
    {
        public AssertionsBase(ILocator actual, bool isNot)
        {
            ActualLocator = (Locator)actual;
            IsNot = isNot;
        }

        protected bool IsNot { get; private set; }

        internal Locator ActualLocator { get; private set; }

        internal async Task ExpectImplAsync(string expression, ExpectedTextValue textValue, object expected, string message, FrameExpectOptions options)
        {
            await ExpectImplAsync(expression, new ExpectedTextValue[] { textValue }, expected, message, options).ConfigureAwait(false);
        }

        internal async Task ExpectImplAsync(string expression, ExpectedTextValue[] expectedText, object expected, string message, FrameExpectOptions options)
        {
            options = options ?? new();
            options.ExpectedText = expectedText;
            options.IsNot = IsNot;
            await ExpectImplAsync(expression, options, expected, message).ConfigureAwait(false);
        }

        internal async Task ExpectImplAsync(string expression, FrameExpectOptions expectOptions, object expected, string message)
        {
            if (expectOptions.Timeout == null)
            {
                expectOptions.Timeout = 5_000;
            }
            if (expectOptions.IsNot)
            {
                message = message.Replace("expected to", "expected not to");
            }
            var result = await ActualLocator.ExpectAsync(expression, expectOptions).ConfigureAwait(false);
            if (result.Matches == IsNot)
            {
                var actual = result.Received;
                var log = string.Join("\n", result.Log);
                if (!string.IsNullOrEmpty(log))
                {
                    log = "\nCall log:\n" + log;
                }
                if (expected == null)
                {
                    throw new PlaywrightException($"{message} {log}");
                }
                throw new PlaywrightException($"{message} '{FormatValue(expected)}'\nBut was: '{FormatValue(actual)}' {log}");
            }
        }

        internal ExpectedTextValue ExpectedRegex(Regex pattern, ExpectedTextValue options = null)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            ExpectedTextValue textValue = options ?? new() { };
            textValue.RegexSource = pattern.ToString();
            textValue.RegexFlags = pattern.Options.GetInlineFlags();
            return textValue;
        }

        internal T ConvertToType<T>(object source)
            where T : new()
        {
            T target = new();
            if (source == null)
            {
                return target;
            }

            var sourceType = source.GetType();
            var targetType = target.GetType();
            foreach (var sourceProperty in sourceType.GetProperties())
            {
                var targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty != null)
                {
                    targetProperty.SetValue(target, sourceProperty.GetValue(source));
                }
            }
            return target;
        }

        internal FrameExpectOptions ConvertToFrameExpectOptions(object source) => ConvertToType<FrameExpectOptions>(source);

        private string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string)
            {
                return (string)value;
            }

            if (value is IEnumerable<object>)
            {
                return "[" + string.Join(", ", ((IEnumerable<object>)value).Select(value => $"'{value}'")) + "]";
            }

            return value.ToString();
        }
    }
}
