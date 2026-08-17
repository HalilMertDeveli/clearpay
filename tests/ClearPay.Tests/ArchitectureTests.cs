using System.Xml.Linq;
using FluentAssertions;

namespace ClearPay.Tests;

public sealed class ArchitectureTests
{
    private static readonly string Root = FindRepoRoot();

    [Fact]
    public void Domain_csproj_has_no_packages_or_project_references()
    {
        var xml = XDocument.Load(Path.Combine(Root, "src", "ClearPay.Domain", "ClearPay.Domain.csproj"));
        xml.Descendants("PackageReference").Should().BeEmpty();
        xml.Descendants("ProjectReference").Should().BeEmpty();
    }

    [Fact]
    public void Application_references_only_domain()
    {
        var refs = ProjectRefs("src", "ClearPay.Application", "ClearPay.Application.csproj");
        refs.Should().ContainSingle()
            .Which.Replace('\\', '/').Should().Contain("ClearPay.Domain/ClearPay.Domain.csproj");
        refs.Should().NotContain(r => r.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase)
            || r.Contains("ClearPay.Web", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Infrastructure_does_not_reference_web()
    {
        var refs = ProjectRefs("src", "ClearPay.Infrastructure", "ClearPay.Infrastructure.csproj");
        refs.Should().Contain(r => r.Contains("ClearPay.Application", StringComparison.Ordinal));
        refs.Should().NotContain(r => r.Contains("ClearPay.Web", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Web_references_application_and_infrastructure_not_domain_directly()
    {
        var refs = ProjectRefs("src", "ClearPay.Web", "ClearPay.Web.csproj");
        refs.Should().HaveCount(2);
        refs.Should().Contain(r => r.Contains("ClearPay.Application", StringComparison.Ordinal));
        refs.Should().Contain(r => r.Contains("ClearPay.Infrastructure", StringComparison.Ordinal));
        refs.Should().NotContain(r => r.Contains("ClearPay.Domain", StringComparison.Ordinal));
    }

    [Fact]
    public void Domain_source_does_not_import_ef_or_aspnet()
    {
        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(Root, "src", "ClearPay.Domain"), "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            text.Should().NotContain("Microsoft.EntityFrameworkCore");
            text.Should().NotContain("Microsoft.AspNetCore");
        }
    }

    [Fact]
    public void Web_pages_do_not_new_gateways_or_run_ledger_math()
    {
        var pages = Path.Combine(Root, "src", "ClearPay.Web", "Pages");
        foreach (var file in Directory.EnumerateFiles(pages, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            text.Should().NotContain("RestBankGateway");
            text.Should().NotContain("SoapBankGateway");
            text.Should().NotContain("LedgerPair");
            text.Should().NotContain("ClearPayDbContext");
            text.Should().NotContain("UseSqlServer");
            text.Should().NotContain("SqlOptions");
            text.Should().NotContain("StackExchange.Redis");
            text.Should().NotContain("IConnectionMultiplexer");
            text.Should().NotContain("RabbitMQ.Client");
        }

        var program = File.ReadAllText(Path.Combine(Root, "src", "ClearPay.Web", "Program.cs"));
        program.Should().NotContain("SqlOptions");
        program.Should().NotContain("new RestBankGateway");
    }

    [Fact]
    public void Domain_contains_no_dart()
    {
        var domain = Path.Combine(Root, "src", "ClearPay.Domain");
        Directory.EnumerateFiles(domain, "*.dart", SearchOption.AllDirectories).Should().BeEmpty();
        File.ReadAllText(Path.Combine(Root, "ClearPay.slnx")).Should().NotContain("mobile");
    }

    private static IReadOnlyList<string> ProjectRefs(params string[] relativeCsproj)
    {
        var xml = XDocument.Load(Path.Combine(new[] { Root }.Concat(relativeCsproj).ToArray()));
        return xml.Descendants("ProjectReference")
            .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
            .ToList();
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "docs", "ARCHITECTURE.md")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Repository root not found from test output directory.");
    }
}
