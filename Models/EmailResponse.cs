using System;
using System.Collections.Generic;

namespace AuthApi.Models;

public class EmailResponse
{
    public string? Message { get; set; }
    public ResponseData? Response { get; set; }
}

public class ResponseData
{
    public IEnumerable<string>? To { get; set; }
    public string? From { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? Status { get; set; }
    public string? Ip { get; set; }
    public IEnumerable<object>? Documents { get; set; }
    public string? Received { get; set; }
    public string? Sent { get; set; }
    public string? Error { get; set; }
    public InfoData? Info { get; set; }
    public string? Id { get; set; }
    public DateTime? Date { get; set; }
    public int? Version { get; set; }
}

public class InfoData
{
    public EnvelopeData? Envelope { get; set; }
    public IEnumerable<object>? Accepted { get; set; }
    public IEnumerable<object>? Rejected { get; set; }
    public string? EnvelopeTime { get; set; }
    public string? MessageTime { get; set; }
    public string? MessageSize { get; set; }
    public string? Response { get; set; }
    public string? MessageId { get; set; }
}

public class EnvelopeData
{
    public string? From { get; set; }
    public IEnumerable<string>? To { get; set; }
}