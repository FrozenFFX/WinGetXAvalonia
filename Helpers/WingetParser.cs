using System.Text.RegularExpressions;
using WinGetXAvalonia.Models;

namespace WinGetXAvalonia.Helpers;

public static class WingetParser
{
    public static List<PackageModel> Parse(List<string> lines)
    {
        List<PackageModel> list = new();

        foreach (string line in lines.Skip(2))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] p = Regex
                .Split(line.Trim(), @"\s{2,}");

            if (p.Length < 3)
                continue;

            var name = p.ElementAtOrDefault(0) ?? "";

            list.Add(new PackageModel
            {
                Name = name,
                Id = p.ElementAtOrDefault(1) ?? "",
                Version = p.ElementAtOrDefault(2) ?? "",
                Available = p.ElementAtOrDefault(3) ?? "",
                Source = p.ElementAtOrDefault(4) ?? "",
                Icon = IconProvider.Get(name)
            });
        }

        return list;
    }
}
