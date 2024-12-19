using BenchmarkDotNet.Running;
using SqlScribe.BenchMark;

var summary = BenchmarkRunner.Run<DbQueryBenchmark>();