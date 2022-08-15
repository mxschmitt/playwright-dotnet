using System;
using NUnitFrameworkBase = NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Microsoft.Playwright.TestAdapter;

namespace Microsoft.Playwright.NUnit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestAttribute : NUnitFrameworkBase.TestAttribute, IWrapTestMethod
    {
        public TestAttribute() : base() { }

        public TestCommand Wrap(TestCommand command)
            => new RetryTestCommand(command);

        public class RetryTestCommand : DelegatingTestCommand
        {
            public RetryTestCommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                string key = Test.Id;
                while (!TestHarnessStorage.IsLastRun(key))
                {
                    Exception? exception = null;
                    try
                    {
                        context.CurrentResult = innerCommand.Execute(context);
                    }
                    catch (System.Exception exc)
                    {
                        exception = exc;
                    }
                    context.CurrentRepeatCount++;
                    SetNextId(Test);

                    TestHarnessStorage.IncrementRunCount(key);

                    if (context.CurrentResult.ResultState == ResultState.Success)
                        break;
                    if (exception != null && TestHarnessStorage.IsLastRun(key))
                    {
                        throw exception;
                    }
                }
                TestHarnessStorage.ResetRunCount(key);
                return context.CurrentResult;
            }

            internal void SetNextId(Test test)
                => test.Id = GetNextId();

            private static string GetNextId()
                => Test.IdPrefix + unchecked(_nextID++);

            private static int _nextID = 1000;
        }
    }
}

