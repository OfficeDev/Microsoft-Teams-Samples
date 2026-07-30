import { TeamsActivityHandler } from "botbuilder";
export declare class TeamsBot extends TeamsActivityHandler {
    constructor();
    fileUploadCompleted(context: any, fileConsentCardResponse: any): Promise<void>;
    fetchTextFileContentAsString(url: string): Promise<string>;
    ReadPdfContents(localFilePath: any): Promise<any>;
}
