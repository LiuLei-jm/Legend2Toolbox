namespace Legend2Toolbox.Api.Hubs;

public record SendAppendRequest(string FilePath,
                                string Content);
public record SendDeleteRequest(string FilePath,
                                string Content);
public record SendDeleteListRequest(string FilePath,
                                    List<string> ContentList);
public record SendSyncUnexpiredCardsListRequest(string FilePath,
                                                List<string> ContentList);
