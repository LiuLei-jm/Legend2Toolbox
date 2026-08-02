namespace Legend2Toolbox.Api.Hubs;

public record SendAppendRequest(string FilePath,
                                string Content,
                                string LogMessage);
public record SendDeleteRequest(string FilePath,
                                string Content,
                                string LogMessage);

public record SendDeleteListRequest(string FilePath,
                                    List<string> ContentList,
                                    string LogMessage);
