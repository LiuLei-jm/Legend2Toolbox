using Legend2Toolbox.Application.Feature.Scripts.DTOS;
using Legend2Toolbox.Domain.Enums;

namespace Legend2Toolbox.Api.Endpoints.Script;

public record CreateScriptSetRequest(string Name,
                                     string? Description);

public record UpdateScriptSetRequest(string Name,
                                     string? Description);

public record CreateScriptFileRequest(Guid ScriptSetId,
                                      string FileName,
                                      string FilePath,
                                      ScriptFileType Type,
                                      string? WholeContent,
                                      List<ScriptSegmentDto>? Segments);

public record UpdateScriptFileRequest(
                                      string FileName,
                                      string FilePath,
                                      ScriptFileType Type,
                                      string? WholeContent,
                                      List<ScriptSegmentDto>? Segments);

public record DeleteScriptFileRequest(Guid Id);

public record CreateMaterialFileRequest(Guid ScriptSetId,
                                        IFormFile File,
                                        string TargetPath,
                                        string Password,
                                        long FileSize);

public record UpdateMaterialFileRequest(
                                        IFormFile? File,
                                        string TargetPath,
                                        string Password);

public record DelteMaterialFileRequest(Guid Id);

public record CreateScriptSetDbDataRequest(Guid ScriptSetId,
                                           GameDbTableType TableType,
                                           string Name,
                                           string DataJson);
public record UpdateScriptSetDbDataRequest(
                                           GameDbTableType TableType,
                                           string Name,
                                           string DataJson);
