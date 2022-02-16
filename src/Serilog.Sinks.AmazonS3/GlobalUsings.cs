#pragma warning disable IDE0065 // Die using-Anweisung wurde falsch platziert.
global using System.Globalization;
global using System.Text;
global using System.Text.RegularExpressions;

global using Amazon;
global using Amazon.S3;
global using Amazon.S3.Model;

global using Serilog.Configuration;
global using Serilog.Core;
global using Serilog.Debugging;
global using Serilog.Events;
global using Serilog.Formatting;
global using Serilog.Formatting.Display;
global using Serilog.Sinks.AmazonS3;
global using Serilog.Sinks.PeriodicBatching;
#pragma warning restore IDE0065 // Die using-Anweisung wurde falsch platziert.