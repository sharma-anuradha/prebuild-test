// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Throw from a try block nested in finally, non error case

using System;
using Xunit;

namespace Test_throwinfinally_general
{
public class a
{
    private static TestUtil.TestLog testLog;

    static a()
    {
        // Create test writer object to hold expected output
        System.IO.StringWriter expectedOut = new System.IO.StringWriter();

        // Write expected output to string writer object
        expectedOut.WriteLine("In outer try");
        expectedOut.WriteLine("In outer finally");
        expectedOut.WriteLine("In inner try");
        expectedOut.WriteLine("In inner finally");
        expectedOut.WriteLine("Pass");

        // Create and initialize test log object
        testLog = new TestUtil.TestLog(expectedOut);
    }

    public static void MiddleMethod()
    {
        try
        {
            Console.WriteLine("In outer try");
        }
        finally
        {
            Console.WriteLine("In outer finally");
            try
            {
                Console.WriteLine("In inner try");
                throw new System.ArgumentException();
                Console.WriteLine("Unreached");
            }
            finally
            {
                Console.WriteLine("In inner finally");
            }
        }
        Console.WriteLine("Unreached...");
    }

    [Fact]
    public static int TestEntryPoint()
    {
        //Start recording
        testLog.StartRecording();

        try
        {
            MiddleMethod();
        }
        catch
        {
            Console.WriteLine("Pass");
        }

        // stop recoding
        testLog.StopRecording();

        return testLog.VerifyOutput();
    }
}
}
