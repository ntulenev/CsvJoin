using CsvJoin.Configuration;
using CsvJoin.Models;

namespace CsvJoin.Abstractions.Application;

internal interface IConfiguredJoinJobFactory
{
    ConfiguredJoinJob Create(AppSettings settings);
}
