// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.Mvvm.UnitTests;

[TestClass]
public class Test_AsyncRelayCommandOfT
{
    [TestMethod]
    public async Task Test_AsyncRelayCommandOfT_AlwaysEnabled()
    {
        int ticks = 0;

        AsyncRelayCommand<string>? command = new(async s =>
        {
            await Task.Delay(1000);
            ticks = int.Parse(s!);
            await Task.Delay(1000);
        });

        Assert.IsTrue(command.CanExecute(null));
        Assert.IsTrue(command.CanExecute("1"));

        (object?, EventArgs?) args = default;

        command.CanExecuteChanged += (s, e) => args = (s, e);

        command.NotifyCanExecuteChanged();

        Assert.AreSame(args.Item1, command);
        Assert.AreSame(args.Item2, EventArgs.Empty);

        Assert.IsNull(command.ExecutionTask);
        Assert.IsFalse(command.IsRunning);

        Task task = command.ExecuteAsync((object)"42");

        Assert.IsNotNull(command.ExecutionTask);
        Assert.AreSame(command.ExecutionTask, task);
        Assert.IsTrue(command.IsRunning);

        await task;

        Assert.IsFalse(command.IsRunning);

        Assert.AreEqual(ticks, 42);

        command.Execute("2");

        await command.ExecutionTask!;

        Assert.AreEqual(ticks, 2);

        _ = Assert.ThrowsException<InvalidCastException>(() => command.Execute(new object()));
    }

    [TestMethod]
    public void Test_AsyncRelayCommandOfT_WithCanExecuteFunctionTrue()
    {
        int ticks = 0;

        AsyncRelayCommand<string>? command = new(
            s =>
            {
                ticks = int.Parse(s!);
                return Task.CompletedTask;
            }, s => true);

        Assert.IsTrue(command.CanExecute(null));
        Assert.IsTrue(command.CanExecute("1"));

        command.Execute("42");

        Assert.AreEqual(ticks, 42);

        command.Execute("2");

        Assert.AreEqual(ticks, 2);
    }

    [TestMethod]
    public void Test_AsyncRelayCommandOfT_WithCanExecuteFunctionFalse()
    {
        int ticks = 0;

        AsyncRelayCommand<string>? command = new(
            s =>
            {
                ticks = int.Parse(s!);
                return Task.CompletedTask;
            }, s => false);

        Assert.IsFalse(command.CanExecute(null));
        Assert.IsFalse(command.CanExecute("1"));

        command.Execute("2");

        Assert.AreEqual(ticks, 0);

        command.Execute("42");

        Assert.AreEqual(ticks, 0);
    }

    [TestMethod]
    public void Test_AsyncRelayCommandOfT_NullWithValueType()
    {
        int n = 0;

        AsyncRelayCommand<int>? command = new(i =>
        {
            n = i;
            return Task.CompletedTask;
        });

        Assert.IsFalse(command.CanExecute(null));
        _ = Assert.ThrowsException<NullReferenceException>(() => command.Execute(null));

        command = new AsyncRelayCommand<int>(
            i =>
            {
                n = i;
                return Task.CompletedTask;
            }, i => i > 0);

        Assert.IsFalse(command.CanExecute(null));
        _ = Assert.ThrowsException<NullReferenceException>(() => command.Execute(null));
    }

    [TestMethod]
    public async Task Test_AsyncRelayCommandOfT_WithConcurrencyControl()
    {
        TaskCompletionSource<object?> tcs = new();

        AsyncRelayCommand<string> command = new(async s => await tcs.Task, allowConcurrentExecutions: false);

        Assert.IsTrue(command.CanExecute(null));
        Assert.IsTrue(command.CanExecute("1"));

        (object?, EventArgs?) args = default;

        command.CanExecuteChanged += (s, e) => args = (s, e);

        command.NotifyCanExecuteChanged();

        Assert.AreSame(args.Item1, command);
        Assert.AreSame(args.Item2, EventArgs.Empty);

        Assert.IsNull(command.ExecutionTask);
        Assert.IsFalse(command.IsRunning);

        Task task = command.ExecuteAsync((object)"42");

        Assert.IsNotNull(command.ExecutionTask);
        Assert.AreSame(command.ExecutionTask, task);
        Assert.IsTrue(command.IsRunning);

        // The command can't be executed now, as there's a pending operation
        Assert.IsFalse(command.CanExecute(null));
        Assert.IsFalse(command.CanExecute("2"));

        Assert.IsFalse(command.CanBeCanceled);
        Assert.IsFalse(command.IsCancellationRequested);

        Task newTask = command.ExecuteAsync(null);

        // Execution failed, so a completed task was returned
        Assert.AreSame(newTask, Task.CompletedTask);

        tcs.SetResult(null);

        await task;

        Assert.IsFalse(command.IsRunning);

        _ = Assert.ThrowsException<InvalidCastException>(() => command.Execute(new object()));
    }
}
