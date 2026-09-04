namespace Legend2Toolbox.Application.Feature.Scripts.DTOS;

public record ScriptSegmentDto(Guid? Id,
                               string TriggerField,
                               string Content);

public record ScriptSetDbDataDto(
    Guid Id,
    Guid ScriptSetId,
    GameDbTableType TableType,
    string Name,
    string DataJson);

public record ScriptFileDto(
    Guid Id,
    string FileName,
    string FilePath,
    ScriptFileType Type
    );

public record ScriptFileDetailDto(Guid Id,
                                  string FileName,
                                  string FilePath,
                                  ScriptFileType Type,
                                  string? WholeContent,
                                  List<ScriptSegmentDto>? Segments);

public record MaterialFileDto(Guid Id,
                              string FileName,
                              string TargetPath,
                              string Password,
                              long FileSize);

public record ScriptSetDto(
    Guid Id,
    string Name,
    string? Description
    );
