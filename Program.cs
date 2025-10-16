
using Octokit;
using System.Text.RegularExpressions;

Console.WriteLine("argment:RepositoryPath(Owner/Repo), GitHubToken, offset, issueNum");

string repoPath = args[0];
string token = args[1];
if (args.Length < 2)
{
    Console.WriteLine("Not enough arguments");
    Console.WriteLine("e.g. Owner/Repo hoghogeToken, 0, 100");
    return;
}
int offset = args.Length > 2 ? int.Parse(args[2]) : 0;
int num = args.Length > 3 ? int.Parse(args[3]) : 100;

var client = new GitHubClient(new ProductHeaderValue("IssueColonFormatter"));
client.Credentials = new Credentials(token);

int page = 1;
int pageSize = 100;

var owner = repoPath.Split('/')[0];
var repo = repoPath.Split('/')[1];

while (true)
{
    var options = new ApiOptions
    {
        PageCount = 1,      // 1ページずつ取得
        PageSize = pageSize,
        StartPage = page
    };

    var issues = await client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest
    {
        State = ItemStateFilter.All
    }, options);

    if (issues.Count == 0)
    {
        break;
    }

    foreach (var issue in issues)
    {
        if (!issue.Title.Contains("[FromGitBucket]")) continue;

        Console.WriteLine($"Processing issue #{issue.Number}: {issue.Title}");

        // Issue本文の整形
        var originalBody = issue.Body ?? "";
        var formattedIssueBody = FormatColonSpacePerLine(originalBody);
        if (originalBody != formattedIssueBody)
        {
            var issueUpdate = new IssueUpdate
            {
                Title = issue.Title,
                Body = formattedIssueBody,
            };
            // ラベルやアサインなども維持
            foreach (var label in issue.Labels)
                issueUpdate.AddLabel(label.Name);
            foreach (var assignee in issue.Assignees)
                issueUpdate.AddAssignee(assignee.Login);

            await client.Issue.Update(owner, repo, issue.Number, issueUpdate);
            Console.WriteLine($"Updated issue body in issue #{issue.Number}");
        }

        // コメント取得＆整形
        var comments = await client.Issue.Comment.GetAllForIssue(owner, repo, issue.Number);

        foreach (var comment in comments)
        {
            var commentOriginalBody = comment.Body;
            var formattedCommentBody = FormatColonSpacePerLine(commentOriginalBody);

            if (commentOriginalBody != formattedCommentBody)
            {
                await client.Issue.Comment.Update(owner, repo, comment.Id, formattedCommentBody);
                Console.WriteLine($"Updated comment {comment.Id} in issue #{issue.Number}");
            }
        }
    }

    page++;
}

// 日付表現を除外し、各行の先頭のXXX:Valueのみ変換
static string FormatColonSpacePerLine(string text)
{
    var lines = text.Split('\n');
    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i];

        // 日付表現パターン（"at:"で始まり、日付が続く）
        var datePattern = @"^\s*[A-Za-z ]*at: ?\d{4}/\d{2}/\d{2}";
        if (Regex.IsMatch(line, datePattern))
        {
            // "at:" の後にスペースがなければ追加
            line = Regex.Replace(line, @"^(.*at:)(\d{4}/\d{2}/\d{2})", "$1 $2");

            // 時刻部分 "HH: MM: SS" → "HH:MM:SS"（余計なスペース除去）
            while (Regex.IsMatch(line, @"(\d{1,2}):\s(\d{2})"))
            {
                line = Regex.Replace(line, @"(\d{1,2}):\s(\d{2})", "$1:$2");
            }
            // タイムゾーン "+HH: MM" → "+HH:MM"
            while (Regex.IsMatch(line, @"(\+\d{2}):\s(\d{2})"))
            {
                line = Regex.Replace(line, @"(\+\d{2}):\s(\d{2})", "$1:$2");
            }

            // 時刻部分 "H:MM:SS" → "HH:MM:SS"（ゼロ埋め）
            line = Regex.Replace(line, @"(\d{4}/\d{2}/\d{2}) (\d{1}):(\d{2}):(\d{2})", m =>
            {
                // m.Groups[2] = 時, [3] = 分, [4] = 秒
                var hour = m.Groups[2].Value.PadLeft(2, '0');
                var min = m.Groups[3].Value;
                var sec = m.Groups[4].Value;
                return $"{m.Groups[1].Value} {hour}:{min}:{sec}";
            });

            lines[i] = line;
            continue;
        }

        // 先頭のXXX:Valueのみ変換
        line = Regex.Replace(line, @"^([A-Za-z0-9_\-]+):([^\s])", "$1: $2");
        lines[i] = line;
    }
    return string.Join("\n", lines);
}