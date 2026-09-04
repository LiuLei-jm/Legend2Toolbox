/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ScriptFileType } from './ScriptFileType';
import type { ScriptSegmentDto } from './ScriptSegmentDto';
export type CreateScriptFileRequest = {
    scriptSetId?: string;
    fileName?: string | null;
    filePath?: string | null;
    type?: ScriptFileType;
    wholeContent?: string | null;
    segments?: Array<ScriptSegmentDto> | null;
};

